using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BrotliCalc.Helpers;
using BrotliImpl.Transformers;
using BrotliLib.Brotli.Encode;
using BrotliLib.Numbers;

namespace BrotliCalc.Commands{
    class CmdTestTransformer : ICommand{
        private static readonly Dictionary<string, IBrotliTransformer> Transformers = new Dictionary<string, IBrotliTransformer>{
            { "distanceparams", new TransformTestDistanceParameters() },
            { "spliticlengths", new TransformSplitInsertCopyLengths() }
        };

        public string FullName => "test-transformer";
        public string ShortName => "tt";

        public string ArgumentDesc => "<{" + string.Join('|', Transformers.Keys) + "}> <source-path> <output-file>";
        public Range ArgumentCount => Range.Only(3);

        public string Process(string[] args){
            if (!Transformers.TryGetValue(args[0], out var transformer)){
                throw new ArgumentException($"Unknown transformer: {args[0]}");
            }

            int totalFiles = 0;
            int failedFiles = 0;

            using(var table = new Table.CSV(args[2], new []{
                "File", "Quality", "Original Bytes", "Rebuild Bytes", "Transformed Bytes", "Transformed-Original", "Transformed-Rebuild"
            })){
                long sumOriginal = 0;
                long sumRebuild = 0;
                long sumTransformed = 0;

                foreach(var group in Brotli.ListPath(args[1])){
                    foreach(var file in group.Compressed){
                        var bfs = file.Structure;

                        int? originalBytes = file.SizeBytes;
                        int? rebuildBytes = null;
                        int? transformedBytes = null;

                        Console.WriteLine($"Processing {file.Name}...");

                        try{
                            var transformed = bfs.Transform(transformer);

                            if (transformed.MetaBlocks.SequenceEqual(bfs.MetaBlocks)){ // if the references have not changed, there was no transformation
                                continue;
                            }

                            rebuildBytes = group.CountBytesAndValidate(bfs.Transform(new TransformRebuild()));
                            transformedBytes = group.CountBytesAndValidate(transformed);
                        }catch(Exception e){
                            Debug.WriteLine(e.ToString());
                            ++failedFiles;
                        }

                        ++totalFiles;
                        table.AddRow(file.Name, file.Identifier, originalBytes, rebuildBytes, transformedBytes, transformedBytes - originalBytes, transformedBytes - rebuildBytes); // subtraction propagates null

                        if (originalBytes.HasValue && rebuildBytes.HasValue && transformedBytes.HasValue){
                            sumOriginal += originalBytes.Value;
                            sumRebuild += rebuildBytes.Value;
                            sumTransformed += transformedBytes.Value;
                        }
                    }
                }
                
                table.AddRow("(Successes)", "-", sumOriginal, sumRebuild, sumTransformed, sumTransformed - sumOriginal, sumTransformed - sumRebuild);
            }

            if (totalFiles > 0){
                Console.WriteLine();
            }

            return "Processed " + totalFiles + " file(s) with " + failedFiles + " error(s).";
        }
    }
}
