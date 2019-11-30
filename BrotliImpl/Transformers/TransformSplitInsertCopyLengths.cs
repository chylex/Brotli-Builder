using System.Linq;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Compressed;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.Encode.Build;
using BrotliLib.Brotli.Parameters;

namespace BrotliImpl.Transformers{
    public class TransformSplitInsertCopyLengths : BrotliTransformerCompressed{
        protected override (MetaBlock, BrotliGlobalState) Transform(MetaBlock.Compressed original, BrotliGlobalState state, BrotliCompressionParameters parameters){
            var builder = new CompressedMetaBlockBuilder(original, state);
            var blockTypes = builder.BlockTypes[Category.InsertCopy];

            if (blockTypes.Commands.Count > 0){
                return (original, state);
            }

            var icCommands = builder.InsertCopyCommands.Count;
            var icSwitchAt = icCommands / 2;

            if (icCommands <= 1){
                return (original, state);
            }

            blockTypes.SetInitialLength(icSwitchAt)
                      .AddBlockSwitch(new BlockSwitchCommand(1, icCommands - icSwitchAt));

            return builder.Build(parameters);
        }
    }
}
