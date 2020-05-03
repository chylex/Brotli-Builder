using System.Collections.Generic;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Utils;
using BrotliLib.Serialization;
using BrotliLib.Serialization.Writer;

namespace BrotliCalc.Commands{
    class CmdReserializeCountBitsContextMaps : CmdAbstractFileTable.Compressed{
        public override string FullName => "reserialize-count-bits-context-maps";
        public override string ShortName => "rcbcm";

        protected override string WorkDesc => "Reserialized and counted context map bits in";

        protected override string[] Columns { get; } = {
            "File", "Quality", "Meta-Block ID", "Category", "Block Types", "Huffman Trees", "Context Map Bits"
        };

        protected override IEnumerable<object[]> GenerateRows(BrotliFileGroup group, BrotliFile.Compressed file){
            var quality = file.Identifier;
            var reader = file.Reader;

            MetaBlock? metaBlock;
            int index = 0;

            while((metaBlock = reader.NextMetaBlock()) != null){
                if (metaBlock is MetaBlock.Compressed c){
                    var literalCtxMap = c.Header.LiteralCtxMap;
                    var distanceCtxMap = c.Header.DistanceCtxMap;

                    yield return new object[]{ file.Name, quality, index, literalCtxMap.Category.Id(),  literalCtxMap.BlockTypes,  literalCtxMap.TreeCount,  CountContextMapBits(literalCtxMap)  };
                    yield return new object[]{ file.Name, quality, index, distanceCtxMap.Category.Id(), distanceCtxMap.BlockTypes, distanceCtxMap.TreeCount, CountContextMapBits(distanceCtxMap) };
                }

                ++index;
            }
        }

        private static int CountContextMapBits(ContextMap contextMap){
            var writer = new BitWriterNull();
            ContextMap.Serialize(writer, contextMap, NoContext.Value, Parameters.Serialization);
            return writer.Length;
        }
    }
}
