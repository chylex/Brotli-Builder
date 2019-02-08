using System;
using System.Collections.Generic;
using BrotliLib.Brotli.Components.Contents.Compressed;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Collections;
using BrotliLib.IO;
using BlockSwitchCommandMap = BrotliLib.Brotli.Components.Utils.CategoryMap<System.Collections.Generic.IReadOnlyList<BrotliLib.Brotli.Components.Contents.Compressed.BlockSwitchCommand>>;

namespace BrotliLib.Brotli.Components.Contents{
    public sealed class CompressedMetaBlockContents{
        public MetaBlockCompressionHeader Header { get; }
        public IReadOnlyList<InsertCopyCommand> InsertCopyCommands { get; }
        public BlockSwitchCommandMap BlockSwitchCommands { get; }

        public CompressedMetaBlockContents(MetaBlockCompressionHeader header, IReadOnlyList<InsertCopyCommand> insertCopyCommands, BlockSwitchCommandMap blockSwitchCommands){
            this.Header = header;
            this.InsertCopyCommands = insertCopyCommands;
            this.BlockSwitchCommands = blockSwitchCommands;
        }

        // Context

        internal abstract class DataContext : MetaBlock.Context{
            public MetaBlockCompressionHeader Header { get; }

            public bool NeedsMoreData => bytesWritten < MetaBlock.DataLength.UncompressedBytes;

            protected readonly CategoryMap<BlockTypeTracker> blockTrackers;
            private readonly RingBuffer<byte> recentOutput;
            private int bytesWritten;

            protected DataContext(MetaBlock.Context wrapped, MetaBlockCompressionHeader header) : base(wrapped.MetaBlock, wrapped.State){
                this.Header = header;
                this.blockTrackers = Header.BlockTypes.Select((_, info) => new BlockTypeTracker(info));
                this.recentOutput = new RingBuffer<byte>(0, 0);
            }

            public abstract int NextBlockID(Category category);

            public int NextLiteralContextID(LiteralContextMode mode){
                return mode.DetermineContextID(recentOutput.Front, recentOutput.Back);
            }

            public void WriteLiteral(byte literal){
                State.Output(literal);
                recentOutput.Push(literal);
                ++bytesWritten;
            }

            public void WriteCopy(int copyLength, int copyDistance, bool usedDistanceCodeZero){
                if (copyDistance < 0){
                    throw new InvalidOperationException("Distance cannot be negative.");
                }

                int maxDistance = State.MaxDistance;

                if (copyDistance <= maxDistance){
                    if (copyDistance != 0 && !usedDistanceCodeZero){
                        State.DistanceBuffer.Push(copyDistance);
                    }

                    for(int index = 0; index < copyLength; index++){
                        State.Output(State.GetByteAt(State.OutputSize - copyDistance));
                    }

                    bytesWritten += copyLength;
                }
                else{
                    byte[] word = State.Dictionary.ReadTransformed(copyLength, copyDistance - maxDistance - 1);

                    State.Output(word);
                    bytesWritten += word.Length;
                }

                recentOutput.Push(State.GetByteAt(State.OutputSize - 2));
                recentOutput.Push(State.GetByteAt(State.OutputSize - 1));
            }
        }

        private class ReaderDataContext : DataContext{
            public BlockSwitchCommandMap BlockSwitchCommands => blockSwitchCommands.Select<IReadOnlyList<BlockSwitchCommand>>((_, list) => list.AsReadOnly());
            
            private readonly BitReader reader;
            private readonly CategoryMap<List<BlockSwitchCommand>> blockSwitchCommands;

            public ReaderDataContext(MetaBlock.Context wrapped, MetaBlockCompressionHeader header, BitReader reader) : base(wrapped, header){
                this.reader = reader;
                this.blockSwitchCommands = new CategoryMap<List<BlockSwitchCommand>>(_ => new List<BlockSwitchCommand>());
            }

            public override int NextBlockID(Category category){
                BlockTypeTracker tracker = blockTrackers[category];
                BlockSwitchCommand nextCommand = tracker.ReadCommand(reader);

                if (nextCommand != null){
                    blockSwitchCommands[category].Add(nextCommand);
                }

                return tracker.CurrentID;
            }
        }

        private class WriterDataContext : DataContext{
            private readonly BitWriter writer;
            private readonly CategoryMap<Queue<BlockSwitchCommand>> blockSwitchQueues;

            public WriterDataContext(MetaBlock.Context wrapped, MetaBlockCompressionHeader header, BlockSwitchCommandMap blockSwitchCommands, BitWriter writer) : base(wrapped, header){
                this.writer = writer;
                this.blockSwitchQueues = blockSwitchCommands.Select((_, list) => new Queue<BlockSwitchCommand>(list));
            }

            public override int NextBlockID(Category category){
                BlockTypeTracker tracker = blockTrackers[category];
                tracker.WriteCommand(writer, blockSwitchQueues[category]);
                return tracker.CurrentID;
            }
        }

        // Serialization

        internal static readonly IBitSerializer<CompressedMetaBlockContents, MetaBlock.Context> Serializer = new BitSerializer<CompressedMetaBlockContents, MetaBlock.Context>(
            fromBits: (reader, context) => {
                MetaBlockCompressionHeader header = MetaBlockCompressionHeader.Serializer.FromBits(reader, context);

                ReaderDataContext dataContext = new ReaderDataContext(context, header, reader);
                List<InsertCopyCommand> icCommands = new List<InsertCopyCommand>();
                
                do{
                    icCommands.Add(InsertCopyCommand.Serializer.FromBits(reader, dataContext));
                }while(dataContext.NeedsMoreData);
                
                return new CompressedMetaBlockContents(header, icCommands, dataContext.BlockSwitchCommands);
            },

            toBits: (writer, obj, context) => {
                MetaBlockCompressionHeader.Serializer.ToBits(writer, obj.Header, context);
                
                DataContext dataContext = new WriterDataContext(context, obj.Header, obj.BlockSwitchCommands, writer);

                foreach(InsertCopyCommand icCommand in obj.InsertCopyCommands){
                    InsertCopyCommand.Serializer.ToBits(writer, icCommand, dataContext);
                }
            }
        );
    }
}
