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
                    var (transformedMetaBlocks, transformedState) = ApplyTransformerChain(encodeInfo.State, metaBlock, compressionParameters);

                    bfs.MetaBlocks.AddRange(transformedMetaBlocks);
                    encodeInfo = newEncodeInfo.WithState(transformedState);
                }
            }while(!encodeInfo.IsFinished);

            FinalizeStructure(bfs);
            return bfs;
        }

        private (IList<MetaBlock>, BrotliGlobalState) ApplyTransformerChain(BrotliGlobalState originalState, MetaBlock encodedMetaBlock, BrotliCompressionParameters compressionParameters){
            var metaBlocks = new List<MetaBlock>{ encodedMetaBlock };
            var states = new List<BrotliGlobalState>{ originalState };

            foreach(var transformer in Transformers){
                var nextMetaBlocks = new List<MetaBlock>();
                var nextStates = new List<BrotliGlobalState>{ originalState }; // first meta-block starts with original state, second meta-block with the first meta-block's end state, etc.

                for(int index = 0; index < metaBlocks.Count; index++){
                    foreach(var (transformedMetaBlock, transformedState) in transformer.Transform(metaBlocks[index], states[index], compressionParameters)){
                        nextMetaBlocks.Add(transformedMetaBlock);
                        nextStates.Add(transformedState);
                    }
                }

                metaBlocks = nextMetaBlocks;
                states = nextStates;
            }

            return (metaBlocks, states[^1]);
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
