using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace BrotliCalc.Helpers{
    sealed class FileWorker<T> where T : BrotliFile{
        public class Result{
            public int TotalProcessed { get; }
            public int TotalErrors { get; }

            public Result(int totalProcessed, int totalErrors){
                this.TotalProcessed = totalProcessed;
                this.TotalErrors = totalErrors;
            }
        }

        private static readonly List<object[]> ErrorEntry = new List<object[]>();

        public Func<T, string> Name { get; set; }
        public Func<BrotliFileGroup, T, IEnumerable<object[]>> Work { get; set; }
        public Func<BrotliFileGroup, T, Exception, IEnumerable<object[]>> Error { get; set; }

        public Result Start(Table output, IEnumerable<(BrotliFileGroup group, T file)> input){
            int errors = 0;

            var name = Name ?? (item => item.ToString());
            var work = Work ?? throw new InvalidOperationException("Cannot start worker without a defined Work function.");
            var error = Error;

            var items = input.ToArray();
            var count = items.Length;

            var results = new List<object[]>[count];
            using var progress = new Progress(count);

            items.WithIndex().Parallelize().ForAll(entry => {
                var (index, (group, file)) = entry;

                try{
                    results[index] = work(group, file).ToList();
                }catch(Exception e){
                    results[index] = error == null ? ErrorEntry : error(group, file, e).ToList();
                    Interlocked.Increment(ref errors);

                    progress.Print($"Error processing {name(file)}: {e.Message}");
                    Debug.WriteLine(e.ToString());
                }finally{
                    progress.Post($"Finished {name(file)}");
                }
            });

            for(int index = 0; index < count; index++){
                var result = results[index];

                if (result == null){
                    progress.Print($"Missing result entry for file {items[index].file}");
                }
                else{
                    foreach(var row in result){
                        output.AddRow(row.ToArray());
                    }
                }
            }

            return new Result(count, errors);
        }
    }
}
