using System;
using System.Diagnostics;
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
            int totalFiles = 0;
            int failedFiles = 0;

            using(var table = new Table.CSV(args[1], new []{
                "File", "Quality", "Original Bytes", "Reserialize Bytes", "Rebuild Bytes", "Reserialize-Original", "Rebuild-Original"
            })){
                long sumOriginal = 0;
                long sumReserialize = 0;
                long sumRebuild = 0;

                foreach(var group in Brotli.ListPath(args[0])){
                    foreach(var file in group.Compressed){
                        var bfs = file.Structure;

                        int? originalBytes = file.SizeBytes;
                        int? reserializeBytes = null;
                        int? rebuildBytes = null;

                        Console.WriteLine($"Processing {file.Name}...");

                        try{
                            reserializeBytes = group.CountBytesAndValidate(bfs);
                            rebuildBytes = group.CountBytesAndValidate(bfs.Transform(new TransformRebuild()));
                        }catch(Exception e){
                            Debug.WriteLine(e.ToString());
                            ++failedFiles;
                        }
                        
                        ++totalFiles;
                        table.AddRow(file.Name, file.Identifier, originalBytes, reserializeBytes, rebuildBytes, reserializeBytes - originalBytes, rebuildBytes - originalBytes); // subtraction propagates null

                        if (originalBytes.HasValue && reserializeBytes.HasValue && rebuildBytes.HasValue){
                            sumOriginal += originalBytes.Value;
                            sumReserialize += reserializeBytes.Value;
                            sumRebuild += rebuildBytes.Value;
                        }
                    }
                }
                
                table.AddRow("(Successes)", "-", sumOriginal, sumReserialize, sumRebuild, sumReserialize - sumOriginal, sumRebuild - sumOriginal);
            }

            if (totalFiles > 0){
                Console.WriteLine();
            }

            return "Processed " + totalFiles + " file(s) with " + failedFiles + " error(s).";
        }
    }
}
