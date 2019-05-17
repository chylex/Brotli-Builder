using System.Collections.Generic;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.State;

namespace BrotliImpl.Transformers{
    public class TransformRebuild : CompressedMetaBlockTransformer{
        protected override IEnumerable<MetaBlock> Transform(MetaBlock.Compressed original, CompressedMetaBlockBuilder builder, BrotliGlobalState initialState){
            yield return builder.Build().MetaBlock;
        }
    }
}
