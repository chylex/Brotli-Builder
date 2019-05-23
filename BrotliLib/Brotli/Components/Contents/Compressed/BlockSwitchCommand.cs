﻿using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Brotli.Markers;
using BrotliLib.IO;

namespace BrotliLib.Brotli.Components.Contents.Compressed{
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
            unchecked{
                var hashCode = 229038177;
                hashCode = hashCode * -1521134295 + Type.GetHashCode();
                hashCode = hashCode * -1521134295 + Length.GetHashCode();
                return hashCode;
            }
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
        
        public static readonly IBitSerializer<BlockSwitchCommand, Context> Serializer = new MarkedBitSerializer<BlockSwitchCommand, Context>(
            markerTitle: "Block Switch Command",

            fromBits: (reader, context) => {
                var info = context.Info;

                int typeCode = reader.ReadValue(info.TypeCodeTree.Root, "BTYPE (code)");
                int typeValue = context.Tracker.FindValue(typeCode);
                
                var lengthCode = reader.ReadValue(info.LengthCodeTree.Root, "BLEN (code)");
                int lengthValue = reader.ReadValue(BlockLengthCode.Serializer, lengthCode, "BLEN (value)");

                return new BlockSwitchCommand(typeValue, lengthValue);
            },

            toBits: (writer, obj, context) => {
                var info = context.Info;

                var typeCodes = context.Tracker.FindCodes(obj.Type);
                writer.WriteBits(info.TypeCodeTree.FindEntry(typeCodes.Contains).Value);

                int lengthValue = obj.Length;
                var lengthEntry = info.LengthCodeTree.FindEntry(entry => entry.CanEncodeValue(lengthValue));

                writer.WriteBits(lengthEntry.Value);
                BlockLengthCode.Serializer.ToBits(writer, lengthValue, lengthEntry.Key);
            }
        );
    }
}
