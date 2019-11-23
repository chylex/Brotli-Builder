using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace BrotliCalc.Helpers{
    sealed class FileWorker<T> where T : BrotliFile{
        private const int WriterSleepMillis = 200;

        public delegate IEnumerable<object[]> WorkFunc(BrotliFileGroup group, T file);
        public delegate IEnumerable<object[]> ErrorFunc(BrotliFileGroup group, T file, Exception ex);

        private static readonly List<object[]> ErrorEntry = new List<object[]>();

        public Func<T, string> Name { get; set; }
        public WorkFunc Work { get; set; }
        public ErrorFunc Error { get; set; }

        public Result Start(Table output, IEnumerable<(BrotliFileGroup group, T file)> input){
            int errors = 0;

            var name = Name ?? (item => item.ToString());
            var work = Work ?? throw new InvalidOperationException("Cannot start worker without a defined Work function.");
            var error = Error;

            var items = input.ToArray();
            var count = items.Length;

            var results = new List<object[]>[count];
            using var progress = new Progress(count);

            var writerToken = new CancellationTokenSource();
            var writerInfo = new ResultWriter(output, results, writerToken.Token);

            var writerThread = new Thread(ResultWriterThread){
                Name = "ResultWriter",
                Priority = ThreadPriority.BelowNormal,
                IsBackground = true
            };

            writerThread.Start(writerInfo);

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

            writerToken.Cancel();
            writerThread.Join();

            if (writerInfo.MissingEntries.Count > 0){
                foreach(int index in writerInfo.MissingEntries){
                    progress.Print($"Missing result entry for file {items[index].file}");
                }

                progress.Print("");
            }

            return new Result(count, errors);
        }

        public class Result{
            public int TotalProcessed { get; }
            public int TotalErrors { get; }

            public Result(int totalProcessed, int totalErrors){
                this.TotalProcessed = totalProcessed;
                this.TotalErrors = totalErrors;
            }
        }

        private class ResultWriter{
            public Table Output { get; }
            public List<object[]>[] Results { get; }
            public CancellationToken Token { get; }

            public List<int> MissingEntries { get; } = new List<int>();

            public ResultWriter(Table output, List<object[]>[] results, CancellationToken token){
                this.Output = output;
                this.Results = results;
                this.Token = token;
            }
        }

        private void ResultWriterThread(object obj){
            var info = (ResultWriter)obj;
            var output = info.Output;
            var results = info.Results;
            var token = info.Token;

            int index = 0;
            int count = results.Length;

            while(index < count){
                var result = results[index];

                if (result == null){
                    if (token.IsCancellationRequested){
                        info.MissingEntries.Add(index++);
                    }
                    else{
                        Thread.Sleep(WriterSleepMillis);
                    }
                }
                else{
                    foreach(var row in result){
                        output.AddRow(row.ToArray());
                    }

                    ++index;
                }
            }
        }
    }
}
