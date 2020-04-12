using System;
using System.Linq;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Utils;
using BrotliLib.Numbers;

namespace BrotliLib.Brotli.Encode.Build{
    public abstract class ContextMapBuilder{
        public sealed class Literals : ContextMapBuilder{
            public static readonly ContextMap Simple = new Literals(1).Build();

            public Literals(int blockTypeCount) : base(Category.Literal, blockTypeCount){}
            public Literals(CompressedMetaBlockBuilder builder) : this(builder.BlockTypes[Category.Literal].TypeCount){}
        }

        public sealed class Distances : ContextMapBuilder{
            public static readonly ContextMap Simple = new Distances(1).Build();

            public Distances(int blockTypeCount) : base(Category.Distance, blockTypeCount){}
            public Distances(CompressedMetaBlockBuilder builder) : this(builder.BlockTypes[Category.Distance].TypeCount){}
        }

        public static ContextMapBuilder For(BlockTypeInfo blockTypeInfo){
            return blockTypeInfo.Category switch{
                Category.Literal => new Literals(blockTypeInfo.TypeCount),
                Category.Distance => new Distances(blockTypeInfo.TypeCount),
                _ => throw new InvalidOperationException("Context maps can only be created for literals and distances."),
            };
        }

        // Implementation
        
        private readonly Category category;
        private readonly int blockTypeCount;
        private readonly int contextsPerBlockType;
        private readonly byte[] contextMap;

        public int Length => contextMap.Length;
        public int TreeCount => 1 + contextMap.Max();

        public byte this[int index]{
            get => contextMap[index];
            set => contextMap[index] = value;
        }

        private protected ContextMapBuilder(Category category, int blockTypeCount){
            this.category = category;
            this.blockTypeCount = blockTypeCount;

            this.contextsPerBlockType = category.Contexts();
            this.contextMap = new byte[blockTypeCount * contextsPerBlockType];
        }

        private void CheckBlockType(int blockType){
            if (blockType < 0 || blockType >= blockTypeCount){
                throw new ArgumentOutOfRangeException(nameof(blockType));
            }
        }

        private void CheckIndexRange(IntRange range){
            if (range.First < 0 || range.Last >= contextsPerBlockType){
                throw new ArgumentOutOfRangeException(nameof(range));
            }
        }

        /// <summary>
        /// Sets the tree ID for a particular <paramref name="blockType"/> and <paramref name="index"/>.
        /// </summary>
        public ContextMapBuilder Set(int blockType, int index, byte value){
            CheckBlockType(blockType);
            CheckIndexRange(IntRange.Only(index));

            contextMap[blockType * contextsPerBlockType + index] = value;
            return this;
        }

        /// <summary>
        /// Sets the tree ID for a particular <paramref name="blockType"/> and a <paramref name="range"/> of indices.
        /// </summary>
        public ContextMapBuilder Set(int blockType, IntRange range, byte value){
            CheckBlockType(blockType);
            CheckIndexRange(range);

            int blockOffset = blockType * contextsPerBlockType;

            for(int index = range.First; index <= range.Last; index++){
                contextMap[blockOffset + index] = value;
            }

            return this;
        }

        /// <summary>
        /// Sets all tree IDs for a particular <paramref name="blockType"/>.
        /// </summary>
        public ContextMapBuilder Set(int blockType, byte[] values){
            CheckBlockType(blockType);

            if (values.Length != contextsPerBlockType){
                throw new ArgumentException("Context map definition has the wrong size (" + values.Length + " != " + contextsPerBlockType + ").", nameof(values));
            }

            Buffer.BlockCopy(values, 0, contextMap, blockType * contextsPerBlockType, contextsPerBlockType);
            return this;
        }

        /// <summary>
        /// Copies the first block type over all other block types.
        /// Adapted from https://github.com/google/brotli/blob/master/c/enc/metablock.c (MapStaticContexts).
        /// </summary>
        /// <param name="separateTreesPerBlockType">When true, each block type gets its own separate tree IDs. When false, the context map from first block type is copied over verbatim.</param>
        public ContextMapBuilder RepeatFirstBlockType(bool separateTreesPerBlockType){
            for(int blockType = 1; blockType < blockTypeCount; blockType++){
                Buffer.BlockCopy(contextMap, 0, contextMap, blockType * contextsPerBlockType, contextsPerBlockType);
            }

            if (separateTreesPerBlockType){
                int totalContexts = 1 + contextMap.Take(contextsPerBlockType).Max();

                if (totalContexts * blockTypeCount > 256){
                    throw new InvalidOperationException("Cannot define separate trees per block type, not enough space (" + (totalContexts * blockTypeCount) + " > 256).");
                }

                for(int blockType = 1; blockType < blockTypeCount; blockType++){
                    int blockOffset = blockType * contextsPerBlockType;
                    byte treeOffset = (byte)(blockType * totalContexts);

                    for(int index = 0; index < contextsPerBlockType; index++){
                        contextMap[blockOffset + index] += treeOffset;
                    }
                }
            }

            return this;
        }

        public ContextMap Build(){
            return new ContextMap(category, contextMap);
        }
    }
}
