using System;
using System.Collections.Generic;
using System.Linq;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;
using BrotliImpl.Transformers;
using BrotliLib.Brotli.Encode;

namespace BrotliCalc.Commands{
    class CmdTestTransformer : CmdAbstractFileTable.Compressed{
        private static readonly Dictionary<string, IBrotliTransformer> Transformers = new Dictionary<string, IBrotliTransformer>{
            { "distanceparams", new TransformTestDistanceParameters() },
            { "spliticlengths", new TransformSplitInsertCopyLengths() }
        };

        public override string FullName => "test-transformer";
        public override string ShortName => "tt";

        protected override int ExtraArgumentCount => 1;
        protected override string ExtraArgumentDesc => "<{" + string.Join('|', Transformers.Keys) + "}>";

        protected override string[] Columns { get; } = {
            "File", "Quality", "Original Bytes", "Rebuild Bytes", "Transformed Bytes", "Transformed-Original", "Transformed-Rebuild"
        };

        private IBrotliTransformer? transformer;

        protected override void Setup(string[] args){
            if (!Transformers.TryGetValue(args[0], out transformer)){
                throw new ArgumentException($"Unknown transformer: {args[0]}");
            }
        }

        protected override IEnumerable<object?[]> GenerateRows(BrotliFileGroup group, BrotliFile.Compressed file){
            var bfs = file.Structure;
            var transformed = bfs.Transform(transformer!);

            if (transformed.MetaBlocks.SequenceEqual(bfs.MetaBlocks)){ // if the references have not changed, there was no transformation
                return new List<object[]>();
            }
                    
            int? originalBytes = file.SizeBytes;
            int rebuildBytes = group.CountBytesAndValidate(bfs.Transform(new TransformRebuild()));
            int transformedBytes = group.CountBytesAndValidate(transformed);

            return new List<object?[]>{
                new object?[]{ file.Name, file.Identifier, originalBytes, rebuildBytes, transformedBytes, transformedBytes - originalBytes, transformedBytes - rebuildBytes } // subtraction propagates null
            };
        }

        protected override IEnumerable<object?[]> OnError(BrotliFileGroup group, BrotliFile.Compressed file, Exception ex){
            return new List<object?[]>{
                new object?[]{ file.Name, file.Identifier, file.SizeBytes, null, null, null, null }
            };
        }
    }
}
