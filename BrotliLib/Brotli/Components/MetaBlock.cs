using System;
using BrotliLib.Brotli.Components.Contents;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.IO;

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

        public bool IsLast { get; set; }
        public DataLength DataLength { get; set; }
        
        protected MetaBlock(bool isLast, DataLength dataLength){
            this.IsLast = isLast;
            this.DataLength = dataLength;
        }

        internal abstract void SerializeContents(BitWriter writer, BrotliGlobalState state);
        internal abstract void DeserializeContents(BitReader reader, BrotliGlobalState state);

        // Types

        /// <inheritdoc />
        /// <summary>
        /// <code>ISLAST = 1, ISLASTEMPTY = 1</code>
        /// </summary>
        public class LastEmpty : MetaBlock{
            public LastEmpty() : base(true, DataLength.Empty){}

            internal override void SerializeContents(BitWriter writer, BrotliGlobalState state){}
            internal override void DeserializeContents(BitReader reader, BrotliGlobalState state){}
        }
        
        /// <inheritdoc />
        /// <summary>
        /// <code>ISLAST = 0, MLEN = 0</code>
        /// </summary>
        public class PaddedEmpty : MetaBlock{
            public PaddedEmptyMetaBlockContents Contents { get; set; }

            internal PaddedEmpty() : base(false, DataLength.Empty){}

            public PaddedEmpty(byte[] data) : this(){
                this.Contents = new PaddedEmptyMetaBlockContents(data);
            }

            internal override void SerializeContents(BitWriter writer, BrotliGlobalState state) => PaddedEmptyMetaBlockContents.Serializer.ToBits(writer, Contents, new Context(this, state));
            internal override void DeserializeContents(BitReader reader, BrotliGlobalState state) => Contents = PaddedEmptyMetaBlockContents.Serializer.FromBits(reader, new Context(this, state));
        }
        
        /// <inheritdoc />
        /// <summary>
        /// <code>ISLAST = 0, MLEN > 0, ISUNCOMPRESSED = 1</code>
        /// </summary>
        public class Uncompressed : MetaBlock{
            public UncompressedMetaBlockContents Contents { get; set; }

            internal Uncompressed(DataLength dataLength) : base(false, dataLength){}

            public Uncompressed(byte[] data) : base(false, new DataLength(data.Length)){
                this.Contents = new UncompressedMetaBlockContents(data);
            }

            internal override void SerializeContents(BitWriter writer, BrotliGlobalState state) => UncompressedMetaBlockContents.Serializer.ToBits(writer, Contents, new Context(this, state));
            internal override void DeserializeContents(BitReader reader, BrotliGlobalState state) => Contents = UncompressedMetaBlockContents.Serializer.FromBits(reader, new Context(this, state));
        }
        
        /// <inheritdoc />
        /// <summary>
        /// <code>ISLAST = ?, MLEN > 0, ISUNCOMPRESSED = 0</code>
        /// </summary>
        public class Compressed : MetaBlock{
            public CompressedMetaBlockContents Contents { get; set; }

            public Compressed(bool isLast, DataLength dataLength) : base(isLast, dataLength){}

            internal override void SerializeContents(BitWriter writer, BrotliGlobalState state) => CompressedMetaBlockContents.Serializer.ToBits(writer, Contents, new Context(this, state));
            internal override void DeserializeContents(BitReader reader, BrotliGlobalState state) => Contents = CompressedMetaBlockContents.Serializer.FromBits(reader, new Context(this, state));
        }

        // Serialization
        
        private static readonly IBitSerializer<MetaBlock, BrotliGlobalState> MetaBlockBaseSerializer = new BitSerializer<MetaBlock, BrotliGlobalState>(
            fromBits: (reader, context) => {
                bool isLast = reader.NextBit();
                bool isLastEmpty = isLast && reader.NextBit();

                if (isLastEmpty){
                    return new LastEmpty();
                }

                DataLength dataLength = DataLength.Serializer.FromBits(reader, NoContext.Value);

                if (dataLength.UncompressedBytes == 0){
                    return new PaddedEmpty();
                }
                
                bool isUncompressed = !isLast && reader.NextBit();

                if (isUncompressed){
                    return new Uncompressed(dataLength);
                }
                else{
                    return new Compressed(isLast, dataLength);
                }
            },

            toBits: (writer, obj, context) => {
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

                DataLength.Serializer.ToBits(writer, obj.DataLength, NoContext.Value);

                if (obj is Uncompressed){
                    if (obj.IsLast){
                        throw new InvalidOperationException("An uncompressed meta-block cannot also be the last.");
                    }
                    else{
                        writer.WriteBit(true);
                    }
                }
            }
        );

        public static readonly IBitSerializer<MetaBlock, BrotliGlobalState> Serializer = new BitSerializer<MetaBlock, BrotliGlobalState>(
            fromBits: (reader, context) => {
                MetaBlock mb = MetaBlockBaseSerializer.FromBits(reader, context);
                mb.DeserializeContents(reader, context);
                return mb;
            },

            toBits: (writer, obj, context) => {
                MetaBlockBaseSerializer.ToBits(writer, obj, context);
                obj.SerializeContents(writer, context);
            }
        );
    }
}
