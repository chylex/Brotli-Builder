﻿using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Components.Utils;
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

            IReadOnlyList<LiteralContextMode> literalCtxModes,
            ContextMap literalCtxMap,
            ContextMap distanceCtxMap,

            IReadOnlyList<LiteralTree> literalTrees,
            IReadOnlyList<InsertCopyTree> insertCopyTrees,
            IReadOnlyList<DistanceTree> distanceTrees
        ){
            this.BlockTypes = blockTypes;
            this.DistanceParameters = distanceParameters;
            this.LiteralCtxModes = literalCtxModes;
            this.LiteralCtxMap = literalCtxMap;
            this.DistanceCtxMap = distanceCtxMap;
            this.LiteralTrees = literalTrees;
            this.InsertCopyTrees = insertCopyTrees;
            this.DistanceTrees = distanceTrees;
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
        
        private static ContextMap ReadContextMap(BitReader reader, Category category, CategoryMap<BlockTypeInfo> blockTypes){
            return ContextMap.Serializer.FromBits(reader, blockTypes.Pick(category));
        }

        private static IReadOnlyList<HuffmanTree<T>> ReadHuffmanTrees<T>(BitReader reader, int treeCount, HuffmanTree<T>.Context context) where T : IComparable<T>{
            return Enumerable.Range(0, treeCount).Select(_ => HuffmanTree<T>.Serializer.FromBits(reader, context)).ToArray();
        }

        internal static readonly IBitSerializer<MetaBlockCompressionHeader, MetaBlock.Context> Serializer = new BitSerializer<MetaBlockCompressionHeader, MetaBlock.Context>(
            fromBits: (reader, context) => {
                var blockTypes = new CategoryMap<BlockTypeInfo>(_ => BlockTypeInfo.Serializer.FromBits(reader, NoContext.Value));
                var distanceParameters = DistanceParameters.Serializer.FromBits(reader, NoContext.Value);
                
                var literalCtxModes = Enumerable.Range(0, blockTypes[Category.Literal].Count).Select(_ => LiteralContextModes.Serializer.FromBits(reader, NoContext.Value)).ToArray();
                var literalCtxMap   = ReadContextMap(reader, Category.Literal, blockTypes);
                var distanceCtxMap  = ReadContextMap(reader, Category.Distance, blockTypes);
                
                var literalTrees    = ReadHuffmanTrees(reader, literalCtxMap.TreeCount, Literal.TreeContext);
                var insertCopyTrees = ReadHuffmanTrees(reader, blockTypes[Category.InsertCopy].Count, InsertCopyLengthCode.TreeContext);
                var distanceTrees   = ReadHuffmanTrees(reader, distanceCtxMap.TreeCount, DistanceCode.GenerateTreeContext(distanceParameters));
                
                return new MetaBlockCompressionHeader(blockTypes, distanceParameters, literalCtxModes, literalCtxMap, distanceCtxMap, literalTrees, insertCopyTrees, distanceTrees);
            },

            toBits: (writer, obj, context) => {
                foreach(BlockTypeInfo blockTypeInfo in obj.BlockTypes.Values){
                    BlockTypeInfo.Serializer.ToBits(writer, blockTypeInfo, NoContext.Value);
                }

                DistanceParameters.Serializer.ToBits(writer, obj.DistanceParameters, NoContext.Value);

                foreach(LiteralContextMode literalCtxMode in obj.LiteralCtxModes){
                    LiteralContextModes.Serializer.ToBits(writer, literalCtxMode, NoContext.Value);
                }

                ContextMap.Serializer.ToBits(writer, obj.LiteralCtxMap, obj.BlockTypes.Pick(Category.Literal));
                ContextMap.Serializer.ToBits(writer, obj.DistanceCtxMap, obj.BlockTypes.Pick(Category.Distance));
                
                foreach(LiteralTree tree in obj.LiteralTrees){
                    LiteralTree.Serializer.ToBits(writer, tree, Literal.TreeContext);
                }

                foreach(InsertCopyTree tree in obj.InsertCopyTrees){
                    InsertCopyTree.Serializer.ToBits(writer, tree, InsertCopyLengthCode.TreeContext);
                }

                foreach(DistanceTree tree in obj.DistanceTrees){
                    DistanceTree.Serializer.ToBits(writer, tree, DistanceCode.GenerateTreeContext(obj.DistanceParameters));
                }
            }
        );
    }
}