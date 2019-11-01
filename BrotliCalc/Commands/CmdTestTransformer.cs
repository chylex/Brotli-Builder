using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        public IntRange ArgumentCount => IntRange.Only(3);

        public string Process(string[] args){
            if (!Transformers.TryGetValue(args[0], out var transformer)){
                throw new ArgumentException($"Unknown transformer: {args[0]}");
            }

            var items = Brotli.ListPath(args[1]).SelectCompressedFiles();

            using var table = new Table.CSV(args[2], new []{
                "File", "Quality", "Original Bytes", "Rebuild Bytes", "Transformed Bytes", "Transformed-Original", "Transformed-Rebuild"
            });

            long sumOriginal = 0;
            long sumRebuild = 0;
            long sumTransformed = 0;

            var result = new FileWorker<BrotliFile.Compressed>{
                Work = (group, file) => {
                    var bfs = file.Structure;
                    var transformed = bfs.Transform(transformer);

                    if (transformed.MetaBlocks.SequenceEqual(bfs.MetaBlocks)){ // if the references have not changed, there was no transformation
                        return new List<object[]>();
                    }
                    
                    int? originalBytes = file.SizeBytes;
                    int rebuildBytes = group.CountBytesAndValidate(bfs.Transform(new TransformRebuild()));
                    int transformedBytes = group.CountBytesAndValidate(transformed);

                    if (originalBytes.HasValue){
                        Interlocked.Add(ref sumOriginal, originalBytes.Value);
                        Interlocked.Add(ref sumRebuild, rebuildBytes);
                        Interlocked.Add(ref sumTransformed, transformedBytes);
                    }

                    return new List<object[]>{
                        new object[]{ file.Name, file.Identifier, originalBytes, rebuildBytes, transformedBytes, transformedBytes - originalBytes, transformedBytes - rebuildBytes } // subtraction propagates null
                    };
                },

                Error = (group, file, e) => {
                    return new List<object[]>{
                        new object[]{ file.Name, file.Identifier, file.SizeBytes, null, null, null, null }
                    };
                }
            }.Start(table, items);

            table.AddRow("(Successes)", "-", sumOriginal, sumRebuild, sumTransformed, sumTransformed - sumOriginal, sumTransformed - sumRebuild);

            return $"Processed {result.TotalProcessed} file(s) with {result.TotalErrors} error(s).";
        }
    }
}
