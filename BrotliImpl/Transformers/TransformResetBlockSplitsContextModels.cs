using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.Encode.Build;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Brotli.Utils;

namespace BrotliImpl.Transformers{
    public class TransformResetBlockSplitsContextModels : BrotliTransformerCompressed{
        protected override (MetaBlock, BrotliGlobalState) Transform(MetaBlock.Compressed original, BrotliGlobalState state, BrotliCompressionParameters parameters){
            var builder = new CompressedMetaBlockBuilder(original, state){
                LiteralCtxMap = ContextMapBuilder.Literals.Simple,
                DistanceCtxMap = ContextMapBuilder.Distances.Simple
            };

            foreach(var category in Categories.LID){
                builder.BlockTypes[category].Reset();
            }

            return builder.UseSameLiteralContextMode(LiteralContextMode.LSB6).Build(parameters);
        }
    }
}
