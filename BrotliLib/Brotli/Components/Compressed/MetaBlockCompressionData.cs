using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Collections;
using BrotliLib.Markers.Serialization;
using BrotliLib.Markers.Serialization.Reader;
using BrotliLib.Markers.Types;
using BrotliLib.Serialization;
using BrotliLib.Serialization.Writer;
using BlockSwitchCommandMap = BrotliLib.Brotli.Components.Utils.CategoryMap<System.Collections.Generic.IReadOnlyList<BrotliLib.Brotli.Components.Compressed.BlockSwitchCommand>>;
using BlockSwitchCommandMutableMap = BrotliLib.Brotli.Components.Utils.CategoryMap<System.Collections.Generic.IList<BrotliLib.Brotli.Components.Compressed.BlockSwitchCommand>>;

namespace BrotliLib.Brotli.Components.Compressed{
    public sealed class MetaBlockCompressionData{
        public IReadOnlyList<InsertCopyCommand> InsertCopyCommands { get; }
        public BlockSwitchCommandMap BlockSwitchCommands { get; }

        public MetaBlockCompressionData(IList<InsertCopyCommand> insertCopyCommands, BlockSwitchCommandMap blockSwitchCommands){
            this.InsertCopyCommands = insertCopyCommands.ToArray();
            this.BlockSwitchCommands = blockSwitchCommands;
        }

        public MetaBlockCompressionData(IList<InsertCopyCommand> insertCopyCommands, BlockSwitchCommandMutableMap blockSwitchCommands) :
            this(insertCopyCommands, blockSwitchCommands.Select<IReadOnlyList<BlockSwitchCommand>>(list => list.ToArray())){}

        // Object

        public override bool Equals(object obj){
            return obj is MetaBlockCompressionData contents &&
                   CollectionHelper.Equal(InsertCopyCommands, contents.InsertCopyCommands) &&
                   Categories.LID.All(category => CollectionHelper.Equal(BlockSwitchCommands[category], contents.BlockSwitchCommands[category]));
        }

        public override int GetHashCode(){
            return HashCode.Combine(CollectionHelper.HashCode(InsertCopyCommands), BlockSwitchCommands.Select(CollectionHelper.HashCode).GetHashCode());
        }

        // Context

        public class Context{
            public MetaBlockCompressionHeader Header { get; }
            public DataLength DataLength { get; }
            public BrotliGlobalState State { get; }

            public Context(MetaBlockCompressionHeader header, DataLength dataLength, BrotliGlobalState state){
                this.Header = header;
                this.DataLength = dataLength;
                this.State = state;
            }
        }

        internal abstract class DataContext : Context{
            public bool NeedsMoreData => bytesWritten < DataLength.UncompressedBytes;
            private int bytesWritten;

            protected DataContext(Context wrapped) : base(wrapped.Header, wrapped.DataLength, wrapped.State){}

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
            public BlockSwitchCommandMutableMap BlockSwitchCommands => blockTrackers.Select(tracker => tracker.ReadCommands);
            
            private readonly IMarkedBitReader reader;
            private readonly CategoryMap<BlockSwitchTracker.Reading> blockTrackers;

            public ReaderDataContext(Context wrapped, IMarkedBitReader reader) : base(wrapped){
                this.reader = reader;
                this.blockTrackers = Header.BlockTypes.Select(info => new BlockSwitchTracker.Reading(info));
            }

            public override int NextBlockID(Category category){
                return blockTrackers[category].ReadCommand(reader);
            }
        }

        private class WriterDataContext : DataContext{
            private readonly IBitWriter writer;
            private readonly CategoryMap<BlockSwitchTracker.Writing> blockTrackers;

            public WriterDataContext(Context wrapped, BlockSwitchCommandMap blockSwitchCommands, IBitWriter writer) : base(wrapped){
                this.writer = writer;
                this.blockTrackers = Header.BlockTypes.Select(info => new BlockSwitchTracker.Writing(info, new Queue<BlockSwitchCommand>(blockSwitchCommands[info.Category])));
            }

            public override int NextBlockID(Category category){
                return blockTrackers[category].WriteCommand(writer);
            }
        }

        // Serialization

        public static readonly BitDeserializer<MetaBlockCompressionData, Context> Deserialize = MarkedBitDeserializer.Wrap<MetaBlockCompressionData, Context>(
            (reader, context) => {
                ReaderDataContext dataContext = new ReaderDataContext(context, reader);
                List<InsertCopyCommand> icCommands = new List<InsertCopyCommand>();
                
                reader.MarkStart();
                
                do{
                    icCommands.Add(InsertCopyCommand.Deserialize(reader, dataContext));
                }while(dataContext.NeedsMoreData);

                reader.MarkEnd(() => new TitleMarker("Command List"));
                
                return new MetaBlockCompressionData(icCommands, dataContext.BlockSwitchCommands);
            }
        );

        public static readonly BitSerializer<MetaBlockCompressionData, Context> Serialize = (writer, obj, context) => {
            DataContext dataContext = new WriterDataContext(context, obj.BlockSwitchCommands, writer);

            foreach(InsertCopyCommand icCommand in obj.InsertCopyCommands){
                InsertCopyCommand.Serialize(writer, icCommand, dataContext);
            }
        };
    }
}
