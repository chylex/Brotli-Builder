using System.Linq;
using BrotliLib.Brotli.Parameters;

namespace BrotliLib.Brotli.Encode{
    public sealed class BrotliEncodePipeline{
        private readonly IBrotliEncoder encoder;
        private readonly IBrotliTransformer[] transformers;

        public BrotliEncodePipeline(IBrotliEncoder encoder, params IBrotliTransformer[] transformers){
            this.encoder = encoder;
            this.transformers = transformers.ToArray();
        }

        public BrotliFileStructure Apply(BrotliFileParameters fileParameters, BrotliCompressionParameters compressionParameters, byte[] bytes){
            var bfs = new BrotliFileStructure(fileParameters);
            var encodeInfo = new BrotliEncodeInfo(fileParameters, compressionParameters, bytes);

            do{
                var (metaBlock, newEncodeInfo) = encoder.Encode(encodeInfo);

                if (transformers.Length == 0){
                    bfs.MetaBlocks.Add(metaBlock);
                    encodeInfo = newEncodeInfo;
                }
                else{
                    var state = newEncodeInfo.State;

                    foreach(var transformer in transformers){
                        var (transformedMetaBlocks, transformedState) = transformer.Transform(metaBlock, state, compressionParameters);

                        bfs.MetaBlocks.AddRange(transformedMetaBlocks);
                        state = transformedState;
                    }

                    encodeInfo = encodeInfo.WithState(state);
                }
            }while(!encodeInfo.IsFinished);

            bfs.Fixup();
            return bfs;
        }
    }
}
