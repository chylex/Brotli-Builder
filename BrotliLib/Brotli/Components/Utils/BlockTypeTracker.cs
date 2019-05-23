using System.Collections.Generic;
using BrotliLib.Collections;

namespace BrotliLib.Brotli.Components.Utils{
    /// <summary>
    /// Tracks the current block type, and allows converting between type codes and type values.
    /// </summary>
    public sealed class BlockTypeTracker{
        private int Code0Value => last.Back;
        private int Code1Value => (1 + last.Front) % count;

        private readonly int count;
        private readonly RingBuffer<int> last;
        
        public BlockTypeTracker(int count){
            this.count = count;
            this.last = new RingBuffer<int>(1, 0);
        }

        public List<int> FindCodes(int value){
            List<int> list = new List<int>(3);

            if (value == Code0Value){
                list.Add(0);
            }

            if (value == Code1Value){
                list.Add(1);
            }

            list.Add(2 + value);
            last.Push(value);
            return list;
        }

        private int FindValueSilent(int code){
            switch(code){
                case 0: return Code0Value;
                case 1: return Code1Value;
                default: return code - 2;
            }
        }

        public int FindValue(int code){
            int value = FindValueSilent(code);
            last.Push(value);
            return value;
        }
    }
}
