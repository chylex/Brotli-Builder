﻿using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Brotli.Utils;
using BrotliLib.Collections;
using BrotliLib.Markers.Serialization;
using BrotliLib.Markers.Serialization.Reader;
using BrotliLib.Serialization;
using BrotliLib.Serialization.Reader;
using LiteralTree    = BrotliLib.Brotli.Components.Header.HuffmanTree<BrotliLib.Brotli.Components.Data.Literal>;
using InsertCopyTree = BrotliLib.Brotli.Components.Header.HuffmanTree<BrotliLib.Brotli.Components.Data.InsertCopyLengthCode>;
using DistanceTree   = BrotliLib.Brotli.Components.Header.HuffmanTree<BrotliLib.Brotli.Components.Data.DistanceCode>;

namespace BrotliLib.Brotli.Components.Compressed{
    public sealed class CompressedHeader{
        public CategoryMap<BlockTypeInfo> BlockTypes { get; }
        public DistanceParameters DistanceParameters { get; }

        public IReadOnlyList<LiteralContextMode> LiteralCtxModes { get; }
        public ContextMap LiteralCtxMap { get; }
        public ContextMap DistanceCtxMap { get; }

        public IReadOnlyList<LiteralTree> LiteralTrees { get; }
        public IReadOnlyList<InsertCopyTree> InsertCopyTrees { get; }
        public IReadOnlyList<DistanceTree> DistanceTrees { get; }

        public CompressedHeader(
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
            return obj is CompressedHeader header &&
                   BlockTypes.Equals(header.BlockTypes) &&
                   DistanceParameters.Equals(header.DistanceParameters) &&
                   CollectionHelper.Equal(LiteralCtxModes, header.LiteralCtxModes) &&
                   LiteralCtxMap.Equals(header.LiteralCtxMap) &&
                   DistanceCtxMap.Equals(header.DistanceCtxMap) &&
                   CollectionHelper.Equal(LiteralTrees, header.LiteralTrees) &&
                   CollectionHelper.Equal(InsertCopyTrees, header.InsertCopyTrees) &&
                   CollectionHelper.Equal(DistanceTrees, header.DistanceTrees);
        }

        public override int GetHashCode(){
            return HashCode.Combine(
                BlockTypes, DistanceParameters,
                CollectionHelper.HashCode(LiteralCtxModes), LiteralCtxMap, DistanceCtxMap,
                CollectionHelper.HashCode(LiteralTrees), CollectionHelper.HashCode(InsertCopyTrees), CollectionHelper.HashCode(DistanceTrees)
            );
        }
        
        // Serialization

        private static LiteralContextMode[] ReadLiteralContextModes(IMarkedBitReader reader, int modeCount){
            return reader.MarkTitle("Literal Context Modes", () => reader.ReadValueArray(modeCount, "CMODE", () => LiteralContextModes.Deserialize(reader, NoContext.Value)));
        }
        
        private static ContextMap ReadContextMap(IBitReader reader, Category category, CategoryMap<BlockTypeInfo> blockTypes){
            return ContextMap.Deserialize(reader, blockTypes[category]);
        }

        private static IList<HuffmanTree<T>> ReadHuffmanTrees<T>(IMarkedBitReader reader, Category category, int treeCount, HuffmanTree<T>.Context context) where T : IComparable<T>{
            return reader.ReadStructureArray(treeCount, HuffmanTree<T>.Deserialize, context, "HTREE" + category.Id());
        }
        
        public static readonly BitDeserializer<CompressedHeader, NoContext> Deserialize = MarkedBitDeserializer.Wrap<CompressedHeader, NoContext>(
            (reader, context) => {
                var blockTypes = new CategoryMap<BlockTypeInfo>(category => BlockTypeInfo.Deserialize(reader, category));
                var distanceParameters = DistanceParameters.Deserialize(reader, NoContext.Value);
                
                var literalCtxModes = ReadLiteralContextModes(reader, blockTypes[Category.Literal].TypeCount);
                var literalCtxMap   = ReadContextMap(reader, Category.Literal, blockTypes);
                var distanceCtxMap  = ReadContextMap(reader, Category.Distance, blockTypes);
                
                var literalTrees    = ReadHuffmanTrees(reader, Category.Literal, literalCtxMap.TreeCount, Literal.TreeContext);
                var insertCopyTrees = ReadHuffmanTrees(reader, Category.InsertCopy, blockTypes[Category.InsertCopy].TypeCount, InsertCopyLengthCode.TreeContext);
                var distanceTrees   = ReadHuffmanTrees(reader, Category.Distance, distanceCtxMap.TreeCount, DistanceCode.GetTreeContext(distanceParameters));
                
                return new CompressedHeader(blockTypes, distanceParameters, literalCtxModes, literalCtxMap, distanceCtxMap, literalTrees, insertCopyTrees, distanceTrees);
            }
        );

        public static readonly BitSerializer<CompressedHeader, NoContext, BrotliSerializationParameters> Serialize = (writer, obj, context, parameters) => {
            foreach(BlockTypeInfo blockTypeInfo in obj.BlockTypes.Values){
                BlockTypeInfo.Serialize(writer, blockTypeInfo, NoContext.Value, parameters);
            }

            DistanceParameters.Serialize(writer, obj.DistanceParameters, NoContext.Value);

            foreach(LiteralContextMode literalCtxMode in obj.LiteralCtxModes){
                LiteralContextModes.Serialize(writer, literalCtxMode, NoContext.Value);
            }

            ContextMap.Serialize(writer, obj.LiteralCtxMap, NoContext.Value, parameters);
            ContextMap.Serialize(writer, obj.DistanceCtxMap, NoContext.Value, parameters);
            
            foreach(LiteralTree tree in obj.LiteralTrees){
                LiteralTree.Serialize(writer, tree, Literal.TreeContext, parameters);
            }

            foreach(InsertCopyTree tree in obj.InsertCopyTrees){
                InsertCopyTree.Serialize(writer, tree, InsertCopyLengthCode.TreeContext, parameters);
            }

            foreach(DistanceTree tree in obj.DistanceTrees){
                DistanceTree.Serialize(writer, tree, DistanceCode.GetTreeContext(obj.DistanceParameters), parameters);
            }
        };
    }
}
