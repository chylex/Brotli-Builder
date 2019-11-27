using System;
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

        public static BrotliFileStructure FromBytes(byte[] bytes){
            return DoDeserialize(CreateReader(new BitStream(bytes)), new FileContext(BrotliDefaultDictionary.Embedded, windowSize => new BrotliOutputWindowed(windowSize)));
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

        public BitStream Serialize(BrotliSerializationParameters parameters){
            BitStream stream = new BitStream();
            DoSerialize(stream.GetWriter(), this, new FileContext(Parameters.Dictionary, new BrotliOutputWindowed(Parameters.WindowSize)), parameters);
            return stream;
        }

        public BrotliOutputStored GetDecompressionState(BitStream bitStream, MarkerLevel? markerLevel = null){
            IMarkedBitReader reader = CreateReader(bitStream, markerLevel);
            BrotliOutputStored output = new BrotliOutputStored(reader.MarkerRoot);

            DoDeserialize(reader, new FileContext(Parameters.Dictionary, output));

            return output;
        }

        // Serialization

        private class FileContext{
            public BrotliDictionary Dictionary { get; }
            public Func<WindowSize, IBrotliOutput> OutputState { get; }

            public FileContext(BrotliDictionary dictionary, Func<WindowSize, IBrotliOutput> outputState){
                Dictionary = dictionary;
                OutputState = outputState;
            }

            public FileContext(BrotliDictionary dictionary, IBrotliOutput outputState) : this(dictionary, _ => outputState){}
        }

        private static IMarkedBitReader CreateReader(BitStream bitStream, MarkerLevel? markerLevel = null){
            return markerLevel.HasValue && markerLevel != MarkerLevel.None ? new MarkedBitReader(bitStream.GetReader(), markerLevel.Value) : (IMarkedBitReader)new MarkedBitReaderDummy(bitStream.GetReader());
        }

        private static readonly BitDeserializer<BrotliFileStructure, FileContext> DoDeserialize = (reader, context) => {
            WindowSize windowSize = WindowSize.Deserialize(reader, NoContext.Value);

            var parameters = new BrotliFileParameters{
                WindowSize = windowSize,
                Dictionary = context.Dictionary
            };

            var structure = new BrotliFileStructure(parameters);
            var state = new BrotliGlobalState(parameters, context.OutputState(windowSize));

            while(true){
                MetaBlock metaBlock = MetaBlock.Deserialize(reader, state);
                structure.MetaBlocks.Add(metaBlock);

                if (metaBlock.IsLast){
                    break;
                }
            }

            return structure;
        };

        private static readonly BitSerializer<BrotliFileStructure, FileContext, BrotliSerializationParameters> DoSerialize = (writer, obj, context, parameters) => {
            var fileParameters = obj.Parameters;
            var windowSize = fileParameters.WindowSize;
            
            WindowSize.Serialize(writer, windowSize, NoContext.Value);

            var state = new BrotliGlobalState(fileParameters, context.OutputState(windowSize));

            foreach(MetaBlock metaBlock in obj.MetaBlocks){
                MetaBlock.Serialize(writer, metaBlock, state, parameters);
            }
        };
    }
}
