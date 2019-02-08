using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.IO;

namespace BrotliLib.Brotli.Components.Contents.Compressed{
    public sealed class BlockSwitchCommand{
        public int TypeCode { get; }
        public int Length { get; }
        
        public BlockSwitchCommand(int typeCode, int length){
            this.TypeCode = typeCode;
            this.Length = length;
        }

        // Serialization
        
        public static readonly IBitSerializer<BlockSwitchCommand, BlockTypeInfo> Serializer = new BitSerializer<BlockSwitchCommand, BlockTypeInfo>(
            fromBits: (reader, context) => {
                int typeCode = context.TypeCodeTree.Root.LookupValue(reader);

                var lengthCode = context.LengthCodeTree.Root.LookupValue(reader);
                int lengthValue = BlockLengthCode.Serializer.FromBits(reader, lengthCode);

                return new BlockSwitchCommand(typeCode, lengthValue);
            },

            toBits: (writer, obj, context) => {
                writer.WriteBits(context.TypeCodeTree.FindPath(obj.TypeCode));

                int lengthValue = obj.Length;
                var lengthEntry = context.LengthCodeTree.FindEntry(entry => entry.CanEncodeValue(lengthValue));

                writer.WriteBits(lengthEntry.Value);
                BlockLengthCode.Serializer.ToBits(writer, lengthValue, lengthEntry.Key);
            }
        );
    }
}
