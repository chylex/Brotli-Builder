using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Huffman;
using BrotliLib.IO;

namespace BrotliLib.Brotli.Components.Header{
    /// <summary>
    /// Represents a context map, which maps &lt;blockID, contextID&gt; pairs to indices in an array of Huffman trees for either literals or distances.
    /// https://tools.ietf.org/html/rfc7932#section-7.3
    /// </summary>
    public sealed class ContextMap{
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

        // Serialization

        private static HuffmanTree<int>.Context GetCodeTreeContext(int alphabetSize){
            return new HuffmanTree<int>.Context(new AlphabetSize(alphabetSize), bits => bits, symbol => symbol);
        }

        public static readonly IBitSerializer<ContextMap, KeyValuePair<Category, BlockTypeInfo>> Serializer = new BitSerializer<ContextMap, KeyValuePair<Category, BlockTypeInfo>>(
            fromBits: (reader, context) => {
                var (category, blockTypeInfo) = context;

                int treeCount = VariableLength11Code.Serializer.FromBits(reader, NoContext.Value).Value;
                int treesPerBlockType = category.HuffmanTreesPerBlockType();

                byte[] contextMap = new byte[treesPerBlockType * blockTypeInfo.Count];

                if (treeCount > 1){
                    byte runLengthCodeCount = (byte)(reader.NextBit() ? 1 + reader.NextChunk(4) : 0);

                    var codeContext = GetCodeTreeContext(treeCount + runLengthCodeCount);
                    var codeLookup = HuffmanTree<int>.Serializer.FromBits(reader, codeContext).Root;
                    
                    for(int index = 0; index < contextMap.Length; index++){
                        int code = codeLookup.LookupValue(reader);

                        if (code > 0){
                            if (code <= runLengthCodeCount){
                                index += (1 << code) + reader.NextChunk(code) - 1;
                            }
                            else{
                                contextMap[index] = (byte)(code - runLengthCodeCount);
                            }
                        }
                    }

                    if (reader.NextBit()){
                        MoveToFront.Decode(contextMap);
                    }
                }
                
                return new ContextMap(treeCount, treesPerBlockType, contextMap);
            },

            toBits: (writer, obj, context) => {
                VariableLength11Code.Serializer.ToBits(writer, new VariableLength11Code(obj.TreeCount), NoContext.Value);

                if (obj.TreeCount > 1){ // TODO implement RLE and IMTF
                    writer.WriteBit(false);

                    var codeContext = GetCodeTreeContext(obj.TreeCount);
                    var codeSymbols = obj.contextMap.GroupBy(value => (int)value).Select(HuffmanGenerator<int>.MakeFreq).ToArray();

                    var codeTree = HuffmanGenerator<int>.FromFrequenciesCanonical(codeSymbols, HuffmanTree<int>.DefaultMaxDepth);
                    var codeMap = codeTree.GenerateValueMap();

                    HuffmanTree<int>.Serializer.ToBits(writer, new HuffmanTree<int>(codeTree), codeContext);

                    foreach(byte symbol in obj.contextMap){
                        writer.WriteBits(codeMap[symbol]);
                    }

                    writer.WriteBit(false);
                }
            }
        );
    }
}
