using System.Collections.Generic;
using System.Linq;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;

namespace BrotliCalc.Commands{
    class CmdExtractWindowSize : CmdAbstractFileTable.Uncompressed{
        public override string FullName => "extract-window-size";
        public override string ShortName => "ews";

        protected override string WorkDesc => "Extracted window sizes from";

        protected override string[] Columns { get; } = {
            "File", "Uncompressed Bytes", "Level 0", "Level 1", "Level 2", "Level 3", "Level 4", "Level 5", "Level 6", "Level 7", "Level 8", "Level 9", "Level 10", "Level 11"
        };

        protected override IEnumerable<object[]> GenerateRows(BrotliFileGroup group, BrotliFile.Uncompressed file){
            int?[] sizes = new int?[13];
            sizes[0] = file.SizeBytes;

            foreach(var compressed in group.Compressed){
                if (int.TryParse(compressed.Identifier, out int level) && level >= 0 && level <= 11){
                    sizes[level + 1] = compressed.Reader.Parameters.WindowSize.Bits;
                }
            }
            
            yield return sizes.Cast<object>().Prepend(file.Name).ToArray();
        }
    }
}
