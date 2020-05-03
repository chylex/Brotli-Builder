using System.Collections.Generic;
using System.Linq;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Compressed;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Utils;

namespace BrotliCalc.Commands{
    class CmdExtractDistanceContextInfo : CmdAbstractFileTable.Compressed{
        public override string FullName => "extract-distance-context-info";
        public override string ShortName => "edci";

        protected override string WorkDesc => "Extracted distance context info from";

        protected override string[] Columns { get; } = new string[]{
            "File", "Quality", "Meta-Block ID", "Data Length",
            "Postfix Bits", "Direct Distance Codes",
            "Block Types [D]", "Huffman Trees [D]",
            "Distance Context ID",
            "Insert&Copy Commands",
            "Complex Distance Code Avg Path Length",
        }.Concat(Enumerable.Range(0, MaxComplexCodes).Select(code => $"[Code = {code}]")).ToArray();

        private static readonly int MaxComplexCodes = new DistanceParameters(DistanceParameters.MaxPostfixBitCount, 0).AlphabetSize.SymbolCount - DistanceCode.Last.CodeCount;

        protected override IEnumerable<object[]> GenerateRows(BrotliFileGroup group, BrotliFile.Compressed file){
            var quality = file.Identifier;
            var reader = file.Reader;

            MetaBlock? metaBlock;
            int index = 0;

            while((metaBlock = reader.NextMetaBlock()) != null){
                if (metaBlock is MetaBlock.Compressed c){
                    var rowStart = new List<object>{ file.Name, quality, index, metaBlock.DataLength.UncompressedBytes };
                    ExtractMetaBlockMetadata(rowStart, c.Header);

                    for(int contextID = 0; contextID < Category.Distance.Contexts(); contextID++){
                        var rowFinal = new List<object>(rowStart){ contextID };
                        ExtractDistanceContextMetadata(rowFinal, c.Header, c.Data, contextID);

                        while(rowFinal.Count < Columns.Length){
                            rowFinal.Add("");
                        }

                        yield return rowFinal.ToArray();
                    }
                }

                index++;
            }
        }

        private static void ExtractMetaBlockMetadata(List<object> row, CompressedHeader header){
            row.Add(header.DistanceParameters.PostfixBitCount);
            row.Add(header.DistanceParameters.DirectCodeCount);
            row.Add(header.BlockTypes[Category.Distance].TypeCount);
            row.Add(header.DistanceTrees.Count);
        }

        private static void ExtractDistanceContextMetadata(List<object> row, CompressedHeader header, CompressedData data, int contextID){
            row.Add(data.InsertCopyCommands.Count(ic => ic.Lengths.DistanceContextID == contextID));

            var distanceParams = header.DistanceParameters;
            int complexCodeOffset = DistanceCode.Last.CodeCount + distanceParams.DirectCodeCount;

            var complexCodeLengths = new List<byte>[distanceParams.AlphabetSize.SymbolCount - complexCodeOffset];

            for(int index = 0; index < complexCodeLengths.Length; index++){
                complexCodeLengths[index] = new List<byte>();
            }

            for(int blockID = 0, blockTypes = header.BlockTypes[Category.Distance].TypeCount; blockID < blockTypes; blockID++){
                var tree = header.DistanceTrees[header.DistanceCtxMap.DetermineTreeID(blockID, contextID)];

                foreach(var (code, path) in tree){
                    if (code is DistanceCode.Complex complex){
                        complexCodeLengths[complex.Code - complexCodeOffset].Add(path.Length);
                    }
                }
            }

            var complexCodeLengthAverages = complexCodeLengths.Select(lengths => {
                var count = lengths.Count;
                return count == 0 ? 0.0 : (double)(lengths.Sum(l => (decimal)l) / count);
            }).ToArray();

            var nonZeroAverages = complexCodeLengthAverages.Where(avg => avg > 0.0).ToArray();
            row.Add(nonZeroAverages.Length == 0 ? 0.0 : nonZeroAverages.Average());

            row.AddRange(complexCodeLengthAverages.Cast<object>());
        }
    }
}
