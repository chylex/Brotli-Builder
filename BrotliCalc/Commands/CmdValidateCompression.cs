using System;
using System.Collections.Generic;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;
using BrotliLib.Brotli.Output;
using BrotliLib.Collections;

namespace BrotliCalc.Commands{
    class CmdValidateCompression : CmdAbstractFileTable.Compressed{
        public override string FullName => "validate-compression";
        public override string ShortName => "vc";

        protected override string WorkDesc => "Validated";

        protected override string[] Columns { get; } = {
            "File", "Quality", "Result"
        };

        protected override IEnumerable<object?[]> GenerateRows(BrotliFileGroup group, BrotliFile.Compressed file){
            var original = group.Uncompressed.Contents;

            var reader = file.Reader;
            var output = new BrotliOutputStored();

            reader.AddOutputCallback(output);
            while(reader.NextMetaBlock() != null){}
            reader.RemoveOutputCallback(output);

            if (!CollectionHelper.Equal(output.AsBytes, original)){
                throw new MismatchedOutputBytesException();
            }

            return new List<object?[]>{
                new object?[]{ file.Name, file.Identifier, "OK" }
            };
        }

        protected override IEnumerable<object?[]> OnError(BrotliFileGroup group, BrotliFile.Compressed file, Exception ex){
            return new List<object?[]>{
                new object?[]{ file.Name, file.Identifier, ex is MismatchedOutputBytesException ? "Mismatch" : "Error" }
            };
        }

        private sealed class MismatchedOutputBytesException : Exception{
            public MismatchedOutputBytesException() : base("Mismatched output bytes."){}
        }
    }
}
