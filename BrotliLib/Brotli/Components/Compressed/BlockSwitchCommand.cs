using System;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Markers.Serialization;
using BrotliLib.Serialization;

namespace BrotliLib.Brotli.Components.Compressed{
    public sealed class BlockSwitchCommand{
        public int Type { get; }
        public int Length { get; }
        
        public BlockSwitchCommand(int type, int length){
            this.Type = type;
            this.Length = length;
        }

        // Object

        public override bool Equals(object obj){
            return obj is BlockSwitchCommand command &&
                   Type == command.Type &&
                   Length == command.Length;
        }

        public override int GetHashCode(){
            return HashCode.Combine(Type, Length);
        }

        // Context

        public sealed class Context{
            public BlockTypeInfo Info { get; }
            public BlockTypeTracker Tracker { get; }

            public Context(BlockTypeInfo info, BlockTypeTracker tracker){
                this.Info = info;
                this.Tracker = tracker;
            }
        }

        // Serialization
        
        public static readonly BitDeserializer<BlockSwitchCommand, Context> Deserialize = MarkedBitDeserializer.Title<BlockSwitchCommand, Context>(
            "Block Switch Command",

            (reader, context) => {
                var info = context.Info;

                int typeCode = reader.ReadValue(info.TypeCodeTree.Root, "BTYPE (code)");
                int typeValue = reader.MarkValue("BTYPE (value)", () => context.Tracker.FindValue(typeCode));
                
                var lengthCode = reader.ReadValue(info.LengthCodeTree.Root, "BLEN (code)");
                int lengthValue = reader.ReadValue(BlockLengthCode.Deserialize, lengthCode, "BLEN (value)");

                return new BlockSwitchCommand(typeValue, lengthValue);
            }
        );

        public static readonly BitSerializer<BlockSwitchCommand, Context> Serialize = (writer, obj, context) => {
            var info = context.Info;

            var typeCodes = context.Tracker.FindCodes(obj.Type);
            writer.WriteBits(info.TypeCodeTree.FindEntry(typeCodes.Contains).Value);

            int lengthValue = obj.Length;
            var lengthEntry = info.LengthCodeTree.FindEntry(entry => entry.CanEncodeValue(lengthValue));

            writer.WriteBits(lengthEntry.Value);
            BlockLengthCode.Serialize(writer, lengthValue, lengthEntry.Key);
        };
    }
}
