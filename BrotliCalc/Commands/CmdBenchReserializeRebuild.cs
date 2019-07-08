using System;
using System.Diagnostics;
using BrotliCalc.Helpers;
using BrotliImpl.Transformers;
using BrotliLib.Numbers;

namespace BrotliCalc.Commands{
    class CmdBenchReserializeRebuild : ICommand{
        private const int SkippedRuns = 3;
        private const int CountedRuns = 10;

        public string FullName => "bench-reserialize-rebuild";
        public string ShortName => "brr";

        public string ArgumentDesc => "<source-path> <output-file>";
        public Range ArgumentCount => Range.Only(2);

        public string Process(string[] args){
            int totalFiles = 0;
            int failedFiles = 0;

            using(var table = new Table.CSV(args[1], new []{
                "File", "Quality", "Original Bytes", "Reserialize Bytes", "Rebuild Bytes", "Avg Reserialize Time (ms)", "Avg Rebuild Time (ms)"
            })){
                foreach(var group in Brotli.ListPath(args[0])){
                    foreach(var file in group.Compressed){
                        var bfs = file.Structure;

                        int? originalBytes = file.SizeBytes;
                        int? reserializeBytes = null;
                        int? rebuildBytes = null;
                        long? reserializeTotalTime = 0L;
                        long? rebuildTotalTime = 0L;

                        for(int run = 1; run <= SkippedRuns + CountedRuns; run++){
                            Stopwatch swReserialize = Stopwatch.StartNew();

                            try{
                                reserializeBytes = group.CountBytesAndValidate(bfs);
                            }catch(Exception e){
                                Debug.WriteLine(e.ToString());
                                ++failedFiles;
                                reserializeTotalTime = null;
                                rebuildTotalTime = null;
                                break;
                            }finally{
                                swReserialize.Stop();
                            }

                            Stopwatch swRebuild = Stopwatch.StartNew();

                            try{
                                rebuildBytes = group.CountBytesAndValidate(bfs.Transform(new TransformRebuild()));
                            }catch(Exception e){
                                Debug.WriteLine(e.ToString());
                                ++failedFiles;
                                reserializeTotalTime = null;
                                rebuildTotalTime = null;
                                break;
                            }finally{
                                swRebuild.Stop();
                            }

                            if (run > SkippedRuns){
                                reserializeTotalTime += swReserialize.ElapsedMilliseconds;
                                rebuildTotalTime += swRebuild.ElapsedMilliseconds;
                            }
                        }
                        
                        ++totalFiles;
                        table.AddRow(file.Name, file.Identifier, originalBytes, reserializeBytes, rebuildBytes, reserializeTotalTime / CountedRuns, rebuildTotalTime / CountedRuns); // subtraction propagates null
                    }
                }
            }

            return "Processed " + totalFiles + " file(s) with " + failedFiles + " error(s).";
        }
    }
}
