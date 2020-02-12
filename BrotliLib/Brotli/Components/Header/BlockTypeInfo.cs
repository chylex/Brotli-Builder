using BrotliLib.Brotli.Components.Data;
using System;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Brotli.Utils;
using BrotliLib.Markers.Serialization;
using BrotliLib.Serialization;
using BlockTypeCodeTree = BrotliLib.Brotli.Components.Header.HuffmanTree<BrotliLib.Brotli.Components.Data.BlockTypeCode>;
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

        public int TypeCount { get; }
        public int InitialLength { get; }

        public BlockTypeCodeTree? TypeCodeTree { get; }
        public BlockLengthCodeTree? LengthCodeTree { get; }

        private BlockTypeInfo(Category category){
            this.Category = category;
            this.TypeCount = 1;
            this.InitialLength = 16777216;
        }

        public BlockTypeInfo(Category category, int typeCount, int initialLength, BlockTypeCodeTree typeCodeTree, BlockLengthCodeTree lengthCodeTree){
            this.Category = category;
            this.TypeCount = typeCount;
            this.InitialLength = initialLength;
            this.TypeCodeTree = typeCodeTree;
            this.LengthCodeTree = lengthCodeTree;
        }

        // Object

        public override bool Equals(object obj){
            return obj is BlockTypeInfo info &&
                   TypeCount == info.TypeCount &&
                   InitialLength == info.InitialLength &&
                   Equals(TypeCodeTree, info.TypeCodeTree) &&
                   Equals(LengthCodeTree, info.LengthCodeTree);
        }

        public override int GetHashCode(){
            return HashCode.Combine(TypeCount, InitialLength, TypeCodeTree, LengthCodeTree);
        }

        public override string ToString(){
            return ReferenceEquals(this, Empty[Category]) ? "Empty" : "TypeCount = " + TypeCount + ", InitialLength = " + InitialLength;
        }

        // Serialization

        public static readonly BitDeserializer<BlockTypeInfo, Category> Deserialize = MarkedBitDeserializer.Title<BlockTypeInfo, Category>(
            context => "Block Type Info (" + context + ")",

            (reader, context) => {
                char cid = context.Id();
                int count = reader.ReadValue(VariableLength11Code.Deserialize, NoContext.Value, "NBLTYPES" + cid, value => value.Value);
                
                if (count == 1){
                    return Empty[context];
                }
                
                var blockTypeCode = reader.ReadStructure(BlockTypeCodeTree.Deserialize, BlockTypeCode.GetTreeContext(count), "HTREE_BTYPE_" + cid);
                var blockLengthCode = reader.ReadStructure(BlockLengthCodeTree.Deserialize, BlockLengthCode.TreeContext, "HTREE_BLEN_" + cid);
                
                var initialLengthCode = reader.ReadValue(blockLengthCode.Root, "BLEN_" + cid + " (code)");
                var initialLength = reader.ReadValue(BlockLengthCode.Deserialize, initialLengthCode, "BLEN_" + cid + " (value)");
                
                return new BlockTypeInfo(context, count, initialLength, blockTypeCode, blockLengthCode);
            }
        );

        public static readonly BitSerializer<BlockTypeInfo, NoContext, BrotliSerializationParameters> Serialize = (writer, obj, context, parameters) => {
            VariableLength11Code.Serialize(writer, new VariableLength11Code(obj.TypeCount), NoContext.Value);

            if (obj.TypeCount == 1){
                return;
            }

            BlockTypeCodeTree.Serialize(writer, obj.TypeCodeTree!, BlockTypeCode.GetTreeContext(obj.TypeCount), parameters);
            BlockLengthCodeTree.Serialize(writer, obj.LengthCodeTree!, BlockLengthCode.TreeContext, parameters);

            var initialLength = obj.InitialLength;
            var initialLengthCode = obj.LengthCodeTree!.FindShortest(initialLength, (code, length) => code.CanEncodeValue(length));

            writer.WriteBits(initialLengthCode.Value);
            BlockLengthCode.Serialize(writer, initialLength, initialLengthCode.Key);
        };
    }
}
