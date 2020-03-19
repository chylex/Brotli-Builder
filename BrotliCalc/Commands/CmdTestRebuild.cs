using System;
using System.Collections.Generic;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;
using BrotliImpl.Transformers;

namespace BrotliCalc.Commands{
    class CmdTestRebuild : CmdAbstractFileTable.Compressed{
        public override string FullName => "test-rebuild";
        public override string ShortName => "trb";

        protected override string[] Columns { get; } = {
            "File", "Quality", "Original Bytes", "Rebuild Bytes", "Rebuild-Original"
        };

        protected override IEnumerable<object?[]> GenerateRows(BrotliFileGroup group, BrotliFile.Compressed file){
            int? originalBytes = file.SizeBytes;
            var rebuildBytes = group.CountBytesAndValidate(file.Transforming(new TransformRebuild()));

            return new List<object?[]>{
                new object?[]{ file.Name, file.Identifier, originalBytes, rebuildBytes, rebuildBytes - originalBytes } // subtraction propagates null
            };
        }

        protected override IEnumerable<object?[]> OnError(BrotliFileGroup group, BrotliFile.Compressed file, Exception ex){
            return new List<object?[]>{
                new object?[]{ file.Name, file.Identifier, file.SizeBytes, null, null }
            };
        }
    }
}
