using System.Collections.Generic;
using System.Linq;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;

namespace BrotliCalc.Commands{
    class CmdCompressStats : CmdAbstractFileTable.Uncompressed{
        public override string FullName => "compress-stats";
        public override string ShortName => "cs";

        protected override string WorkDesc => "Generated size statistics for";

        protected override string[] Columns { get; } = {
            "File", "Uncompressed Bytes", "Level 0", "Level 1", "Level 2", "Level 3", "Level 4", "Level 5", "Level 6", "Level 7", "Level 8", "Level 9", "Level 10", "Level 11"
        };

        protected override IEnumerable<object[]> GenerateRows(BrotliFileGroup group, BrotliFile.Uncompressed file){
            int?[] sizes = new int?[13];
            sizes[0] = file.SizeBytes;

            foreach(var compressed in group.Compressed){
                if (int.TryParse(compressed.Identifier, out int level) && level >= 0 && level <= 11){
                    sizes[level + 1] = compressed.SizeBytes;
                }
            }

            yield return sizes.Cast<object>().Prepend(file.Name).ToArray();
        }
    }
}
