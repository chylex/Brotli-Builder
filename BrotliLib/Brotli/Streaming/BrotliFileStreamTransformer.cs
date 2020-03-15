using System.Collections.Generic;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.Parameters;

namespace BrotliLib.Brotli.Streaming{
    public class BrotliFileStreamTransformer : IBrotliFileStream{
        public BrotliFileParameters Parameters => original.Parameters;
        public BrotliGlobalState State => transformedState?.Clone() ?? original.State;

        private readonly IBrotliFileStream original;
        private readonly IBrotliTransformer[] transformers;
        private readonly BrotliCompressionParameters compressionParameters;

        private readonly Queue<MetaBlock> transformedQueue;
        private BrotliGlobalState? transformedState;

        public BrotliFileStreamTransformer(IBrotliFileStream original, BrotliCompressionParameters compressionParameters, params IBrotliTransformer[] transformers){
            this.original = original;
            this.transformers = transformers;
            this.compressionParameters = compressionParameters;
            this.transformedQueue = new Queue<MetaBlock>();
        }

        public MetaBlock? NextMetaBlock(){
            if (transformedQueue.Count == 0){
                transformedState = original.State; // update state to match start of the next meta-block

                var metaBlock = original.NextMetaBlock();

                if (metaBlock != null){
                    var (transformedMetaBlocks, newTransformedState) = BrotliEncodePipeline.ApplyTransformerChain(transformedState, metaBlock, compressionParameters, transformers);

                    foreach(var transformedMetaBlock in transformedMetaBlocks){
                        transformedQueue.Enqueue(transformedMetaBlock);
                    }

                    transformedState = newTransformedState;
                }
            }

            return transformedQueue.TryDequeue(out var next) ? next : null;
        }
    }
}
