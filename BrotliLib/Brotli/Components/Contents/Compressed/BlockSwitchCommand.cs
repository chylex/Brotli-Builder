using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Markers;
using BrotliLib.IO;

namespace BrotliLib.Brotli.Components.Contents.Compressed{
    public sealed class BlockSwitchCommand{
        public int TypeCode { get; }
        public int Length { get; }
        
        public BlockSwitchCommand(int typeCode, int length){
            this.TypeCode = typeCode;
            this.Length = length;
        }

        // Object

        public override bool Equals(object obj){
            return obj is BlockSwitchCommand command &&
                   TypeCode == command.TypeCode &&
                   Length == command.Length;
        }

        public override int GetHashCode(){
            unchecked{
                var hashCode = 229038177;
                hashCode = hashCode * -1521134295 + TypeCode.GetHashCode();
                hashCode = hashCode * -1521134295 + Length.GetHashCode();
                return hashCode;
            }
        }

        // Serialization
        
        public static readonly IBitSerializer<BlockSwitchCommand, BlockTypeInfo> Serializer = new MarkedBitSerializer<BlockSwitchCommand, BlockTypeInfo>(
            markerTitle: "Block Switch Command",

            fromBits: (reader, context) => {
                int typeCode = reader.ReadValue(context.TypeCodeTree.Root, "BTYPE (code)");
                
                var lengthCode = reader.ReadValue(context.LengthCodeTree.Root, "BLEN (code)");
                int lengthValue = reader.ReadValue(BlockLengthCode.Serializer, lengthCode, "BLEN (value)");

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
