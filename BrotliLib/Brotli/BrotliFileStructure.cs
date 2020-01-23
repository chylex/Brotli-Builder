using System.Collections.Generic;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Dictionary;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.Output;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Markers;
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

        public static (BrotliFileStructure Structure, MarkerRoot MarkerRoot) FromBytes(byte[] bytes, MarkerLevel markerLevel, BrotliDictionary? dictionary = null){
            return FromBytes(new BitStream(bytes), markerLevel, dictionary);
        }

        public static (BrotliFileStructure Structure, MarkerRoot MarkerRoot) FromBytes(BitStream bits, MarkerLevel markerLevel, BrotliDictionary? dictionary = null){
            var reader = BrotliFileReader.FromBytes(bits, markerLevel, dictionary);
            var bfs = new BrotliFileStructure(reader.Parameters);

            MetaBlock? metaBlock;

            while((metaBlock = reader.NextMetaBlock()) != null){
                bfs.MetaBlocks.Add(metaBlock);
            }

            return (bfs, reader.MarkerRoot);
        }

        public static BrotliFileStructure FromEncoder(BrotliFileParameters fileParameters, BrotliCompressionParameters compressionParameters, byte[] bytes, IBrotliEncoder encoder, params IBrotliTransformer[] transformers){
            return new BrotliEncodePipeline(encoder, transformers).Apply(fileParameters, compressionParameters, bytes);
        }

        // Instance

        public BrotliFileParameters Parameters { get; set; }
        public List<MetaBlock> MetaBlocks { get; }
        
        public BrotliFileStructure(BrotliFileParameters parameters){
            this.Parameters = parameters;
            this.MetaBlocks = new List<MetaBlock>();
        }

        public BrotliFileStructure Transform(IBrotliTransformer transformer, BrotliCompressionParameters compressionParameters){
            var copy = new BrotliFileStructure(Parameters);
            var state = new BrotliGlobalState(Parameters);

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

        public BitStream Serialize(BrotliSerializationParameters serializationParameters){
            var stream = new BitStream();
            var writer = stream.GetWriter();

            var state = new BrotliGlobalState(Parameters);

            WindowSize.Serialize(writer, Parameters.WindowSize, NoContext.Value);

            foreach(MetaBlock metaBlock in MetaBlocks){
                MetaBlock.Serialize(writer, metaBlock, state, serializationParameters);
            }

            return stream;
        }
    }
}
