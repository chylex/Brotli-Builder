using System.Collections.Generic;
using BrotliLib.Brotli.Components.Contents.Compressed;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Collections;
using BrotliLib.IO;

namespace BrotliLib.Brotli.Components.Data{
    /// <summary>
    /// Tracks the current block type, and allows reading and writing block-switch commands.
    /// </summary>
    public sealed class BlockTypeTracker{
        public int CurrentID { get; private set; }

        private readonly BlockTypeInfo info;
        private readonly RingBuffer<int> last;
        private int remaining;
        
        public BlockTypeTracker(BlockTypeInfo info){
            this.info = info;
            this.last = info.Count == 1 ? null : new RingBuffer<int>(1, 0);

            this.CurrentID = 0;
            this.remaining = info.InitialLength;
        }
        
        private void UpdateState(BlockSwitchCommand command){
            int newTypeCode = command.TypeCode;

            switch(newTypeCode){
                case 0: CurrentID = last.Back; break;
                case 1: CurrentID = (1 + last.Front) % info.Count; break;
                default: CurrentID = newTypeCode - 2; break;
            }

            last.Push(CurrentID);
            remaining = command.Length;
        }

        /// <summary>
        /// Reads a block-switch command if there are no more symbols in the current block type, then decreases the amount of remaining symbols.
        /// </summary>
        public BlockSwitchCommand ReadCommand(BitReader reader){
            BlockSwitchCommand nextCommand = null;

            if (remaining == 0){
                nextCommand = BlockSwitchCommand.Serializer.FromBits(reader, info);
                UpdateState(nextCommand);
            }

            --remaining;
            return nextCommand;
        }

        /// <summary>
        /// Writes a block-switch command if there are no more symbols in the current block type, then decreases the amount of remaining symbols.
        /// </summary>
        public void WriteCommand(BitWriter writer, Queue<BlockSwitchCommand> commands){
            if (remaining == 0){
                BlockSwitchCommand nextCommand = commands.Dequeue();
                BlockSwitchCommand.Serializer.ToBits(writer, nextCommand, info);
                UpdateState(nextCommand);
            }

            --remaining;
        }
    }
}
