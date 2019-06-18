using System;
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
        public static readonly CategoryMap<BlockTypeInfo> Empty = new CategoryMap<BlockTypeInfo>(category => new BlockTypeInfo(category));
        
        // Data

        public Category Category { get; }

        public int Count { get; }
        public int InitialLength { get; }

        public BlockTypeCodeTree TypeCodeTree { get; }
        public BlockLengthCodeTree LengthCodeTree { get; }

        private BlockTypeInfo(Category category){
            this.Category = category;
            this.Count = 1;
            this.InitialLength = 16777216;
        }

        public BlockTypeInfo(Category category, int count, int initialLength, BlockTypeCodeTree typeCodeTree, BlockLengthCodeTree lengthCodeTree){
            this.Category = category;
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

        public static readonly BitDeserializer<BlockTypeInfo, Category> Deserialize = MarkedBitDeserializer.Title<BlockTypeInfo, Category>(
            context => "Block Type Info (" + context + ")",

            (reader, context) => {
                char cid = context.Id();
                int count = reader.ReadValue(VariableLength11Code.Deserialize, NoContext.Value, "NBLTYPES" + cid, value => value.Value);
                
                if (count == 1){
                    return Empty[context];
                }
                
                var blockTypeCode = reader.ReadStructure(BlockTypeCodeTree.Deserialize, GetBlockTypeCodeTreeContext(count), "HTREE_BTYPE_" + cid);
                var blockLengthCode = reader.ReadStructure(BlockLengthCodeTree.Deserialize, BlockLengthCode.TreeContext, "HTREE_BLEN_" + cid);
                
                var initialLengthCode = reader.ReadValue(blockLengthCode.Root, "BLEN_" + cid + " (code)");
                var initialLength = reader.ReadValue(BlockLengthCode.Deserialize, initialLengthCode, "BLEN_" + cid + " (value)");
                
                return new BlockTypeInfo(context, count, initialLength, blockTypeCode, blockLengthCode);
            }
        );

        public static readonly BitSerializer<BlockTypeInfo, NoContext> Serialize = (writer, obj, context) => {
            VariableLength11Code.Serialize(writer, new VariableLength11Code(obj.Count), NoContext.Value);

            if (obj.Count == 1){
                return;
            }

            BlockTypeCodeTree.Serialize(writer, obj.TypeCodeTree, GetBlockTypeCodeTreeContext(obj.Count));
            BlockLengthCodeTree.Serialize(writer, obj.LengthCodeTree, BlockLengthCode.TreeContext);

            var initialLength = obj.InitialLength;
            var initialLengthCode = obj.LengthCodeTree.FindEntry(code => code.CanEncodeValue(initialLength));

            writer.WriteBits(initialLengthCode.Value);
            BlockLengthCode.Serialize(writer, initialLength, initialLengthCode.Key);
        };
    }
}
