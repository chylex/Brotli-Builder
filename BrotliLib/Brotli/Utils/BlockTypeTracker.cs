using System.Collections.Generic;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Collections;

namespace BrotliLib.Brotli.Utils{
    /// <summary>
    /// Tracks the current block type, and allows converting between type codes and type values.
    /// </summary>
    public sealed class BlockTypeTracker{
        private static readonly BlockTypeCode Code0 = new BlockTypeCode(0);
        private static readonly BlockTypeCode Code1 = new BlockTypeCode(1);

        private byte Code0Value => last.Back;
        private byte Code1Value => (byte)((1 + last.Front) % count);

        private readonly int count;
        private readonly RingBufferFast<byte> last;
        
        public BlockTypeTracker(int count){
            this.count = count;
            this.last = RingBufferFast<byte>.From(1, 0);
        }

        public List<BlockTypeCode> FindCodes(byte value){
            var list = new List<BlockTypeCode>(3);

            if (value == Code0Value){
                list.Add(Code0);
            }

            if (value == Code1Value){
                list.Add(Code1);
            }

            list.Add(new BlockTypeCode(2 + value));
            last.Push(value);
            return list;
        }

        public byte NextType(BlockTypeCode code){
            int id = code.Code;
            byte value = id switch{
                0 => Code0Value,
                1 => Code1Value,
                _ => (byte)(id - 2),
            };

            last.Push(value);
            return value;
        }
    }
}
