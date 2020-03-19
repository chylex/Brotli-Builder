using System;
using System.Collections.Generic;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;

namespace BrotliCalc.Commands{
    class CmdTestReserialize : CmdAbstractFileTable.Compressed{
        public override string FullName => "test-reserialize";
        public override string ShortName => "trs";

        protected override string[] Columns { get; } = {
            "File", "Quality", "Original Bytes", "Reserialized Bytes", "Reserialized-Original"
        };

        protected override IEnumerable<object?[]> GenerateRows(BrotliFileGroup group, BrotliFile.Compressed file){
            int? originalBytes = file.SizeBytes;
            var reserializeBytes = group.CountBytesAndValidate(file.Reader);

            return new List<object?[]>{
                new object?[]{ file.Name, file.Identifier, originalBytes, reserializeBytes, reserializeBytes - originalBytes } // subtraction propagates null
            };
        }

        protected override IEnumerable<object?[]> OnError(BrotliFileGroup group, BrotliFile.Compressed file, Exception ex){
            return new List<object?[]>{
                new object?[]{ file.Name, file.Identifier, file.SizeBytes, null, null }
            };
        }
    }
}
