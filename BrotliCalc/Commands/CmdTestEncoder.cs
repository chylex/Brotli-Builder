using System;
using System.Collections.Generic;
using System.Diagnostics;
using BrotliCalc.Helpers;
using BrotliImpl.Encoders;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Encode;
using BrotliLib.Numbers;

namespace BrotliCalc.Commands{
    class CmdTestEncoder : ICommand{
        private static readonly Dictionary<string, IBrotliEncoder> Encoders = new Dictionary<string, IBrotliEncoder>{
            { "literals", new EncodeLiterals() },
            { "greedycopy", new EncodeGreedyCopySearch() }
        };

        public string FullName => "test-encoder";
        public string ShortName => "te";

        public string ArgumentDesc => "<{" + string.Join('|', Encoders.Keys) + "}> <source-path> <output-file>";
        public Range ArgumentCount => Range.Only(3);

        public string Process(string[] args){
            if (!Encoders.TryGetValue(args[0], out var encoder)){
                throw new ArgumentException($"Unknown encoder: {args[0]}");
            }

            var parameters = new BrotliFileParameters();

            int totalFiles = 0;
            int failedFiles = 0;

            using(var table = new Table.CSV(args[2], new []{
                "File", "Uncompressed Bytes", "Encoded Bytes", "Encoded-Uncompressed"
            })){
                long sumUncompressed = 0;
                long sumEncoded = 0;

                foreach(var group in Brotli.ListPath(args[1])){
                    var file = group.Uncompressed;

                    int? uncompressedBytes = file.SizeBytes;
                    int? encodeBytes = null;

                    try{
                        encodeBytes = group.CountBytesAndValidate(BrotliFileStructure.FromEncoder(parameters, encoder, file.Contents));
                    }catch(Exception e){
                        Debug.WriteLine(e.ToString());
                        ++failedFiles;
                    }

                    ++totalFiles;
                    table.AddRow(file.Name, uncompressedBytes, encodeBytes, encodeBytes - uncompressedBytes); // subtraction propagates null

                    if (uncompressedBytes.HasValue && encodeBytes.HasValue){
                        sumUncompressed += uncompressedBytes.Value;
                        sumEncoded += encodeBytes.Value;
                    }
                }
                
                table.AddRow("(Successes)", sumUncompressed, sumEncoded, sumEncoded - sumUncompressed);
            }

            return "Processed " + totalFiles + " file(s) with " + failedFiles + " error(s).";
        }
    }
}
