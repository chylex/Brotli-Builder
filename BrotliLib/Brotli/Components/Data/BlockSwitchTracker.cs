using System.Collections.Generic;
using BrotliLib.Brotli.Components.Contents.Compressed;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Serialization.Reader;
using BrotliLib.Serialization.Writer;

namespace BrotliLib.Brotli.Components.Data{
    /// <summary>
    /// Tracks the current block type. Its subtypes <see cref="Reading"/> and <see cref="Writing"/> allow reading and writing block-switch commands.
    /// </summary>
    public abstract class BlockSwitchTracker{
        protected readonly BlockSwitchCommand.Context context;

        protected int currentID;
        protected int remaining;
        
        protected BlockSwitchTracker(BlockTypeInfo info){
            this.context = info.Count == 1 ? null : new BlockSwitchCommand.Context(info, new BlockTypeTracker(info.Count));
            this.remaining = info.InitialLength;
        }
        
        protected void UpdateState(BlockSwitchCommand command){
            currentID = command.Type;
            remaining = command.Length;
        }

        public sealed class Reading : BlockSwitchTracker{
            public IList<BlockSwitchCommand> ReadCommands { get; } = new List<BlockSwitchCommand>();

            public Reading(BlockTypeInfo info) : base(info){}

            /// <summary>
            /// Reads a block-switch command if there are no more symbols in the current block type, then decreases the amount of remaining symbols. Returns <see cref="BlockSwitchTracker.currentID"/>.
            /// </summary>
            public int ReadCommand(IBitReader reader){
                if (remaining == 0){
                    BlockSwitchCommand nextCommand = BlockSwitchCommand.Deserialize(reader, context);
                    ReadCommands.Add(nextCommand);
                    UpdateState(nextCommand);
                }

                --remaining;
                return currentID;
            }
        }

        public sealed class Writing : BlockSwitchTracker{
            private readonly Queue<BlockSwitchCommand> queue;

            public Writing(BlockTypeInfo info, Queue<BlockSwitchCommand> queue) : base(info){
                this.queue = queue;
            }

            /// <summary>
            /// Writes a block-switch command if there are no more symbols in the current block type, then decreases the amount of remaining symbols. Returns <see cref="BlockSwitchTracker.currentID"/>.
            /// </summary>
            public int WriteCommand(IBitWriter writer){
                if (remaining == 0){
                    BlockSwitchCommand nextCommand = queue.Dequeue();
                    BlockSwitchCommand.Serialize(writer, nextCommand, context);
                    UpdateState(nextCommand);
                }

                --remaining;
                return currentID;
            }

            /// <summary>
            /// Simulates processing a block-switch command if there are no more symbols in the current block type, then decreases the amount of remaining symbols. Returns <see cref="BlockSwitchTracker.currentID"/>.
            /// </summary>
            public int SimulateCommand(){
                if (remaining == 0){
                    UpdateState(queue.Dequeue());
                }

                --remaining;
                return currentID;
            }
        }
    }
}
