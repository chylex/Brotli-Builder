using System.Collections.Generic;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.State;

namespace BrotliLib.Brotli.Encode.Impl{
    public class TransformRebuild : CompressedMetaBlockTransformer{
        protected override IEnumerable<MetaBlock> Transform(MetaBlock.Compressed original, CompressedMetaBlockBuilder builder, BrotliGlobalState initialState){
            yield return builder.Build().MetaBlock;
        }
    }
}
