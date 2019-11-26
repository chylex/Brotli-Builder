using System.Collections.Generic;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Encode;

namespace BrotliImpl.Transformers{
    public class TransformTestDistanceParameters : CompressedMetaBlockTransformer{
        protected override IEnumerable<MetaBlock> Transform(MetaBlock.Compressed original, CompressedMetaBlockBuilder builder, BrotliGlobalState initialState){
            var tracker = new MetaBlockSizeTracker(initialState);

            for(byte postfixBitCount = 0; postfixBitCount <= DistanceParameters.MaxPostfixBitCount; postfixBitCount++){
                for(byte directCodeBits = 0; directCodeBits <= DistanceParameters.MaxDirectCodeBits; directCodeBits++){
                    builder.DistanceParameters = new DistanceParameters(postfixBitCount, directCodeBits);
                    tracker.Test(builder, debugText: "[PostfixBitCount = " + postfixBitCount + ", DirectCodeBits = " + directCodeBits + "]");
                }
            }

            yield return tracker.Smallest!;
        }
    }
}
