using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli.Components.Contents.Compressed;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Brotli.Markers;
using BrotliLib.Brotli.Markers.Data;
using BrotliLib.IO;
using BlockSwitchCommandMap = BrotliLib.Brotli.Components.Utils.CategoryMap<System.Collections.Generic.IReadOnlyList<BrotliLib.Brotli.Components.Contents.Compressed.BlockSwitchCommand>>;
using BlockSwitchCommandMutableMap = BrotliLib.Brotli.Components.Utils.CategoryMap<System.Collections.Generic.IList<BrotliLib.Brotli.Components.Contents.Compressed.BlockSwitchCommand>>;

namespace BrotliLib.Brotli.Components.Contents{
    public sealed class CompressedMetaBlockContents{
        public MetaBlockCompressionHeader Header { get; }
        public IReadOnlyList<InsertCopyCommand> InsertCopyCommands { get; }
        public BlockSwitchCommandMap BlockSwitchCommands { get; }

        public CompressedMetaBlockContents(MetaBlockCompressionHeader header, IList<InsertCopyCommand> insertCopyCommands, BlockSwitchCommandMutableMap blockSwitchCommands){
            this.Header = header;
            this.InsertCopyCommands = insertCopyCommands.ToArray();
            this.BlockSwitchCommands = blockSwitchCommands.Select<IReadOnlyList<BlockSwitchCommand>>((_, list) => list.ToArray());
        }

        // Object

        public override bool Equals(object obj){
            return obj is CompressedMetaBlockContents contents &&
                   EqualityComparer<MetaBlockCompressionHeader>.Default.Equals(Header, contents.Header) &&
                   EqualityComparer<IReadOnlyList<InsertCopyCommand>>.Default.Equals(InsertCopyCommands, contents.InsertCopyCommands) &&
                   EqualityComparer<BlockSwitchCommandMap>.Default.Equals(BlockSwitchCommands, contents.BlockSwitchCommands);
        }

        public override int GetHashCode(){
            unchecked{
                var hashCode = -1778056541;
                hashCode = hashCode * -1521134295 + EqualityComparer<MetaBlockCompressionHeader>.Default.GetHashCode(Header);
                hashCode = hashCode * -1521134295 + EqualityComparer<IReadOnlyList<InsertCopyCommand>>.Default.GetHashCode(InsertCopyCommands);
                hashCode = hashCode * -1521134295 + EqualityComparer<BlockSwitchCommandMap>.Default.GetHashCode(BlockSwitchCommands);
                return hashCode;
            }
        }

        // Context

        internal abstract class DataContext : MetaBlock.Context{
            public MetaBlockCompressionHeader Header { get; }

            public bool NeedsMoreData => bytesWritten < MetaBlock.DataLength.UncompressedBytes;

            protected readonly CategoryMap<BlockSwitchTracker> blockTrackers;
            private int bytesWritten;

            protected DataContext(MetaBlock.Context wrapped, MetaBlockCompressionHeader header) : base(wrapped.MetaBlock, wrapped.State){
                this.Header = header;
                this.blockTrackers = Header.BlockTypes.Select((_, info) => new BlockSwitchTracker(info));
            }

            public abstract int NextBlockID(Category category);

            public void WriteLiteral(in Literal literal){
                State.OutputLiteral(literal);
                ++bytesWritten;
            }

            public void WriteCopy(int length, DistanceInfo distance){
                bytesWritten += State.OutputCopy(length, distance);
            }
        }

        private class ReaderDataContext : DataContext{
            public BlockSwitchCommandMutableMap BlockSwitchCommands { get; }
            
            private readonly MarkedBitReader reader;

            public ReaderDataContext(MetaBlock.Context wrapped, MetaBlockCompressionHeader header, MarkedBitReader reader) : base(wrapped, header){
                this.reader = reader;
                this.BlockSwitchCommands = new CategoryMap<IList<BlockSwitchCommand>>(_ => new List<BlockSwitchCommand>());
            }

            public override int NextBlockID(Category category){
                BlockSwitchTracker tracker = blockTrackers[category];
                BlockSwitchCommand nextCommand = tracker.ReadCommand(reader);

                if (nextCommand != null){
                    BlockSwitchCommands[category].Add(nextCommand);
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
                BlockSwitchTracker tracker = blockTrackers[category];
                tracker.WriteCommand(writer, blockSwitchQueues[category]);
                return tracker.CurrentID;
            }
        }

        // Serialization

        internal static readonly IBitSerializer<CompressedMetaBlockContents, MetaBlock.Context> Serializer = new MarkedBitSerializer<CompressedMetaBlockContents, MetaBlock.Context>(
            fromBits: (reader, context) => {
                MetaBlockCompressionHeader header = MetaBlockCompressionHeader.Serializer.FromBits(reader, context);

                ReaderDataContext dataContext = new ReaderDataContext(context, header, reader);
                List<InsertCopyCommand> icCommands = new List<InsertCopyCommand>();
                
                reader.MarkStart();
                
                do{
                    icCommands.Add(InsertCopyCommand.Serializer.FromBits(reader, dataContext));
                }while(dataContext.NeedsMoreData);

                reader.MarkEnd(new TitleMarker("Command List"));
                
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
