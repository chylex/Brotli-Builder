using System.Collections.Generic;
using BrotliLib.Collections;

namespace BrotliLib.Brotli.Components.Data{
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

        public IList<int> FindCodes(int value){
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

        public int FindValue(int code){
            int value = code switch{
                0 => Code0Value,
                1 => Code1Value,
                _ => code - 2,
            };

            last.Push(value);
            return value;
        }
    }
}
