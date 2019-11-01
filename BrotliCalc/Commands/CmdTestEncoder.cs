using System;
using System.Collections.Generic;
using System.Threading;
using BrotliCalc.Helpers;
using BrotliImpl.Encoders;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Encode;
using BrotliLib.Numbers;

namespace BrotliCalc.Commands{
    class CmdTestEncoder : ICommand{
        private static readonly Dictionary<string, IBrotliEncoder> Encoders = new Dictionary<string, IBrotliEncoder>{
            { "literals", new EncodeLiterals() },
            { "greedy-copies", new EncodeGreedySearch.OnlyBackReferences(minLength: 4) },
            { "greedy-dict", new EncodeGreedySearch.OnlyDictionary() },
            { "greedy-mixed", new EncodeGreedySearch.Mixed(minCopyLength: 4) }
        };

        public string FullName => "test-encoder";
        public string ShortName => "te";

        public string ArgumentDesc => "<{" + string.Join('|', Encoders.Keys) + "}> <source-path> <output-file>";
        public IntRange ArgumentCount => IntRange.Only(3);

        public string Process(string[] args){
            if (!Encoders.TryGetValue(args[0], out var encoder)){
                throw new ArgumentException($"Unknown encoder: {args[0]}");
            }

            var items = Brotli.ListPath(args[1]).SelectUncompressedFiles();
            var parameters = new BrotliFileParameters();

            using var table = new Table.CSV(args[2], new []{
                "File", "Uncompressed Bytes", "Encoded Bytes", "Encoded-Uncompressed"
            });

            long sumUncompressed = 0;
            long sumEncoded = 0;

            var result = new FileWorker<BrotliFile.Uncompressed>{
                Work = (group, file) => {
                    int? uncompressedBytes = file.SizeBytes;
                    int encodeBytes = group.CountBytesAndValidate(BrotliFileStructure.FromEncoder(parameters, encoder, file.Contents));

                    if (uncompressedBytes.HasValue){
                        Interlocked.Add(ref sumUncompressed, uncompressedBytes.Value);
                        Interlocked.Add(ref sumEncoded, encodeBytes);
                    }

                    return new List<object[]>{
                        new object[]{ file.Name, uncompressedBytes, encodeBytes, encodeBytes - uncompressedBytes } // subtraction propagates null
                    };
                },

                Error = (group, file, e) => {
                    return new List<object[]>{
                        new object[]{ file.Name, file.SizeBytes, null, null }
                    };
                }
            }.Start(table, items);

            table.AddRow("(Successes)", sumUncompressed, sumEncoded, sumEncoded - sumUncompressed);
            
            return $"Processed {result.TotalProcessed} file(s) with {result.TotalErrors} error(s).";
        }
    }
}
