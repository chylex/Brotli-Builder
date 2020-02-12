using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.Encode.Build;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Brotli.Utils;

namespace BrotliImpl.Transformers{
    public class TransformSplitInsertCopyLengths : BrotliTransformerCompressed{
        protected override (MetaBlock, BrotliGlobalState) Transform(MetaBlock.Compressed original, BrotliGlobalState state, BrotliCompressionParameters parameters){
            if (original.Data.InsertCopyCommands.Count <= 1 || original.Data.BlockSwitchCommands[Category.InsertCopy].Count > 0){
                return (original, state);
            }

            var builder = new CompressedMetaBlockBuilder(original, state);
            var icBlocks = builder.BlockTypes[Category.InsertCopy];
            var icCommands = builder.GetTotalBlockLength(Category.InsertCopy);

            icBlocks.SetInitialLength(icCommands / 2)
                    .AddFinalBlockSwitch(1);

            return builder.Build(parameters);
        }
    }
}
