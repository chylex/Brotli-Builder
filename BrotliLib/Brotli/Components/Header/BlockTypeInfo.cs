using BrotliLib.Brotli.Components.Utils;
using BrotliLib.IO;
using System.Collections.Generic;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Markers;
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

        // Object

        public override bool Equals(object obj){
            return obj is BlockTypeInfo info &&
                   Count == info.Count &&
                   InitialLength == info.InitialLength &&
                   EqualityComparer<BlockTypeCodeTree>.Default.Equals(TypeCodeTree, info.TypeCodeTree) &&
                   EqualityComparer<BlockLengthCodeTree>.Default.Equals(LengthCodeTree, info.LengthCodeTree);
        }

        public override int GetHashCode(){
            unchecked{
                var hashCode = 1953717823;
                hashCode = hashCode * -1521134295 + Count.GetHashCode();
                hashCode = hashCode * -1521134295 + InitialLength.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<BlockTypeCodeTree>.Default.GetHashCode(TypeCodeTree);
                hashCode = hashCode * -1521134295 + EqualityComparer<BlockLengthCodeTree>.Default.GetHashCode(LengthCodeTree);
                return hashCode;
            }
        }

        // Serialization

        private static BlockTypeCodeTree.Context GetBlockTypeCodeTreeContext(int count){
            return new BlockTypeCodeTree.Context(new AlphabetSize(count + 2), value => value, symbol => symbol);
        }

        public static readonly IBitSerializer<BlockTypeInfo, Category?> Serializer = new MarkedBitSerializer<BlockTypeInfo, Category?>(
            markerTitle: context => "Block Type Info (" + context + ")",

            fromBits: (reader, context) => {
                char cat = context.Value.Id();
                
                int count = reader.ReadValue(VariableLength11Code.Serializer, NoContext.Value, "NBLTYPES" + cat, value => value.Value);
                
                if (count == 1){
                    return Empty;
                }
                
                var blockTypeCode = reader.ReadStructure(BlockTypeCodeTree.Serializer, GetBlockTypeCodeTreeContext(count), "HTREE_BTYPE_" + cat);
                var blockLengthCode = reader.ReadStructure(BlockLengthCodeTree.Serializer, BlockLengthCode.TreeContext, "HTREE_BLEN_" + cat);
                
                var initialLengthCode = reader.ReadValue(blockLengthCode.Root, "BLEN_" + cat + " (code)");
                var initialLength = reader.ReadValue(BlockLengthCode.Serializer, initialLengthCode, "BLEN_" + cat + " (value)");
                
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
