using System.Collections.Generic;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Utils;
using BrotliLib.Serialization.Reader;
using BrotliLib.Serialization.Writer;

namespace BrotliLib.Brotli.Components.Data{
    /// <summary>
    /// Tracks the current block type.
    /// </summary>
    public abstract class BlockSwitchTracker{
        protected readonly BlockSwitchCommand.Context? context;

        private int currentID;
        private int remaining;
        
        protected BlockSwitchTracker(BlockTypeInfo info){
            this.context = info.TypeCount == 1 ? null : new BlockSwitchCommand.Context(info, new BlockTypeTracker(info.TypeCount));
            this.remaining = info.InitialLength;
        }
        
        protected abstract BlockSwitchCommand GetNextCommand();
        
        /// <summary>
        /// Requests a block-switch command if there are no more symbols in the current block type, then decreases the amount of remaining symbols. Returns <see cref="BlockSwitchTracker.currentID"/>.
        /// </summary>
        public int Advance(){
            if (remaining == 0){
                var nextCommand = GetNextCommand();

                currentID = nextCommand.Type;
                remaining = nextCommand.Length;
            }

            --remaining;
            return currentID;
        }
        
        /// <summary>
        /// Tracks the current block type, reading commands from an <see cref="IBitReader"/> whenever the current block ends.
        /// </summary>
        public sealed class Reading : BlockSwitchTracker{
            public IList<BlockSwitchCommand> ReadCommands { get; } = new List<BlockSwitchCommand>();

            private readonly IBitReader reader;

            public Reading(BlockTypeInfo info, IBitReader reader) : base(info){
                this.reader = reader;
            }

            protected override BlockSwitchCommand GetNextCommand(){
                var nextCommand = BlockSwitchCommand.Deserialize(reader, context!);
                ReadCommands.Add(nextCommand);
                return nextCommand;
            }
        }
        
        /// <summary>
        /// Tracks the current block type, writing commands from a list into an <see cref="IBitWriter"/> whenever the current block ends.
        /// </summary>
        public sealed class Writing : BlockSwitchTracker{
            private readonly IBitWriter writer;
            private readonly IReadOnlyList<BlockSwitchCommand> queue;
            private int index;

            public Writing(BlockTypeInfo info, IBitWriter writer, IReadOnlyList<BlockSwitchCommand> queue) : base(info){
                this.writer = writer;
                this.queue = queue;
            }

            protected override BlockSwitchCommand GetNextCommand(){
                var nextCommand = queue[index++];
                BlockSwitchCommand.Serialize(writer, nextCommand, context!);
                return nextCommand;
            }
        }
        
        /// <summary>
        /// Tracks the current block type, pulling commands from a list whenever the current block ends.
        /// </summary>
        public sealed class Simulating : BlockSwitchTracker{
            private readonly IReadOnlyList<BlockSwitchCommand> queue;
            private int index;

            public Simulating(BlockTypeInfo info, IReadOnlyList<BlockSwitchCommand> queue) : base(info){
                this.queue = queue;
            }

            protected override BlockSwitchCommand GetNextCommand(){
                return queue[index++];
            }
        }
    }
}
