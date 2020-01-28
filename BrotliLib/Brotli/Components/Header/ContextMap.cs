using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Brotli.Utils;
using BrotliLib.Collections;
using BrotliLib.Markers.Serialization;
using BrotliLib.Numbers;
using BrotliLib.Serialization;

namespace BrotliLib.Brotli.Components.Header{
    /// <summary>
    /// Represents a context map, which maps &lt;blockID, contextID&gt; pairs to indices in an array of Huffman trees for either literals or distances.
    /// https://tools.ietf.org/html/rfc7932#section-7.3
    /// </summary>
    public sealed class ContextMap{
        public Category Category { get; }
        public int TreeCount { get; }
        
        private readonly int treesPerBlockType;
        private readonly byte[] contextMap;

        private ContextMap(Category category, int treeCount, int treesPerBlockType, byte[] contextMap){
            this.Category = category;
            this.TreeCount = treeCount;
            this.treesPerBlockType = treesPerBlockType;
            this.contextMap = contextMap;
        }

        public byte DetermineTreeID(int blockID, int contextID){
            return contextMap[blockID * treesPerBlockType + contextID];
        }

        // Object
        
        public override bool Equals(object obj){
            return obj is ContextMap map &&
                   Category == map.Category &&
                   TreeCount == map.TreeCount &&
                   CollectionHelper.Equal(contextMap, map.contextMap);
        }

        public override int GetHashCode(){
            return HashCode.Combine(Category, TreeCount, CollectionHelper.HashCode(contextMap));
        }

        public override string ToString(){
            return "TreeCount = " + TreeCount + ", Map = { " + string.Join(", ", contextMap) + " }";
        }

        // Types

        public sealed class Literals : Builder{
            public const int TreesPerBlockType = 64;
            public static readonly ContextMap Simple = new Literals(1).Build();

            public Literals(int blockTypeCount) : base(Category.Literal, TreesPerBlockType, blockTypeCount){}
        }

        public sealed class Distances : Builder{
            public const int TreesPerBlockType = 4;
            public static readonly ContextMap Simple = new Distances(1).Build();

            public Distances(int blockTypeCount) : base(Category.Distance, TreesPerBlockType, blockTypeCount){}
        }

        public static Builder For(BlockTypeInfo blockTypeInfo){
            return blockTypeInfo.Category switch{
                Category.Literal => new Literals(blockTypeInfo.TypeCount),
                Category.Distance => new Distances(blockTypeInfo.TypeCount),
                _ => throw new InvalidOperationException("Context maps can only be created for literals and distances."),
            };
        }

        // Builder

        public abstract class Builder{
            private readonly Category category;
            private readonly int treesPerBlockType;
            private readonly int blockTypeCount;
            private readonly byte[] contextMap;

            public int Length => contextMap.Length;
            public int TreeCount => 1 + contextMap.Max();

            public byte this[int index]{
                get => contextMap[index];
                set => contextMap[index] = value;
            }

            private protected Builder(Category category, int treesPerBlockType, int blockTypeCount){
                this.category = category;
                this.treesPerBlockType = treesPerBlockType;
                this.blockTypeCount = blockTypeCount;
                this.contextMap = new byte[blockTypeCount * treesPerBlockType];
            }

            private void CheckBlockType(int blockType){
                if (blockType < 0 || blockType >= blockTypeCount){
                    throw new ArgumentOutOfRangeException(nameof(blockType));
                }
            }

            private void CheckIndexRange(IntRange range){
                if (range.First < 0 || range.Last >= treesPerBlockType){
                    throw new ArgumentOutOfRangeException(nameof(range));
                }
            }

            /// <summary>
            /// Sets the tree ID for a particular <paramref name="blockType"/> and <paramref name="index"/>.
            /// </summary>
            public Builder Set(int blockType, int index, byte value){
                CheckBlockType(blockType);
                CheckIndexRange(IntRange.Only(index));

                contextMap[blockType * treesPerBlockType + index] = value;
                return this;
            }

            /// <summary>
            /// Sets the tree ID for a particular <paramref name="blockType"/> and a <paramref name="range"/> of indices.
            /// </summary>
            public Builder Set(int blockType, IntRange range, byte value){
                CheckBlockType(blockType);
                CheckIndexRange(range);

                int blockOffset = blockType * treesPerBlockType;

                for(int index = range.First; index <= range.Last; index++){
                    contextMap[blockOffset + index] = value;
                }

                return this;
            }

            /// <summary>
            /// Sets all tree IDs for a particular <paramref name="blockType"/>.
            /// </summary>
            public Builder Set(int blockType, byte[] values){
                CheckBlockType(blockType);

                if (values.Length != treesPerBlockType){
                    throw new ArgumentException("Context map definition has the wrong size (" + values.Length + " != " + treesPerBlockType + ").", nameof(values));
                }

                Buffer.BlockCopy(values, 0, contextMap, blockType * treesPerBlockType, treesPerBlockType);
                return this;
            }

            /// <summary>
            /// Copies the first block type over all other block types.
            /// Adapted from https://github.com/google/brotli/blob/master/c/enc/metablock.c (MapStaticContexts).
            /// </summary>
            /// <param name="separateTreesPerBlockType">When true, each block type gets its own separate tree IDs. When false, the context map from first block type is copied over verbatim.</param>
            public Builder RepeatFirstBlockType(bool separateTreesPerBlockType){
                for(int blockType = 1; blockType < blockTypeCount; blockType++){
                    Buffer.BlockCopy(contextMap, 0, contextMap, blockType * treesPerBlockType, treesPerBlockType);
                }

                if (separateTreesPerBlockType){
                    int totalContexts = 1 + contextMap.Take(treesPerBlockType).Max();

                    if (totalContexts * blockTypeCount > 256){
                        throw new InvalidOperationException("Cannot define separate trees per block type, not enough space (" + (totalContexts * blockTypeCount) + " > 256).");
                    }

                    for(int blockType = 1; blockType < blockTypeCount; blockType++){
                        int blockOffset = blockType * treesPerBlockType;
                        byte treeOffset = (byte)(blockType * totalContexts);

                        for(int index = 0; index < treesPerBlockType; index++){
                            contextMap[blockOffset + index] += treeOffset;
                        }
                    }
                }

                return this;
            }

            internal void Apply(Action<byte[]> action){
                action(contextMap);
            }

            public ContextMap Build(){
                return new ContextMap(category, TreeCount, treesPerBlockType, CollectionHelper.Clone(contextMap));
            }
        }
        
        // Helpers

        /// <summary>
        /// Returns how many zeroes there are in a sequence starting at <paramref name="startIndex"/> in the <paramref name="contextMap"/>.
        /// Returns <code>-1</code> if <paramref name="startIndex"/> points beyond the end of the <paramref name="contextMap"/>.
        /// </summary>
        private static int FindRunLength(byte[] contextMap, int startIndex){
            for(int index = startIndex; index < contextMap.Length + 1; index++){
                if (index == contextMap.Length || contextMap[index] != 0){
                    return index - startIndex;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns the RLE code required to encode the specified <paramref name="value"/>.
        /// The codes follow the pattern:
        /// <list type="bullet">
        /// <item><description>code 1 ... values 1-2,</description></item>
        /// <item><description>code 2 ... values 3-6,</description></item>
        /// <item><description>code 3 ... values 7-14,</description></item>
        /// <item><description>code 4 ... values 15-30,</description></item>
        /// <item><description>(up to code 16)</description></item>
        /// </list>
        /// </summary>
        private static byte CalculateRunLengthCodeFor(int value){
            return Log2.Floor(value + 1);
        }

        /// <summary>
        /// Returns the RLE code required to encode the longest sequence of zeroes in the <paramref name="contextMap"/>.
        /// </summary>
        private static byte CalculateLargestRunLengthCode(byte[] contextMap){
            int longestZeroSequence = 0;
            int lastStartIndex = 0;
            int lastRunLength;

            while((lastRunLength = FindRunLength(contextMap, lastStartIndex)) != -1){
                longestZeroSequence = Math.Max(longestZeroSequence, lastRunLength);
                lastStartIndex += lastRunLength + 1;
            }

            return longestZeroSequence > 1 ? CalculateRunLengthCodeFor(longestZeroSequence - 1) : (byte)0;
        }

        // Serialization

        private static HuffmanTree<int>.Context GetCodeTreeContext(int alphabetSize){
            return new HuffmanTree<int>.Context(new AlphabetSize(alphabetSize), bits => bits, symbol => symbol);
        }

        public static readonly BitDeserializer<ContextMap, BlockTypeInfo> Deserialize = MarkedBitDeserializer.Title<ContextMap, BlockTypeInfo>(
            context => "Context Map (" + context.Category + ")",

            (reader, context) => {
                int treeCount = reader.ReadValue(VariableLength11Code.Deserialize, NoContext.Value, "NTREES", value => value.Value);
                var contextMap = For(context);

                if (treeCount > 1){
                    byte runLengthCodeCount = (byte)reader.MarkValue("RLEMAX", () => reader.NextBit() ? 1 + reader.NextChunk(4) : 0);
                    
                    var codeContext = GetCodeTreeContext(treeCount + runLengthCodeCount);
                    var codeLookup = reader.ReadStructure(HuffmanTree<int>.Deserialize, codeContext, "code tree").Root;
                    
                    for(int index = 0; index < contextMap.Length; index++){
                        reader.MarkStart();

                        int code = codeLookup.LookupValue(reader);

                        if (code > 0){
                            if (code <= runLengthCodeCount){
                                index += (1 << code) - 1 + reader.NextChunk(code);

                                reader.MarkEndTitle("skip to index " + (index + 1));
                                continue;
                            }
                            else{
                                contextMap[index] = (byte)(code - runLengthCodeCount);
                            }
                        }
                        
                        reader.MarkEndValue("CMAP" + context.Category.Id() + "[" + index + "]", contextMap[index]);
                    }

                    if (reader.NextBit("IMTF")){
                        contextMap.Apply(MoveToFront.Decode);
                    }
                }

                return contextMap.Build();
            }
        );

        public static BitSerializer<ContextMap, BlockTypeInfo, BrotliSerializationParameters> Serialize = (writer, obj, context, parameters) => {
            VariableLength11Code.Serialize(writer, new VariableLength11Code(obj.TreeCount), NoContext.Value);

            if (obj.TreeCount > 1){
                bool imtf = parameters.UseContextMapIMTF(obj);
                byte[] contextMap;

                if (imtf){
                    contextMap = CollectionHelper.Clone(obj.contextMap);
                    MoveToFront.Encode(contextMap);
                }
                else{
                    contextMap = obj.contextMap;
                }

                byte runLengthCodeCount = parameters.UseContextMapRLE(obj) ? CalculateLargestRunLengthCode(contextMap) : (byte)0;
                
                if (runLengthCodeCount > 0){
                    writer.WriteBit(true);
                    writer.WriteChunk(4, runLengthCodeCount - 1);
                }
                else{
                    writer.WriteBit(false);
                }

                List<int> symbols = new List<int>();
                Queue<int> extra = new Queue<int>();

                for(int index = 0; index < contextMap.Length; index++){
                    byte symbol = contextMap[index];

                    if (symbol == 0){
                        int runLength = runLengthCodeCount == 0 ? 0 : FindRunLength(contextMap, index) - 1;

                        if (runLength > 0){
                            byte code = CalculateRunLengthCodeFor(runLength);

                            symbols.Add(code);
                            extra.Enqueue(runLength - ((1 << code) - 1));

                            index += runLength;
                        }
                        else{
                            symbols.Add(0);
                        }
                    }
                    else{
                        symbols.Add(symbol + runLengthCodeCount);
                    }
                }

                var codeContext = GetCodeTreeContext(obj.TreeCount + runLengthCodeCount);
                var codeTree = HuffmanTree<int>.FromSymbols(new FrequencyList<int>(symbols));

                HuffmanTree<int>.Serialize(writer, codeTree, codeContext, parameters);

                foreach(int symbol in symbols){
                    writer.WriteBits(codeTree.FindPath(symbol));

                    if (symbol > 0 && symbol <= runLengthCodeCount){
                        writer.WriteChunk(symbol, extra.Dequeue());
                    }
                }

                writer.WriteBit(imtf);
            }
        };
    }
}
