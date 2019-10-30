using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrotliLib.Brotli.Components.Contents.Compressed;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Brotli.Markers;
using BrotliLib.Brotli.Markers.Data;
using BrotliLib.Brotli.Markers.Reader;
using BrotliLib.Collections;
using BrotliLib.IO;
using BrotliLib.IO.Writer;
using BrotliLib.Markers;
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
            this.BlockSwitchCommands = blockSwitchCommands.Select<IReadOnlyList<BlockSwitchCommand>>(list => list.ToArray());
        }

        // Object

        public override bool Equals(object obj){
            return obj is CompressedMetaBlockContents contents &&
                   Header.Equals(contents.Header) &&
                   CollectionHelper.Equal(InsertCopyCommands, contents.InsertCopyCommands) &&
                   Categories.LID.All(category => CollectionHelper.Equal(BlockSwitchCommands[category], contents.BlockSwitchCommands[category]));
        }

        public override int GetHashCode(){
            return HashCode.Combine(Header, CollectionHelper.HashCode(InsertCopyCommands), BlockSwitchCommands.Select(CollectionHelper.HashCode).GetHashCode());
        }

        // Context

        internal abstract class DataContext : MetaBlock.Context{
            public MetaBlockCompressionHeader Header { get; }

            public bool NeedsMoreData => bytesWritten < MetaBlock.DataLength.UncompressedBytes;

            protected readonly CategoryMap<BlockSwitchTracker> blockTrackers;
            private int bytesWritten;

            protected DataContext(MetaBlock.Context wrapped, MetaBlockCompressionHeader header) : base(wrapped.MetaBlock, wrapped.State){
                this.Header = header;
                this.blockTrackers = Header.BlockTypes.Select(info => new BlockSwitchTracker(info));
            }

            public abstract int NextBlockID(Category category);

            public void WriteLiteral(in Literal literal){
                State.OutputLiteral(literal);
                ++bytesWritten;
            }

            public void WriteCopy(int length, DistanceInfo distance){
                bytesWritten += State.OutputCopy(length, distance).BytesWritten;
            }

            public void WriteCopyWithMarker(IMarkedBitReader reader, int length, DistanceInfo distance){
                reader.MarkCall(() => WriteCopyTracked(length, distance), GenerateCopyMarker);
            }

            private CopyOutputInfo WriteCopyTracked(int length, DistanceInfo distance){
                var info = State.OutputCopy(length, distance);

                bytesWritten += info.BytesWritten;
                return info;
            }

            private IMarkerInfo GenerateCopyMarker(CopyOutputInfo info){
                int written = Math.Min(info.BytesWritten, State.Parameters.WindowSize.Bytes); // State.GetOutput doesn't guarantee access past window size
                byte[] value = new byte[written + 2];

                for(int index = 0; index < written; index++){
                    value[index + 1] = State.GetOutput(written - index);
                }

                value[0] = value[written + 1] = (byte)'"';

                return new ValueMarker(info.IsBackReference ? "backreference" : "dictionary", Encoding.UTF8.GetString(value));
            }
        }

        private class ReaderDataContext : DataContext{
            public BlockSwitchCommandMutableMap BlockSwitchCommands { get; }
            
            private readonly IMarkedBitReader reader;

            public ReaderDataContext(MetaBlock.Context wrapped, MetaBlockCompressionHeader header, IMarkedBitReader reader) : base(wrapped, header){
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
            private readonly IBitWriter writer;
            private readonly CategoryMap<Queue<BlockSwitchCommand>> blockSwitchQueues;

            public WriterDataContext(MetaBlock.Context wrapped, MetaBlockCompressionHeader header, BlockSwitchCommandMap blockSwitchCommands, IBitWriter writer) : base(wrapped, header){
                this.writer = writer;
                this.blockSwitchQueues = blockSwitchCommands.Select(list => new Queue<BlockSwitchCommand>(list));
            }

            public override int NextBlockID(Category category){
                BlockSwitchTracker tracker = blockTrackers[category];
                tracker.WriteCommand(writer, blockSwitchQueues);
                return tracker.CurrentID;
            }
        }

        // Serialization

        internal static readonly BitDeserializer<CompressedMetaBlockContents, MetaBlock.Context> Deserialize = MarkedBitDeserializer.Wrap<CompressedMetaBlockContents, MetaBlock.Context>(
            (reader, context) => {
                MetaBlockCompressionHeader header = MetaBlockCompressionHeader.Deserialize(reader, NoContext.Value);

                ReaderDataContext dataContext = new ReaderDataContext(context, header, reader);
                List<InsertCopyCommand> icCommands = new List<InsertCopyCommand>();
                
                reader.MarkStart();
                
                do{
                    icCommands.Add(InsertCopyCommand.Deserialize(reader, dataContext));
                }while(dataContext.NeedsMoreData);

                reader.MarkEnd(new TitleMarker("Command List"));
                
                return new CompressedMetaBlockContents(header, icCommands, dataContext.BlockSwitchCommands);
            }
        );

        internal static readonly BitSerializer<CompressedMetaBlockContents, MetaBlock.Context> Serialize = (writer, obj, context) => {
            MetaBlockCompressionHeader.Serialize(writer, obj.Header, NoContext.Value);
            
            DataContext dataContext = new WriterDataContext(context, obj.Header, obj.BlockSwitchCommands, writer);

            foreach(InsertCopyCommand icCommand in obj.InsertCopyCommands){
                InsertCopyCommand.Serialize(writer, icCommand, dataContext);
            }
        };
    }
}
