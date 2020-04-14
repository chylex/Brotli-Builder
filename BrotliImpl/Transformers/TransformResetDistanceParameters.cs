using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.Encode.Build;
using BrotliLib.Brotli.Parameters;

namespace BrotliImpl.Transformers{
    public class TransformResetDistanceParameters : BrotliTransformerCompressed{
        protected override (MetaBlock, BrotliGlobalState) Transform(MetaBlock.Compressed original, BrotliGlobalState state, BrotliCompressionParameters parameters){
            return new CompressedMetaBlockBuilder(original, state){
                DistanceParameters = DistanceParameters.Zero
            }.Build(parameters);
        }
    }
}
