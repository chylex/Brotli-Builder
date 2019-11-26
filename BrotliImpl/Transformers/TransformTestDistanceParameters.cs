using System;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.Encode.Build;
using BrotliLib.Brotli.Parameters;

namespace BrotliImpl.Transformers{
    public class TransformTestDistanceParameters : BrotliTransformerCompressed{
        protected override (MetaBlock, BrotliGlobalState) Transform(MetaBlock.Compressed original, BrotliGlobalState state, BrotliCompressionParameters parameters){
            var builder = new CompressedMetaBlockBuilder(original, state);
            var tracker = new MetaBlockSizeTracker(state);

            for(byte postfixBitCount = 0; postfixBitCount <= DistanceParameters.MaxPostfixBitCount; postfixBitCount++){
                for(byte directCodeBits = 0; directCodeBits <= DistanceParameters.MaxDirectCodeBits; directCodeBits++){
                    builder.DistanceParameters = new DistanceParameters(postfixBitCount, directCodeBits);
                    tracker.Test(builder, parameters, debugText: "[PostfixBitCount = " + postfixBitCount + ", DirectCodeBits = " + directCodeBits + "]");
                }
            }

            return tracker.Smallest ?? throw new InvalidOperationException("Transformation did not generate any meta-blocks.");
        }
    }
}
