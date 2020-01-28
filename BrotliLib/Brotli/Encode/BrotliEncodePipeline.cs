using System.Collections.Generic;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Dictionary;
using BrotliLib.Brotli.Parameters;

namespace BrotliLib.Brotli.Encode{
    public abstract class BrotliEncodePipeline{
        protected List<IBrotliTransformer> Transformers { get; } = new List<IBrotliTransformer>();

        protected virtual WindowSize DetermineWindowSize(byte[] bytes){
            return WindowSize.Default;
        }

        protected virtual BrotliCompressionParameters SetupCompressionParameters(byte[] bytes){
            return BrotliCompressionParameters.Default;
        }

        protected abstract IBrotliEncoder CreateEncoder(byte[] bytes, BrotliFileParameters fileParameters);

        protected virtual void FinalizeStructure(BrotliFileStructure structure){
            structure.Fixup();
        }

        public BrotliFileStructure Apply(byte[] bytes, BrotliDictionary dictionary){
            var fileParameters = new BrotliFileParameters.Builder{
                WindowSize = DetermineWindowSize(bytes),
                Dictionary = dictionary
            }.Build();

            var compressionParameters = SetupCompressionParameters(bytes);

            var bfs = new BrotliFileStructure(fileParameters);
            var encoder = CreateEncoder(bytes, fileParameters);
            var encodeInfo = new BrotliEncodeInfo(fileParameters, compressionParameters, bytes);

            do{
                var (metaBlock, newEncodeInfo) = encoder.Encode(encodeInfo);

                if (Transformers.Count == 0){
                    bfs.MetaBlocks.Add(metaBlock);
                    encodeInfo = newEncodeInfo;
                }
                else{
                    var state = newEncodeInfo.State;

                    foreach(var transformer in Transformers){
                        var (transformedMetaBlocks, transformedState) = transformer.Transform(metaBlock, state, compressionParameters);

                        bfs.MetaBlocks.AddRange(transformedMetaBlocks);
                        state = transformedState;
                    }

                    encodeInfo = encodeInfo.WithState(state);
                }
            }while(!encodeInfo.IsFinished);

            FinalizeStructure(bfs);
            return bfs;
        }

        public class Simple : BrotliEncodePipeline{
            private readonly WindowSize windowSize;
            private readonly BrotliCompressionParameters compressionParameters;
            private readonly IBrotliEncoder encoder;

            public Simple(WindowSize windowSize, BrotliCompressionParameters compressionParameters, IBrotliEncoder encoder, params IBrotliTransformer[] transformers){
                this.windowSize = windowSize;
                this.compressionParameters = compressionParameters;

                this.encoder = encoder;
                this.Transformers.AddRange(transformers);
            }

            protected override WindowSize DetermineWindowSize(byte[] bytes){
                return windowSize;
            }

            protected override BrotliCompressionParameters SetupCompressionParameters(byte[] bytes){
                return compressionParameters;
            }

            protected override IBrotliEncoder CreateEncoder(byte[] bytes, BrotliFileParameters fileParameters){
                return encoder;
            }
        }
    }
}
