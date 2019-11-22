using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Contents.Compressed;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Brotli.Encode;

namespace BrotliImpl.Transformers{
    public class TransformSplitInsertCopyLengths : CompressedMetaBlockTransformer{
        protected override IEnumerable<MetaBlock> Transform(MetaBlock.Compressed original, CompressedMetaBlockBuilder builder, BrotliGlobalState initialState){
            var blockTypes = builder.BlockTypes[Category.InsertCopy];

            if (blockTypes.Commands.Any()){
                yield return original;
                yield break;
            }

            var icCommands = builder.InsertCopyCommands.Count();
            var icSwitchAt = icCommands / 2;

            if (icCommands <= 1){
                yield return original;
                yield break;
            }

            blockTypes.SetInitialLength(icSwitchAt)
                      .AddBlockSwitch(new BlockSwitchCommand(1, icCommands - icSwitchAt));

            yield return builder.Build().MetaBlock;
        }
    }
}
