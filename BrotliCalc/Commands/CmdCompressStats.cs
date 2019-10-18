using System.Linq;
using BrotliCalc.Helpers;
using BrotliLib.Numbers;

namespace BrotliCalc.Commands{
    class CmdCompressStats : ICommand{
        public string FullName => "compress-stats";
        public string ShortName => "cs";

        public string ArgumentDesc => "<source-path> <output-file>";
        public IntRange ArgumentCount => IntRange.Only(2);

        public string Process(string[] args){
            int totalFiles = 0;

            using(var table = new Table.CSV(args[1], new []{
                "File", "Uncompressed Bytes", "Level 0", "Level 1", "Level 2", "Level 3", "Level 4", "Level 5", "Level 6", "Level 7", "Level 8", "Level 9", "Level 10", "Level 11"
            })){
                foreach(var group in Brotli.ListPath(args[0])){
                    var file = group.Uncompressed;

                    int?[] sizes = new int?[13];
                    sizes[0] = file.SizeBytes;

                    foreach(var compressed in group.Compressed){
                        if (int.TryParse(compressed.Identifier, out int level) && level >= 0 && level <= 11){
                            sizes[level + 1] = compressed.SizeBytes;
                        }
                    }

                    ++totalFiles;
                    table.AddRow(new object[]{ file.Name }.Concat(sizes.Cast<object>()).ToArray());
                }
            }

            return $"Generated statistics for {totalFiles} file(s).";
        }
    }
}
