using System.Collections.Generic;
using BrotliLib.Brotli.Components.Contents.Compressed;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.IO;

namespace BrotliLib.Brotli.Components.Data{
    /// <summary>
    /// Tracks the current block type, and allows reading and writing block-switch commands.
    /// </summary>
    public sealed class BlockSwitchTracker{
        public int CurrentID { get; private set; } = 0;

        private readonly BlockSwitchCommand.Context context;
        private int remaining;
        
        public BlockSwitchTracker(BlockTypeInfo info){
            this.context = info.Count == 1 ? null : new BlockSwitchCommand.Context(info, new BlockTypeTracker(info.Count));
            this.remaining = info.InitialLength;
        }
        
        private void UpdateState(BlockSwitchCommand command){
            CurrentID = command.Type;
            remaining = command.Length;
        }

        /// <summary>
        /// Reads a block-switch command if there are no more symbols in the current block type, then decreases the amount of remaining symbols.
        /// </summary>
        public BlockSwitchCommand ReadCommand(BitReader reader){
            BlockSwitchCommand nextCommand = null;

            if (remaining == 0){
                nextCommand = BlockSwitchCommand.Deserialize(reader, context);
                UpdateState(nextCommand);
            }

            --remaining;
            return nextCommand;
        }

        /// <summary>
        /// Writes a block-switch command if there are no more symbols in the current block type, then decreases the amount of remaining symbols.
        /// </summary>
        public void WriteCommand(BitWriter writer, CategoryMap<Queue<BlockSwitchCommand>> commands){
            if (remaining == 0){
                BlockSwitchCommand nextCommand = commands[context.Info.Category].Dequeue();
                BlockSwitchCommand.Serialize(writer, nextCommand, context);
                UpdateState(nextCommand);
            }

            --remaining;
        }

        /// <summary>
        /// Simulates processing a block-switch command if there are no more symbols in the current block type, then decreases the amount of remaining symbols. Returns <see cref="CurrentID"/>.
        /// </summary>
        public int SimulateCommand(CategoryMap<Queue<BlockSwitchCommand>> commands){
            if (remaining == 0){
                UpdateState(commands[context.Info.Category].Dequeue());
            }

            --remaining;
            return CurrentID;
        }
    }
}
