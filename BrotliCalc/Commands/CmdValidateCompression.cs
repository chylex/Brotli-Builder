using System;
using System.Collections.Generic;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;
using BrotliLib.Collections;

namespace BrotliCalc.Commands{
    class CmdValidateCompression : CmdAbstractFileTable.Compressed{
        public override string FullName => "validate-compression";
        public override string ShortName => "vc";

        protected override string WorkDesc => "Validated";

        protected override string[] Columns { get; } = {
            "File", "Quality", "Matches"
        };

        protected override IEnumerable<object?[]> GenerateRows(BrotliFileGroup group, BrotliFile.Compressed file){
            var original = group.Uncompressed.Contents;
            var decompressed = file.Structure.Decompress().AsBytes;

            if (!CollectionHelper.Equal(decompressed, original)){
                throw new InvalidOperationException("Mismatched output bytes.");
            }

            return new List<object?[]>{
                new object?[]{ file.Name, file.Identifier, 1 }
            };
        }

        protected override IEnumerable<object?[]> OnError(BrotliFileGroup group, BrotliFile.Compressed file, Exception ex){
            return new List<object?[]>{
                new object?[]{ file.Name, file.Identifier, 0 }
            };
        }
    }
}
