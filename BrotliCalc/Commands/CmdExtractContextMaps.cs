using System;
using System.Collections.Generic;
using System.Text;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Compressed;
using BrotliLib.Brotli.Utils;

namespace BrotliCalc.Commands{
    class CmdExtractContextMaps : CmdAbstractFileTable.Compressed{
        public override string FullName => "extract-context-maps";
        public override string ShortName => "ecm";

        protected override int ExtraArgumentCount => 1;
        protected override string ExtraArgumentDesc => "<literal|distance>";

        protected override string WorkDesc => "Extracted context maps from";

        protected override string[] Columns { get; } = {
            "File", "Quality", "Meta-Block ID", "Data Length",
            "Block Types", "Huffman Trees", "Context Map"
        };

        private Category category;

        protected override void Setup(string[] args){
            category = args[0] switch{
                "literal" => Category.Literal,
                "distance" => Category.Distance,
                _ => throw new ArgumentException("Invalid context map category: " + args[0])
            };
        }

        protected override IEnumerable<object[]> GenerateRows(BrotliFileGroup group, BrotliFile.Compressed file){
            var quality = file.Identifier;
            var reader = file.Reader;

            MetaBlock? metaBlock;
            int index = 0;

            while((metaBlock = reader.NextMetaBlock()) != null){
                if (metaBlock is MetaBlock.Compressed c){
                    var row = new List<object>{ file.Name, quality, index, metaBlock.DataLength.UncompressedBytes };
                    ExtractMetadata(row, c.Header);
                    yield return row.ToArray();
                }

                ++index;
            }
        }

        private void ExtractMetadata(List<object> row, CompressedHeader header){
            var blockTypes = header.BlockTypes[category].TypeCount;

            var map = category switch{
                Category.Literal => header.LiteralCtxMap,
                Category.Distance => header.DistanceCtxMap,
                _ => throw new InvalidOperationException("Invalid context map category: " + category)
            };

            row.Add(blockTypes);
            row.Add(map.TreeCount);

            StringBuilder builder = new StringBuilder();

            for(int blockID = 0; blockID < blockTypes; blockID++){
                for(int contextID = 0; contextID < map.ContextsPerBlockType; contextID++){
                    builder.Append(map.DetermineTreeID(blockID, contextID));
                    builder.Append(',');
                }
            }

            builder.Length -= 1;
            row.Add(builder.ToString());
        }
    }
}
