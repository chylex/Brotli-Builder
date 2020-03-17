using System.Collections.Generic;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Compressed;
using BrotliLib.Brotli.Components.Data;

namespace BrotliCalc.Commands{
    class CmdExtractInsertCopyStats : CmdAbstractFileTable.Compressed{
        public override string FullName => "extract-insert-copy-stats";
        public override string ShortName => "eics";

        protected override string WorkDesc => "Extracted insert/copy statistics from";

        protected override string[] Columns { get; } = {
            "File", "Quality", "Meta-Block ID", "Data Length",
            "Insert&Copy Commands", "Literal Bytes",
            "Backward Reference Bytes", "Dictionary Reference Bytes",
            "Backward Reference Count", "Dictionary Reference Count"
        };

        protected override IEnumerable<object[]> GenerateRows(BrotliFileGroup group, BrotliFile.Compressed file){
            var quality = file.Identifier;
            var reader = file.Reader;

            BrotliGlobalState state = reader.State;
            MetaBlock? metaBlock;
            int index = 0;

            while((metaBlock = reader.NextMetaBlock()) != null){
                if (metaBlock is MetaBlock.Compressed c){
                    var row = new List<object>{ file.Name, quality, index, metaBlock.DataLength.UncompressedBytes };
                    ExtractMetadata(row, c.Data, state);
                    yield return row.ToArray();
                }

                ++index;
                state = reader.State;
            }
        }

        private static void ExtractMetadata(List<object> row, CompressedData data, BrotliGlobalState state){
            row.Add(data.InsertCopyCommands.Count);

            int literalBytes = 0;
            int backRefBytes = 0;
            int backRefCount = 0;
            int dictRefBytes = 0;
            int dictRefCount = 0;

            foreach(var command in data.InsertCopyCommands){
                var literals = command.Literals;
                var copyLength = command.CopyLength;
                var copyDistance = command.CopyDistance;

                state.OutputLiterals(literals);
                literalBytes += literals.Count;

                if (copyDistance == DistanceInfo.EndsAfterLiterals){
                    break;
                }

                var copy = state.OutputCopy(copyLength, copyDistance);

                if (copy.IsBackReference){
                    backRefBytes += copy.BytesWritten;
                    ++backRefCount;
                }
                else{
                    dictRefBytes += copy.BytesWritten;
                    ++dictRefCount;
                }
            }

            row.Add(literalBytes);
            row.Add(backRefBytes);
            row.Add(dictRefBytes);
            row.Add(backRefCount);
            row.Add(dictRefCount);
        }
    }
}
