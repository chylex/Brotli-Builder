using System;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Utils;
using BrotliLib.Markers.Serialization;
using BrotliLib.Serialization;

namespace BrotliLib.Brotli.Components.Data{
    public sealed class BlockSwitchCommand{
        public byte Type { get; }
        public int Length { get; }
        
        public BlockSwitchCommand(byte type, int length){
            BlockLengthCode.CheckBounds(length);

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

        public override string ToString(){
            return "Type = " + Type + ", Length = " + Length;
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

                var typeCode = reader.ReadValue(info.TypeCodeTree!.Root, "BTYPE (code)");
                byte typeValue = reader.MarkValue("BTYPE (value)", () => context.Tracker.NextType(typeCode));
                
                var lengthCode = reader.ReadValue(info.LengthCodeTree!.Root, "BLEN (code)");
                int lengthValue = reader.ReadValue(BlockLengthCode.Deserialize, lengthCode, "BLEN (value)");

                return new BlockSwitchCommand(typeValue, lengthValue);
            }
        );

        public static readonly BitSerializer<BlockSwitchCommand, Context> Serialize = (writer, obj, context) => {
            var info = context.Info;

            var typeCodes = context.Tracker.FindCodes(obj.Type);
            writer.WriteBits(info.TypeCodeTree!.FindShortest(typeCodes, (code, available) => available.Contains(code)).Value);

            int lengthValue = obj.Length;
            var lengthEntry = info.LengthCodeTree!.FindShortest(lengthValue, (code, value) => code.CanEncodeValue(value));

            writer.WriteBits(lengthEntry.Value);
            BlockLengthCode.Serialize(writer, lengthValue, lengthEntry.Key);
        };
    }
}
