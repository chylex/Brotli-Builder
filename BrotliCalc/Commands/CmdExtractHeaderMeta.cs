using System.Collections.Generic;
using System.Linq;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Compressed;
using BrotliLib.Brotli.Utils;

namespace BrotliCalc.Commands{
    class CmdExtractHeaderMeta : CmdAbstractFileTable.Compressed{
        public override string FullName => "extract-header-meta";
        public override string ShortName => "ehm";

        protected override string WorkDesc => "Extracted header metadata from";

        protected override string[] Columns { get; } = {
            "File", "Quality", "Meta-Block ID", "Meta-Block Type", "Data Length",
            "Postfix Bits", "Direct Distance Codes", "Literal Context Modes",
            "Block Types [L]", "Block Types [I]", "Block Types [D]",
            "Huffman Trees [L]", "Huffman Trees [I]", "Huffman Trees [D]",
            "Block Switch Commands [L]", "Block Switch Commands [I]", "Block Switch Commands [D]",
            "Insert&Copy Commands"
        };

        protected override IEnumerable<object[]> GenerateRows(BrotliFileGroup group, BrotliFile.Compressed file){
            var quality = file.Identifier;
            var reader = file.Reader;

            MetaBlock? metaBlock;
            int index = 0;

            while((metaBlock = reader.NextMetaBlock()) != null){
                var row = new List<object>{ file.Name, quality, index++, GetMetaBlockType(metaBlock), metaBlock.DataLength.UncompressedBytes };

                if (metaBlock is MetaBlock.Compressed c){
                    ExtractMetadata(row, c.Header, c.Data);
                }
                else{
                    while(row.Count < Columns.Length){
                        row.Add("");
                    }
                }

                yield return row.ToArray();
            }
        }

        private static string GetMetaBlockType(MetaBlock metaBlock){
            switch(metaBlock){
                case MetaBlock.LastEmpty _:
                    return "Empty, Last";

                case MetaBlock.PaddedEmpty pe:
                    int length = pe.HiddenData.Length;
                    return length == 0 ? "Empty, Padded" : $"Empty, Skip {length} B";

                case MetaBlock.Uncompressed _:
                    return "Uncompressed";

                case MetaBlock.Compressed _:
                    return "Compressed";

                default:
                    return "Unknown";
            }
        }

        private static void ExtractMetadata(List<object> row, CompressedHeader header, CompressedData data){
            row.Add(header.DistanceParameters.PostfixBitCount);
            row.Add(header.DistanceParameters.DirectCodeCount);

            var lcm = header.LiteralCtxModes;

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (Enumerable.Range(1, lcm.Count - 1).All(index => lcm[index] == lcm[0])){
                row.Add(lcm[0].ToString());
            }
            else{
                row.Add(string.Join(',', lcm.Select(mode => mode.ToString())));
            }

            foreach(var category in Categories.LID){
                row.Add(header.BlockTypes[category].TypeCount);
            }

            row.Add(header.LiteralTrees.Count);
            row.Add(header.InsertCopyTrees.Count);
            row.Add(header.DistanceTrees.Count);

            foreach(var category in Categories.LID){
                row.Add(data.BlockSwitchCommands[category].Count);
            }

            row.Add(data.InsertCopyCommands.Count);
        }
    }
}
