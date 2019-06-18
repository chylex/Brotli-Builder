using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Brotli.Markers;
using BrotliLib.IO;
using LiteralTree    = BrotliLib.Brotli.Components.Header.HuffmanTree<BrotliLib.Brotli.Components.Data.Literal>;
using InsertCopyTree = BrotliLib.Brotli.Components.Header.HuffmanTree<BrotliLib.Brotli.Components.Data.InsertCopyLengthCode>;
using DistanceTree   = BrotliLib.Brotli.Components.Header.HuffmanTree<BrotliLib.Brotli.Components.Data.DistanceCode>;

namespace BrotliLib.Brotli.Components.Contents.Compressed{
    public sealed class MetaBlockCompressionHeader{
        public CategoryMap<BlockTypeInfo> BlockTypes { get; }
        public DistanceParameters DistanceParameters { get; }

        public IReadOnlyList<LiteralContextMode> LiteralCtxModes { get; }
        public ContextMap LiteralCtxMap { get; }
        public ContextMap DistanceCtxMap { get; }

        public IReadOnlyList<LiteralTree> LiteralTrees { get; }
        public IReadOnlyList<InsertCopyTree> InsertCopyTrees { get; }
        public IReadOnlyList<DistanceTree> DistanceTrees { get; }

        public MetaBlockCompressionHeader(
            CategoryMap<BlockTypeInfo> blockTypes,
            DistanceParameters distanceParameters,

            IList<LiteralContextMode> literalCtxModes,
            ContextMap literalCtxMap,
            ContextMap distanceCtxMap,

            IList<LiteralTree> literalTrees,
            IList<InsertCopyTree> insertCopyTrees,
            IList<DistanceTree> distanceTrees
        ){
            this.BlockTypes = blockTypes;
            this.DistanceParameters = distanceParameters;
            this.LiteralCtxModes = literalCtxModes.ToArray();
            this.LiteralCtxMap = literalCtxMap;
            this.DistanceCtxMap = distanceCtxMap;
            this.LiteralTrees = literalTrees.ToArray();
            this.InsertCopyTrees = insertCopyTrees.ToArray();
            this.DistanceTrees = distanceTrees.ToArray();
        }

        // Object

        public override bool Equals(object obj){
            return obj is MetaBlockCompressionHeader header &&
                   EqualityComparer<CategoryMap<BlockTypeInfo>>.Default.Equals(BlockTypes, header.BlockTypes) &&
                   EqualityComparer<DistanceParameters>.Default.Equals(DistanceParameters, header.DistanceParameters) &&
                   EqualityComparer<IReadOnlyList<LiteralContextMode>>.Default.Equals(LiteralCtxModes, header.LiteralCtxModes) &&
                   EqualityComparer<ContextMap>.Default.Equals(LiteralCtxMap, header.LiteralCtxMap) &&
                   EqualityComparer<ContextMap>.Default.Equals(DistanceCtxMap, header.DistanceCtxMap) &&
                   EqualityComparer<IReadOnlyList<LiteralTree>>.Default.Equals(LiteralTrees, header.LiteralTrees) &&
                   EqualityComparer<IReadOnlyList<InsertCopyTree>>.Default.Equals(InsertCopyTrees, header.InsertCopyTrees) &&
                   EqualityComparer<IReadOnlyList<DistanceTree>>.Default.Equals(DistanceTrees, header.DistanceTrees);
        }

        public override int GetHashCode(){
            unchecked{
                var hashCode = -301170250;
                hashCode = hashCode * -1521134295 + EqualityComparer<CategoryMap<BlockTypeInfo>>.Default.GetHashCode(BlockTypes);
                hashCode = hashCode * -1521134295 + EqualityComparer<DistanceParameters>.Default.GetHashCode(DistanceParameters);
                hashCode = hashCode * -1521134295 + EqualityComparer<IReadOnlyList<LiteralContextMode>>.Default.GetHashCode(LiteralCtxModes);
                hashCode = hashCode * -1521134295 + EqualityComparer<ContextMap>.Default.GetHashCode(LiteralCtxMap);
                hashCode = hashCode * -1521134295 + EqualityComparer<ContextMap>.Default.GetHashCode(DistanceCtxMap);
                hashCode = hashCode * -1521134295 + EqualityComparer<IReadOnlyList<LiteralTree>>.Default.GetHashCode(LiteralTrees);
                hashCode = hashCode * -1521134295 + EqualityComparer<IReadOnlyList<InsertCopyTree>>.Default.GetHashCode(InsertCopyTrees);
                hashCode = hashCode * -1521134295 + EqualityComparer<IReadOnlyList<DistanceTree>>.Default.GetHashCode(DistanceTrees);
                return hashCode;
            }
        }
        
        // Serialization

        private static LiteralContextMode[] ReadLiteralContextModes(MarkedBitReader reader, int modeCount){
            return reader.MarkTitle("Literal Context Modes", () => reader.ReadValueArray(modeCount, "CMODE", () => LiteralContextModes.Deserialize(reader, NoContext.Value)));
        }
        
        private static ContextMap ReadContextMap(BitReader reader, Category category, CategoryMap<BlockTypeInfo> blockTypes){
            return ContextMap.Deserialize(reader, blockTypes[category]);
        }

        private static IList<HuffmanTree<T>> ReadHuffmanTrees<T>(MarkedBitReader reader, Category category, int treeCount, HuffmanTree<T>.Context context) where T : IComparable<T>{
            return reader.ReadStructureArray(treeCount, HuffmanTree<T>.Deserialize, context, "HTREE" + category.Id());
        }
        
        internal static readonly BitDeserializer<MetaBlockCompressionHeader, NoContext> Deserialize = MarkedBitDeserializer.Wrap<MetaBlockCompressionHeader, NoContext>(
            (reader, context) => {
                var blockTypes = new CategoryMap<BlockTypeInfo>(category => BlockTypeInfo.Deserialize(reader, category));
                var distanceParameters = DistanceParameters.Deserialize(reader, NoContext.Value);
                
                var literalCtxModes = ReadLiteralContextModes(reader, blockTypes[Category.Literal].Count);
                var literalCtxMap   = ReadContextMap(reader, Category.Literal, blockTypes);
                var distanceCtxMap  = ReadContextMap(reader, Category.Distance, blockTypes);
                
                var literalTrees    = ReadHuffmanTrees(reader, Category.Literal, literalCtxMap.TreeCount, Literal.TreeContext);
                var insertCopyTrees = ReadHuffmanTrees(reader, Category.InsertCopy, blockTypes[Category.InsertCopy].Count, InsertCopyLengthCode.TreeContext);
                var distanceTrees   = ReadHuffmanTrees(reader, Category.Distance, distanceCtxMap.TreeCount, DistanceCode.GenerateTreeContext(distanceParameters));
                
                return new MetaBlockCompressionHeader(blockTypes, distanceParameters, literalCtxModes, literalCtxMap, distanceCtxMap, literalTrees, insertCopyTrees, distanceTrees);
            }
        );

        internal static readonly BitSerializer<MetaBlockCompressionHeader, NoContext> Serialize = (writer, obj, context) => {
            foreach(BlockTypeInfo blockTypeInfo in obj.BlockTypes.Values){
                BlockTypeInfo.Serialize(writer, blockTypeInfo, NoContext.Value);
            }

            DistanceParameters.Serialize(writer, obj.DistanceParameters, NoContext.Value);

            foreach(LiteralContextMode literalCtxMode in obj.LiteralCtxModes){
                LiteralContextModes.Serialize(writer, literalCtxMode, NoContext.Value);
            }

            var contextMapSerialize = ContextMap.MakeSerializer(imtf: true, rle: true);

            contextMapSerialize(writer, obj.LiteralCtxMap, obj.BlockTypes[Category.Literal]);
            contextMapSerialize(writer, obj.DistanceCtxMap, obj.BlockTypes[Category.Distance]);
            
            foreach(LiteralTree tree in obj.LiteralTrees){
                LiteralTree.Serialize(writer, tree, Literal.TreeContext);
            }

            foreach(InsertCopyTree tree in obj.InsertCopyTrees){
                InsertCopyTree.Serialize(writer, tree, InsertCopyLengthCode.TreeContext);
            }

            foreach(DistanceTree tree in obj.DistanceTrees){
                DistanceTree.Serialize(writer, tree, DistanceCode.GenerateTreeContext(obj.DistanceParameters));
            }
        };
    }
}
