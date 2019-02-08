using BrotliLib.Brotli.Components.Utils;
using BrotliLib.IO;
using System.Collections.Generic;
using BrotliLib.Brotli.Components.Data;
using BlockTypeCodeTree = BrotliLib.Brotli.Components.Header.HuffmanTree<int>;
using BlockLengthCodeTree = BrotliLib.Brotli.Components.Header.HuffmanTree<BrotliLib.Brotli.Components.Data.BlockLengthCode>;

namespace BrotliLib.Brotli.Components.Header{
    /// <summary>
    /// Describes information about block types of a <see cref="Category"/> within a meta-block.
    /// https://tools.ietf.org/html/rfc7932#section-6
    /// </summary>
    public sealed class BlockTypeInfo{
        public static readonly BlockTypeInfo Empty = new BlockTypeInfo();
        
        // Data

        public int Count { get; }
        public int InitialLength { get; }

        public BlockTypeCodeTree TypeCodeTree { get; }
        public BlockLengthCodeTree LengthCodeTree { get; }

        private BlockTypeInfo(){
            this.Count = 1;
            this.InitialLength = 16777216;
        }

        public BlockTypeInfo(int count, int initialLength, BlockTypeCodeTree typeCodeTree, BlockLengthCodeTree lengthCodeTree){
            this.Count = count;
            this.InitialLength = initialLength;
            this.TypeCodeTree = typeCodeTree;
            this.LengthCodeTree = lengthCodeTree;
        }
        
        public override int GetHashCode(){
            unchecked{
                var hashCode = -1945866543;
                hashCode = hashCode * -1521134295 + Count.GetHashCode();
                hashCode = hashCode * -1521134295 + InitialLength.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<BlockTypeCodeTree>.Default.GetHashCode(TypeCodeTree);
                hashCode = hashCode * -1521134295 + EqualityComparer<BlockLengthCodeTree>.Default.GetHashCode(LengthCodeTree);
                return hashCode;
            }
        }

        public override bool Equals(object obj){
            return obj is BlockTypeInfo other &&
                   Count == other.Count &&
                   InitialLength == other.InitialLength &&
                   EqualityComparer<BlockTypeCodeTree>.Default.Equals(TypeCodeTree, other.TypeCodeTree) &&
                   EqualityComparer<BlockLengthCodeTree>.Default.Equals(LengthCodeTree, other.LengthCodeTree);
        }

        // Serialization

        private static BlockTypeCodeTree.Context GetBlockTypeCodeTreeContext(int count){
            return new BlockTypeCodeTree.Context(new AlphabetSize(count + 2), value => value, symbol => symbol);
        }

        public static readonly IBitSerializer<BlockTypeInfo, NoContext> Serializer = new BitSerializer<BlockTypeInfo, NoContext>(
            fromBits: (reader, context) => {
                int count = VariableLength11Code.Serializer.FromBits(reader, NoContext.Value).Value;

                if (count == 1){
                    return Empty;
                }
                
                var blockTypeCode = BlockTypeCodeTree.Serializer.FromBits(reader, GetBlockTypeCodeTreeContext(count));
                var blockLengthCode = BlockLengthCodeTree.Serializer.FromBits(reader, BlockLengthCode.TreeContext);

                var initialLengthCode = blockLengthCode.Root.LookupValue(reader);
                var initialLength = BlockLengthCode.Serializer.FromBits(reader, initialLengthCode);

                return new BlockTypeInfo(count, initialLength, blockTypeCode, blockLengthCode);
            },

            toBits: (writer, obj, context) => {
                VariableLength11Code.Serializer.ToBits(writer, new VariableLength11Code(obj.Count), NoContext.Value);

                if (obj.Count == 1){
                    return;
                }

                BlockTypeCodeTree.Serializer.ToBits(writer, obj.TypeCodeTree, GetBlockTypeCodeTreeContext(obj.Count));
                BlockLengthCodeTree.Serializer.ToBits(writer, obj.LengthCodeTree, BlockLengthCode.TreeContext);

                var initialLength = obj.InitialLength;
                var initialLengthCode = obj.LengthCodeTree.FindEntry(code => code.CanEncodeValue(initialLength));

                writer.WriteBits(initialLengthCode.Value);
                BlockLengthCode.Serializer.ToBits(writer, initialLength, initialLengthCode.Key);
            }
        );
    }
}
