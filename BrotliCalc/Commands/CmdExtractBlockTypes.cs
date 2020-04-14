using System.Collections.Generic;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Utils;

namespace BrotliCalc.Commands{
    class CmdExtractBlockTypes : CmdAbstractFileTable.Compressed{
        public override string FullName => "extract-block-types";
        public override string ShortName => "ebt";

        protected override string WorkDesc => "Extracted block types from";

        protected override string[] Columns { get; } = {
            "File", "Quality", "Meta-Block ID",
            "Category", "Block ID", "Block Type", "Block Length"
        };

        protected override IEnumerable<object[]> GenerateRows(BrotliFileGroup group, BrotliFile.Compressed file){
            var quality = file.Identifier;
            var reader = file.Reader;

            MetaBlock? metaBlock;
            int index = 0;

            while((metaBlock = reader.NextMetaBlock()) != null){
                if (metaBlock is MetaBlock.Compressed c){
                    var rowStart = new List<object>{ file.Name, quality, index };

                    foreach(var category in Categories.LID){
                        var rowFinal = new List<object>(rowStart){ category.Id() };
                        var blockID = 0;

                        rowFinal.Add(blockID);
                        rowFinal.Add(0);
                        rowFinal.Add(c.Header.BlockTypes[category].InitialLength);
                        yield return rowFinal.ToArray();

                        foreach(var command in c.Data.BlockSwitchCommands[category]){
                            rowFinal[^3] = ++blockID;
                            rowFinal[^2] = command.Type;
                            rowFinal[^1] = command.Length;
                            yield return rowFinal.ToArray();
                        }
                    }
                }

                ++index;
            }
        }
    }
}
