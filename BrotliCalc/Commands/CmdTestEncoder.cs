using System;
using System.Collections.Generic;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;
using BrotliImpl.Encoders;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Encode;

namespace BrotliCalc.Commands{
    class CmdTestEncoder : CmdAbstractFileTable.Uncompressed{
        private static readonly Dictionary<string, IBrotliEncoder> Encoders = new Dictionary<string, IBrotliEncoder>{
            { "literals", new EncodeLiterals() },
            { "greedy-copies", new EncodeGreedySearch.OnlyBackReferences(minLength: 4) },
            { "greedy-dict", new EncodeGreedySearch.OnlyDictionary() },
            { "greedy-mixed", new EncodeGreedySearch.Mixed(minCopyLength: 4) }
        };

        public override string FullName => "test-encoder";
        public override string ShortName => "te";

        protected override int ExtraArgumentCount => 1;
        protected override string ExtraArgumentDesc => "<{" + string.Join('|', Encoders.Keys) + "}>";

        protected override string[] Columns { get; } = {
            "File", "Uncompressed Bytes", "Encoded Bytes", "Encoded-Uncompressed"
        };

        private IBrotliEncoder encoder;

        protected override void Setup(string[] args){
            if (!Encoders.TryGetValue(args[0], out encoder)){
                throw new ArgumentException($"Unknown encoder: {args[0]}");
            }
        }

        protected override IEnumerable<object[]> GenerateRows(BrotliFileGroup group, BrotliFile.Uncompressed file){
            int? uncompressedBytes = file.SizeBytes;
            int encodeBytes = group.CountBytesAndValidate(BrotliFileStructure.FromEncoder(new BrotliFileParameters(), encoder, file.Contents));

            return new List<object[]>{
                new object[]{ file.Name, uncompressedBytes, encodeBytes, encodeBytes - uncompressedBytes } // subtraction propagates null
            };
        }

        protected override IEnumerable<object[]> OnError(BrotliFileGroup group, BrotliFile.Uncompressed file, Exception ex){
            return new List<object[]>{
                new object[]{ file.Name, file.SizeBytes, null, null }
            };
        }
    }
}
