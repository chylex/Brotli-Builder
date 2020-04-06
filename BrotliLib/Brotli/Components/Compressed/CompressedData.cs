using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Output;
using BrotliLib.Brotli.Utils;
using BrotliLib.Collections;
using BrotliLib.Markers.Serialization;
using BrotliLib.Markers.Serialization.Reader;
using BrotliLib.Markers.Types;
using BrotliLib.Serialization;
using BrotliLib.Serialization.Writer;
using BlockSwitchCommandMap = BrotliLib.Brotli.Utils.CategoryMap<System.Collections.Generic.IReadOnlyList<BrotliLib.Brotli.Components.Data.BlockSwitchCommand>>;
using BlockSwitchCommandMutableMap = BrotliLib.Brotli.Utils.CategoryMap<System.Collections.Generic.IList<BrotliLib.Brotli.Components.Data.BlockSwitchCommand>>;

namespace BrotliLib.Brotli.Components.Compressed{
    public sealed class CompressedData{
        public IReadOnlyList<InsertCopyCommand> InsertCopyCommands { get; }
        public BlockSwitchCommandMap BlockSwitchCommands { get; }

        public CompressedData(IList<InsertCopyCommand> insertCopyCommands, BlockSwitchCommandMap blockSwitchCommands){
            this.InsertCopyCommands = insertCopyCommands.ToArray();
            this.BlockSwitchCommands = blockSwitchCommands;
        }

        public CompressedData(IList<InsertCopyCommand> insertCopyCommands, BlockSwitchCommandMutableMap blockSwitchCommands) :
            this(insertCopyCommands, blockSwitchCommands.Select<IReadOnlyList<BlockSwitchCommand>>(list => list.ToArray())){}

        // Object

        public override bool Equals(object obj){
            return obj is CompressedData contents &&
                   CollectionHelper.Equal(InsertCopyCommands, contents.InsertCopyCommands) &&
                   Categories.LID.All(category => CollectionHelper.Equal(BlockSwitchCommands[category], contents.BlockSwitchCommands[category]));
        }

        public override int GetHashCode(){
            return HashCode.Combine(CollectionHelper.HashCode(InsertCopyCommands), BlockSwitchCommands.Select(CollectionHelper.HashCode).GetHashCode());
        }

        // Context

        public class Context{
            public CompressedHeader Header { get; }
            public DataLength DataLength { get; }
            public BrotliGlobalState State { get; }

            public Context(CompressedHeader header, DataLength dataLength, BrotliGlobalState state){
                this.Header = header;
                this.DataLength = dataLength;
                this.State = state;
            }
        }

        internal abstract class DataContext : Context{
            public abstract CategoryMap<BlockSwitchTracker> BlockTrackers { get; }

            public bool NeedsMoreData => bytesWritten < DataLength.UncompressedBytes;
            private int bytesWritten;

            protected DataContext(Context wrapped) : base(wrapped.Header, wrapped.DataLength, wrapped.State){}

            public void WriteLiteral(in Literal literal){
                State.OutputLiteral(literal);
                ++bytesWritten;
            }

            public void WriteCopy(int length, DistanceInfo distance){
                bytesWritten += State.OutputCopy(length, distance).BytesWritten;
            }

            public void WriteCopyWithMarker(IMarkedBitReader reader, int length, DistanceInfo distance){
                if (reader is MarkedBitReaderDummy){
                    WriteCopy(length, distance);
                }
                else{
                    reader.MarkStart();

                    var written = new BrotliOutputStored();
                    written.Write((byte)'"');

                    State.AddOutputCallback(written);
                    CopyOutputInfo copy = WriteCopyTracked(length, distance);
                    State.RemoveOutputCallback(written);
                    
                    written.Write((byte)'"');
                    reader.MarkEnd(new ValueMarker(copy.IsBackReference ? "backreference" : "dictionary", Encoding.UTF8.GetString(written.AsBytes)));
                }
            }

            private CopyOutputInfo WriteCopyTracked(int length, DistanceInfo distance){
                var info = State.OutputCopy(length, distance);

                bytesWritten += info.BytesWritten;
                return info;
            }
        }

        private class ReaderDataContext : DataContext{
            public BlockSwitchCommandMutableMap BlockSwitchCommands => readingTrackers.Select(tracker => tracker.ReadCommands);

            public override CategoryMap<BlockSwitchTracker> BlockTrackers { get; }
            private readonly CategoryMap<BlockSwitchTracker.Reading> readingTrackers;

            public ReaderDataContext(Context wrapped, IMarkedBitReader reader) : base(wrapped){
                this.readingTrackers = Header.BlockTypes.Select(info => new BlockSwitchTracker.Reading(info, reader));
                this.BlockTrackers = readingTrackers.Select<BlockSwitchTracker>(tracker => tracker);
            }
        }

        private class WriterDataContext : DataContext{
            public override CategoryMap<BlockSwitchTracker> BlockTrackers { get; }

            public WriterDataContext(Context wrapped, BlockSwitchCommandMap blockSwitchCommands, IBitWriter writer) : base(wrapped){
                this.BlockTrackers = Header.BlockTypes.Select<BlockSwitchTracker>(info => new BlockSwitchTracker.Writing(info, writer, blockSwitchCommands[info.Category]));
            }
        }

        // Serialization

        public static readonly BitDeserializer<CompressedData, Context> Deserialize = MarkedBitDeserializer.Wrap<CompressedData, Context>(
            (reader, context) => {
                ReaderDataContext dataContext = new ReaderDataContext(context, reader);
                List<InsertCopyCommand> icCommands = new List<InsertCopyCommand>();
                
                reader.MarkStart();
                
                do{
                    icCommands.Add(InsertCopyCommand.Deserialize(reader, dataContext));
                }while(dataContext.NeedsMoreData);

                reader.MarkEndTitle("Command List");
                
                return new CompressedData(icCommands, dataContext.BlockSwitchCommands);
            }
        );

        public static readonly BitSerializer<CompressedData, Context> Serialize = (writer, obj, context) => {
            DataContext dataContext = new WriterDataContext(context, obj.BlockSwitchCommands, writer);

            foreach(InsertCopyCommand icCommand in obj.InsertCopyCommands){
                InsertCopyCommand.Serialize(writer, icCommand, dataContext);
            }
        };
    }
}
