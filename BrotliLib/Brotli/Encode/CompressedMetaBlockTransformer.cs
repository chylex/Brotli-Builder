using System.Collections.Generic;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.State;

namespace BrotliLib.Brotli.Encode{
    public abstract class CompressedMetaBlockTransformer : IBrotliTransformer{
        public IEnumerable<MetaBlock> Transform(MetaBlock original, BrotliGlobalState initialState){
            if (!(original is MetaBlock.Compressed compressed)){
                yield return original;
                yield break;
            }

            foreach(MetaBlock transformed in Transform(compressed, new CompressedMetaBlockBuilder(compressed, initialState), initialState)){
                yield return transformed;
            }
        }

        protected abstract IEnumerable<MetaBlock> Transform(MetaBlock.Compressed original, CompressedMetaBlockBuilder builder, BrotliGlobalState initialState);
    }
}
