using System;
using System.Diagnostics;
using System.Linq;
using BrotliCalc.Helpers;
using BrotliImpl.Transformers;
using BrotliLib.Brotli;
using BrotliLib.Numbers;

namespace BrotliCalc.Commands{
    class CmdTestReserializeRebuild : ICommand{
        public string FullName => "test-reserialize-rebuild";
        public string ShortName => "trr";

        public string ArgumentDesc => "<source-path> <output-file>";
        public Range ArgumentCount => Range.Only(2);

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
                    var originalContents = group.Uncompressed.Contents;

                    foreach(var file in group.Compressed){
                        var bfs = file.Structure;

                        int? originalBytes = file.SizeBytes;
                        int? reserializeBytes = null;
                        int? rebuildBytes = null;

                        try{
                            reserializeBytes = GetBytesAndValidate(bfs, originalContents);
                            rebuildBytes = GetBytesAndValidate(bfs.Transform(new TransformRebuild()), originalContents);
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

            return "Processed " + totalFiles + " file(s) with " + failedFiles + " error(s).";
        }

        private static int GetBytesAndValidate(BrotliFileStructure bfs, byte[] originalContents){
            var serialized = bfs.Serialize();
            var output = bfs.GetDecompressionState(serialized, enableMarkers: false);

            if (!output.AsBytes.SequenceEqual(originalContents)){
                throw new InvalidOperationException("Mismatched output bytes.");
            }

            return (7 + serialized.Length) / 8;
        }
    }
}
