using System;
using System.Collections.Generic;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;
using BrotliImpl.Transformers;

namespace BrotliCalc.Commands{
    class CmdTestReserializeRebuild : CmdAbstractFileTable.Compressed{
        public override string FullName => "test-reserialize-rebuild";
        public override string ShortName => "trr";

        protected override string[] Columns { get; } = {
            "File", "Quality", "Original Bytes", "Reserialize Bytes", "Rebuild Bytes", "Reserialize-Original", "Rebuild-Original"
        };

        protected override IEnumerable<object?[]> GenerateRows(BrotliFileGroup group, BrotliFile.Compressed file){
            var bfs = file.Structure;

            int? originalBytes = file.SizeBytes;
            var reserializeBytes = group.CountBytesAndValidate(bfs, Parameters.Serialization);
            var rebuildBytes = group.CountBytesAndValidate(bfs.Transform(new TransformRebuild(), Parameters.Compression), Parameters.Serialization);

            return new List<object?[]>{
                new object?[]{ file.Name, file.Identifier, originalBytes, reserializeBytes, rebuildBytes, reserializeBytes - originalBytes, rebuildBytes - originalBytes } // subtraction propagates null
            };
        }

        protected override IEnumerable<object?[]> OnError(BrotliFileGroup group, BrotliFile.Compressed file, Exception ex){
            return new List<object?[]>{
                new object?[]{ file.Name, file.Identifier, file.SizeBytes, null, null, null, null }
            };
        }
    }
}
