using System;
using BrotliLib.Brotli.Components.Contents;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Markers;
using BrotliLib.Brotli.Markers.Data;
using BrotliLib.Brotli.State;
using BrotliLib.IO;
using BrotliLib.IO.Reader;
using BrotliLib.IO.Writer;

namespace BrotliLib.Brotli.Components{
    /// <summary>
    /// Describes both the header and data of a Brotli meta-block.
    /// https://tools.ietf.org/html/rfc7932#section-9.2
    /// https://tools.ietf.org/html/rfc7932#section-9.3
    /// </summary>
    public abstract class MetaBlock{
        internal class Context{
            public MetaBlock MetaBlock { get; }
            public BrotliGlobalState State { get; }

            public Context(MetaBlock metaBlock, BrotliGlobalState state){
                this.MetaBlock = metaBlock;
                this.State = state;
            }
        }

        // Data

        public bool IsLast { get; internal set; }
        public DataLength DataLength { get; }
        
        protected MetaBlock(bool isLast, DataLength dataLength){
            this.IsLast = isLast;
            this.DataLength = dataLength;
        }

        internal abstract void SerializeContents(IBitWriter writer, BrotliGlobalState state);
        internal abstract void DeserializeContents(IBitReader reader, BrotliGlobalState state);
        
        protected bool Equals(MetaBlock other){
            return IsLast == other.IsLast &&
                   DataLength.Equals(other.DataLength);
        }

        protected int ParentHashCode(){
            return HashCode.Combine(IsLast, DataLength);
        }

        // Types

        /// <inheritdoc />
        /// <summary>
        /// <code>ISLAST = 1, ISLASTEMPTY = 1</code>
        /// </summary>
        public class LastEmpty : MetaBlock{
            public LastEmpty() : base(true, DataLength.Empty){}

            public override bool Equals(object obj){
                return obj is LastEmpty;
            }

            public override int GetHashCode(){
                return ParentHashCode();
            }

            internal override void SerializeContents(IBitWriter writer, BrotliGlobalState state){}
            internal override void DeserializeContents(IBitReader reader, BrotliGlobalState state){}
        }
        
        /// <inheritdoc />
        /// <summary>
        /// <code>ISLAST = 0, MLEN = 0</code>
        /// </summary>
        public class PaddedEmpty : MetaBlock{
            public PaddedEmptyMetaBlockContents Contents { get; private set; }

            internal PaddedEmpty() : base(false, DataLength.Empty){}

            public PaddedEmpty(byte[] data) : this(){
                this.Contents = new PaddedEmptyMetaBlockContents(data);
            }

            public override bool Equals(object obj){
                return obj is PaddedEmpty other &&
                       Contents.Equals(other.Contents);
            }

            public override int GetHashCode(){
                return HashCode.Combine(ParentHashCode(), Contents);
            }

            internal override void SerializeContents(IBitWriter writer, BrotliGlobalState state) => PaddedEmptyMetaBlockContents.Serialize(writer, Contents, NoContext.Value);
            internal override void DeserializeContents(IBitReader reader, BrotliGlobalState state) => Contents = PaddedEmptyMetaBlockContents.Deserialize(reader, NoContext.Value);
        }
        
        /// <inheritdoc />
        /// <summary>
        /// <code>ISLAST = 0, MLEN > 0, ISUNCOMPRESSED = 1</code>
        /// </summary>
        public class Uncompressed : MetaBlock{
            public UncompressedMetaBlockContents Contents { get; private set; }

            internal Uncompressed(DataLength dataLength) : base(false, dataLength){}

            public Uncompressed(byte[] data) : base(false, new DataLength(data.Length)){
                this.Contents = new UncompressedMetaBlockContents(data);
            }
            
            public override bool Equals(object obj){
                return obj is Uncompressed other &&
                       base.Equals(other) &&
                       Contents.Equals(other.Contents);
            }

            public override int GetHashCode(){
                return HashCode.Combine(ParentHashCode(), Contents);
            }

            internal override void SerializeContents(IBitWriter writer, BrotliGlobalState state) => UncompressedMetaBlockContents.Serialize(writer, Contents, new Context(this, state));
            internal override void DeserializeContents(IBitReader reader, BrotliGlobalState state) => Contents = UncompressedMetaBlockContents.Deserialize(reader, new Context(this, state));
        }
        
        /// <inheritdoc />
        /// <summary>
        /// <code>ISLAST = ?, MLEN > 0, ISUNCOMPRESSED = 0</code>
        /// </summary>
        public class Compressed : MetaBlock{
            public CompressedMetaBlockContents Contents { get; private set; }

            public Compressed(bool isLast, DataLength dataLength) : base(isLast, dataLength){}
            public Compressed(bool isLast, DataLength dataLength, CompressedMetaBlockContents contents) : this(isLast, dataLength){
                this.Contents = contents;
            }
            
            public override bool Equals(object obj){
                return obj is Compressed other &&
                       base.Equals(other) &&
                       Contents.Equals(other.Contents);
            }

            public override int GetHashCode(){
                return HashCode.Combine(ParentHashCode(), Contents);
            }

            internal override void SerializeContents(IBitWriter writer, BrotliGlobalState state) => CompressedMetaBlockContents.Serialize(writer, Contents, new Context(this, state));
            internal override void DeserializeContents(IBitReader reader, BrotliGlobalState state) => Contents = CompressedMetaBlockContents.Deserialize(reader, new Context(this, state));
        }

        // Serialization
        
        private static readonly BitDeserializer<MetaBlock, BrotliGlobalState> MetaBlockBaseDeserialize = MarkedBitDeserializer.Wrap<MetaBlock, BrotliGlobalState>(
            (reader, context) => {
                bool isLast = reader.NextBit("ISLAST");
                bool isLastEmpty = isLast && reader.NextBit("ISLASTEMPTY");

                if (isLastEmpty){
                    return new LastEmpty();
                }

                DataLength dataLength = DataLength.Deserialize(reader, NoContext.Value);

                if (dataLength.UncompressedBytes == 0){
                    return new PaddedEmpty();
                }
                
                bool isUncompressed = !isLast && reader.NextBit("ISUNCOMPRESSED");

                if (isUncompressed){
                    return new Uncompressed(dataLength);
                }
                else{
                    return new Compressed(isLast, dataLength);
                }
            }
        );

        private static readonly BitSerializer<MetaBlock, BrotliGlobalState> MetaBlockBaseSerialize = (writer, obj, context) => {
            if (obj is LastEmpty){
                writer.WriteBit(true);
                writer.WriteBit(true);
                return;
            }

            if (obj.IsLast){
                writer.WriteBit(true);
                writer.WriteBit(false);
            }
            else{
                writer.WriteBit(false);
            }

            DataLength.Serialize(writer, obj.DataLength, NoContext.Value);

            if (obj.DataLength.ChunkNibbles == 0){
                return;
            }

            if (!obj.IsLast){
                writer.WriteBit(obj is Uncompressed);
            }
            else if (obj is Uncompressed){
                throw new InvalidOperationException("An uncompressed meta-block cannot also be the last.");
            }
        };

        public static readonly BitDeserializer<MetaBlock, BrotliGlobalState> Deserialize = MarkedBitDeserializer.Wrap<MetaBlock, BrotliGlobalState>(
            (reader, context) => {
                reader.MarkStart();

                MetaBlock mb = reader.ReadStructure(MetaBlockBaseDeserialize, context, "Header");
                
                reader.MarkStart();
                mb.DeserializeContents(reader, context);
                reader.MarkEnd(() => new TitleMarker("Contents"));

                reader.MarkEnd(() => new TitleMarker("Meta-Block (" + mb.GetType().Name + ")"));
                return mb;
            }
        );

        public static readonly BitSerializer<MetaBlock, BrotliGlobalState> Serialize = (writer, obj, context) => {
            MetaBlockBaseSerialize(writer, obj, context);
            obj.SerializeContents(writer, context);
        };
    }
}
