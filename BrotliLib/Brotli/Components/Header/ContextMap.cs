using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Brotli.Markers;
using BrotliLib.Brotli.Markers.Data;
using BrotliLib.Huffman;
using BrotliLib.IO;

namespace BrotliLib.Brotli.Components.Header{
    /// <summary>
    /// Represents a context map, which maps &lt;blockID, contextID&gt; pairs to indices in an array of Huffman trees for either literals or distances.
    /// https://tools.ietf.org/html/rfc7932#section-7.3
    /// </summary>
    public sealed class ContextMap{
        private const bool EncodeRLE = true;
        private const bool EncodeIMTF = true;
        
        // Data

        public int TreeCount { get; }
        public int TreesPerBlockType { get; }

        private readonly byte[] contextMap;

        private ContextMap(int treeCount, int treesPerBlockType, byte[] contextMap){
            this.TreeCount = treeCount;
            this.TreesPerBlockType = treesPerBlockType;

            this.contextMap = contextMap;
        }

        public byte DetermineTreeID(int blockID, int contextID){
            return contextMap[blockID * TreesPerBlockType + contextID];
        }

        // Object

        public override bool Equals(object obj){
            return obj is ContextMap map &&
                   TreeCount == map.TreeCount &&
                   TreesPerBlockType == map.TreesPerBlockType &&
                   EqualityComparer<byte[]>.Default.Equals(contextMap, map.contextMap);
        }

        public override int GetHashCode(){
            unchecked{
                var hashCode = 1980284268;
                hashCode = hashCode * -1521134295 + TreeCount.GetHashCode();
                hashCode = hashCode * -1521134295 + TreesPerBlockType.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<byte[]>.Default.GetHashCode(contextMap);
                return hashCode;
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
            byte runLengthCode = 0;
            int remaining = (value + 1) >> 1;

            while(remaining > 0){
                remaining >>= 1;
                ++runLengthCode;
            }

            return runLengthCode;
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

        public static readonly IBitSerializer<ContextMap, KeyValuePair<Category, BlockTypeInfo>> Serializer = new MarkedBitSerializer<ContextMap, KeyValuePair<Category, BlockTypeInfo>>(
            markerTitle: context => "Context Map (" + context.Key + ")",

            fromBits: (reader, context) => {
                var (category, blockTypeInfo) = context;
                
                int treeCount = reader.ReadValue(VariableLength11Code.Serializer, NoContext.Value, "NTREES", value => value.Value);
                int treesPerBlockType = category.HuffmanTreesPerBlockType();

                byte[] contextMap = new byte[treesPerBlockType * blockTypeInfo.Count];

                if (treeCount > 1){
                    byte runLengthCodeCount = (byte)reader.MarkValue("RLEMAX", () => reader.NextBit() ? 1 + reader.NextChunk(4) : 0);
                    
                    var codeContext = GetCodeTreeContext(treeCount + runLengthCodeCount);
                    var codeLookup = reader.ReadStructure(HuffmanTree<int>.Serializer, codeContext, "code tree").Root;
                    
                    for(int index = 0; index < contextMap.Length; index++){
                        reader.MarkStart();

                        int code = codeLookup.LookupValue(reader);

                        if (code > 0){
                            if (code <= runLengthCodeCount){
                                index += (1 << code) + reader.NextChunk(code) - 1;

                                reader.MarkEnd(new TextMarker("skip to index " + (index + 1)));
                                continue;
                            }
                            else{
                                contextMap[index] = (byte)(code - runLengthCodeCount);
                            }
                        }
                        
                        reader.MarkEnd(new TextMarker("CMAP" + category.Id() + "[" + index + "]", contextMap[index]));
                    }

                    if (reader.NextBit("IMTF")){
                        MoveToFront.Decode(contextMap);
                    }
                }
                
                return new ContextMap(treeCount, treesPerBlockType, contextMap);
            },

            toBits: (writer, obj, context) => {
                VariableLength11Code.Serializer.ToBits(writer, new VariableLength11Code(obj.TreeCount), NoContext.Value);

                if (obj.TreeCount > 1){
                    byte[] contextMap;

                    if (EncodeIMTF){
                        contextMap = (byte[])obj.contextMap.Clone();
                        MoveToFront.Encode(contextMap);
                    }
                    else{
                        contextMap = obj.contextMap;
                    }

                    byte runLengthCodeCount = EncodeRLE ? CalculateLargestRunLengthCode(contextMap) : (byte)0;
                    
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
                    var codeSymbols = symbols.GroupBy(symbol => symbol).Select(HuffmanGenerator<int>.MakeFreq).ToArray();

                    var codeTree = HuffmanGenerator<int>.FromFrequenciesCanonical(codeSymbols, HuffmanTree<int>.DefaultMaxDepth);
                    var codeMap = codeTree.GenerateValueMap();

                    HuffmanTree<int>.Serializer.ToBits(writer, new HuffmanTree<int>(codeTree), codeContext);

                    foreach(int symbol in symbols){
                        writer.WriteBits(codeMap[symbol]);

                        if (symbol > 0 && symbol <= runLengthCodeCount){
                            writer.WriteChunk(symbol, extra.Dequeue());
                        }
                    }

                    writer.WriteBit(EncodeIMTF);
                }
            }
        );
    }
}
