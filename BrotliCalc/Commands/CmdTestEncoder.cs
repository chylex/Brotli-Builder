using System;
using System.Collections.Generic;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;
using BrotliLib.Brotli.Encode;

namespace BrotliCalc.Commands{
    class CmdTestEncoder : CmdAbstractFileTable.Uncompressed{
        public override string FullName => "test-encoder";
        public override string ShortName => "te";

        protected override int ExtraArgumentCount => 1;
        protected override string ExtraArgumentDesc => CmdEncode.EncoderArgumentDesc;

        protected override string[] Columns { get; } = {
            "File", "Uncompressed Bytes", "Encoded Bytes", "Encoded-Uncompressed"
        };

        private IBrotliEncoder? encoder;

        protected override void Setup(string[] args){
            encoder = CmdEncode.GetEncoder(args[0]);
        }

        protected override IEnumerable<object?[]> GenerateRows(BrotliFileGroup group, BrotliFile.Uncompressed file){
            int? uncompressedBytes = file.SizeBytes;
            int encodeBytes = group.CountBytesAndValidate(file.Encoding(encoder!));

            return new List<object?[]>{
                new object?[]{ file.Name, uncompressedBytes, encodeBytes, encodeBytes - uncompressedBytes } // subtraction propagates null
            };
        }

        protected override IEnumerable<object?[]> OnError(BrotliFileGroup group, BrotliFile.Uncompressed file, Exception ex){
            return new List<object?[]>{
                new object?[]{ file.Name, file.SizeBytes, null, null }
            };
        }
    }
}
