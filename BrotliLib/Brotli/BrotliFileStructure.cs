using System.Collections.Generic;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Dictionary;
using BrotliLib.Brotli.Dictionary.Default;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.Output;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Markers;
using BrotliLib.Markers.Serialization.Reader;
using BrotliLib.Serialization;

namespace BrotliLib.Brotli{
    /// <summary>
    /// Represents the object structure of a Brotli-compressed file.
    /// https://tools.ietf.org/html/rfc7932#section-2
    /// </summary>
    public sealed class BrotliFileStructure{
        public static BrotliFileStructure NewEmpty(){
            var bfs = new BrotliFileStructure(new BrotliFileParameters());
            bfs.MetaBlocks.Add(new MetaBlock.LastEmpty());
            return bfs;
        }

        public static (BrotliFileStructure Structure, MarkerRoot MarkerRoot) FromBytes(byte[] bytes, MarkerLevel markerLevel){
            return FromBytes(new BitStream(bytes), markerLevel);
        }

        public static (BrotliFileStructure Structure, MarkerRoot MarkerRoot) FromBytes(BitStream bits, MarkerLevel markerLevel){
            var reader = CreateReader(bits, markerLevel);
            var structure = DoDeserialize(reader, BrotliDefaultDictionary.Embedded);

            return (structure, reader.MarkerRoot);
        }

        public static BrotliFileStructure FromEncoder(BrotliFileParameters fileParameters, BrotliCompressionParameters compressionParameters, byte[] bytes, IBrotliEncoder encoder, params IBrotliTransformer[] transformers){
            return new BrotliEncodePipeline(encoder, transformers).Apply(fileParameters, compressionParameters, bytes);
        }

        // Data

        public BrotliFileParameters Parameters { get; set; }
        public List<MetaBlock> MetaBlocks { get; }
        
        public BrotliFileStructure(BrotliFileParameters parameters){
            this.Parameters = parameters;
            this.MetaBlocks = new List<MetaBlock>();
        }

        public BrotliFileStructure Transform(IBrotliTransformer transformer, BrotliCompressionParameters compressionParameters){
            var copy = new BrotliFileStructure(Parameters);
            var state = new BrotliGlobalState(Parameters, new BrotliOutputWindowed(Parameters.WindowSize));

            foreach(MetaBlock original in MetaBlocks){
                var (transformedMetaBlocks, transformedState) = transformer.Transform(original, state, compressionParameters);

                copy.MetaBlocks.AddRange(transformedMetaBlocks);
                state = transformedState;
            }

            copy.Fixup();
            return copy;
        }

        public void Fixup(){
            MetaBlocks.RemoveAll(mb => mb is MetaBlock.LastEmpty);

            if (MetaBlocks.Count == 0 || MetaBlocks[^1] is MetaBlock.Uncompressed){
                MetaBlocks.Add(new MetaBlock.LastEmpty());
            }

            for(int index = 0, last = MetaBlocks.Count - 1; index <= last; index++){
                MetaBlocks[index].IsLast = index == last;
            }
        }

        public BrotliOutputStored Decompress(){
            var output = new BrotliOutputStored();
            var state = new BrotliGlobalState(Parameters, output);

            foreach(var metaBlock in MetaBlocks){
                metaBlock.Decompress(state);
            }

            return output;
        }

        public BitStream Serialize(BrotliSerializationParameters parameters){
            BitStream stream = new BitStream();
            DoSerialize(stream.GetWriter(), this, NoContext.Value, parameters);
            return stream;
        }

        // Serialization

        private static IMarkedBitReader CreateReader(BitStream bitStream, MarkerLevel? markerLevel = null){
            return markerLevel.HasValue && markerLevel != MarkerLevel.None ? new MarkedBitReader(bitStream.GetReader(), markerLevel.Value) : (IMarkedBitReader)new MarkedBitReaderDummy(bitStream.GetReader());
        }

        private static readonly BitDeserializer<BrotliFileStructure, BrotliDictionary> DoDeserialize = (reader, context) => {
            WindowSize windowSize = WindowSize.Deserialize(reader, NoContext.Value);

            var parameters = new BrotliFileParameters{
                WindowSize = windowSize,
                Dictionary = context
            };

            var structure = new BrotliFileStructure(parameters);
            var state = new BrotliGlobalState(parameters, new BrotliOutputWindowed(windowSize));

            while(true){
                MetaBlock metaBlock = MetaBlock.Deserialize(reader, state);
                structure.MetaBlocks.Add(metaBlock);

                if (metaBlock.IsLast){
                    break;
                }
            }

            return structure;
        };

        private static readonly BitSerializer<BrotliFileStructure, NoContext, BrotliSerializationParameters> DoSerialize = (writer, obj, context, parameters) => {
            var fileParameters = obj.Parameters;
            var windowSize = fileParameters.WindowSize;
            
            WindowSize.Serialize(writer, windowSize, NoContext.Value);

            var state = new BrotliGlobalState(fileParameters, new BrotliOutputWindowed(windowSize));

            foreach(MetaBlock metaBlock in obj.MetaBlocks){
                MetaBlock.Serialize(writer, metaBlock, state, parameters);
            }
        };
    }
}
