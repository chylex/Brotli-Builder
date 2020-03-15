using System;
using System.Collections.Generic;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;
using BrotliImpl.Transformers;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Brotli.Streaming;

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
            var checkingTransformer = new ReferenceChecker(file.Reader, file.Transforming(transformer!));

            int? originalBytes = file.SizeBytes;
            int transformedBytes = group.CountBytesAndValidate(checkingTransformer);

            if (!checkingTransformer.IsDifferent){
                return new List<object[]>();
            }

            int rebuildBytes = group.CountBytesAndValidate(file.Transforming(new TransformRebuild()));

            return new List<object?[]>{
                new object?[]{ file.Name, file.Identifier, originalBytes, rebuildBytes, transformedBytes, transformedBytes - originalBytes, transformedBytes - rebuildBytes } // subtraction propagates null
            };
        }

        protected override IEnumerable<object?[]> OnError(BrotliFileGroup group, BrotliFile.Compressed file, Exception ex){
            return new List<object?[]>{
                new object?[]{ file.Name, file.Identifier, file.SizeBytes, null, null, null, null }
            };
        }

        private class ReferenceChecker : IBrotliFileReader{
            public BrotliFileParameters Parameters => transformingReader.Parameters;
            public BrotliGlobalState State => transformingReader.State;

            public bool IsDifferent { get; private set; }

            private readonly IBrotliFileReader originalReader;
            private readonly IBrotliFileReader transformingReader;

            public ReferenceChecker(IBrotliFileReader originalReader, IBrotliFileReader transformingReader){
                this.originalReader = originalReader;
                this.transformingReader = transformingReader;
            }

            public MetaBlock? NextMetaBlock(){
                if (IsDifferent){
                    return transformingReader.NextMetaBlock();
                }

                var original = originalReader.NextMetaBlock();
                var transformed = transformingReader.NextMetaBlock();

                if (!ReferenceEquals(original, transformed)){ // if the references have not changed, there was no transformation
                    IsDifferent = true;
                }

                return transformed;
            }
        }
    }
}
