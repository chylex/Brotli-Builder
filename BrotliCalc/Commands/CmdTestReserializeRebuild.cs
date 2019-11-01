using System.Collections.Generic;
using System.Threading;
using BrotliCalc.Helpers;
using BrotliImpl.Transformers;
using BrotliLib.Numbers;

namespace BrotliCalc.Commands{
    class CmdTestReserializeRebuild : ICommand{
        public string FullName => "test-reserialize-rebuild";
        public string ShortName => "trr";

        public string ArgumentDesc => "<source-path> <output-file>";
        public IntRange ArgumentCount => IntRange.Only(2);

        public string Process(string[] args){
            var items = Brotli.ListPath(args[0]).SelectCompressedFiles();

            using var table = new Table.CSV(args[1], new []{
                "File", "Quality", "Original Bytes", "Reserialize Bytes", "Rebuild Bytes", "Reserialize-Original", "Rebuild-Original"
            });

            long sumOriginal = 0;
            long sumReserialize = 0;
            long sumRebuild = 0;

            var result = new FileWorker<BrotliFile.Compressed>{
                Work = (group, file) => {
                    var bfs = file.Structure;

                    int? originalBytes = file.SizeBytes;
                    var reserializeBytes = group.CountBytesAndValidate(bfs);
                    var rebuildBytes = group.CountBytesAndValidate(bfs.Transform(new TransformRebuild()));

                    if (originalBytes.HasValue){
                        Interlocked.Add(ref sumOriginal, originalBytes.Value);
                        Interlocked.Add(ref sumReserialize, reserializeBytes);
                        Interlocked.Add(ref sumRebuild, rebuildBytes);
                    }

                    return new List<object[]>{
                        new object[]{ file.Name, file.Identifier, originalBytes, reserializeBytes, rebuildBytes, reserializeBytes - originalBytes, rebuildBytes - originalBytes } // subtraction propagates null
                    };
                },

                Error = (group, file, e) => {
                    return new List<object[]>{
                        new object[]{ file.Name, file.Identifier, file.SizeBytes, null, null, null, null }
                    };
                }
            }.Start(table, items);
            
            table.AddRow("(Successes)", "-", sumOriginal, sumReserialize, sumRebuild, sumReserialize - sumOriginal, sumRebuild - sumOriginal);

            return $"Processed {result.TotalProcessed} file(s) with {result.TotalErrors} error(s).";
        }
    }
}
