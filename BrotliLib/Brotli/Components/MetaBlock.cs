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

        // Data

        public bool IsLast { get; set; }
        public DataLength DataLength { get; set; }
        
        protected MetaBlock(bool isLast, DataLength dataLength){
            this.IsLast = isLast;
            this.DataLength = dataLength;
        }

        internal abstract void SerializeContents(BitWriter writer);
        internal abstract void DeserializeContents(BitReader reader);

        // Types

        /// <inheritdoc />
        /// <summary>
        /// <code>ISLAST = 1, ISLASTEMPTY = 1</code>
        /// </summary>
        public class LastEmpty : MetaBlock{
            public LastEmpty() : base(true, DataLength.Empty){}

            internal override void SerializeContents(BitWriter writer){}
            internal override void DeserializeContents(BitReader reader){}
        }
        
        /// <inheritdoc />
        /// <summary>
        /// <code>ISLAST = 0, MLEN = 0</code>
        /// </summary>
        public class PaddedEmpty : MetaBlock{
            public PaddedEmptyMetaBlockContents Contents { get; set; }

            public PaddedEmpty() : base(false, DataLength.Empty){}

            internal override void SerializeContents(BitWriter writer) => PaddedEmptyMetaBlockContents.Serializer.ToBits(writer, Contents, this);
            internal override void DeserializeContents(BitReader reader) => Contents = PaddedEmptyMetaBlockContents.Serializer.FromBits(reader, this);
        }
        
        /// <inheritdoc />
        /// <summary>
        /// <code>ISLAST = 0, MLEN > 0, ISUNCOMPRESSED = 1</code>
        /// </summary>
        public class Uncompressed : MetaBlock{
            public UncompressedMetaBlockContents Contents { get; set; }

            public Uncompressed(DataLength dataLength) : base(false, dataLength){}

            internal override void SerializeContents(BitWriter writer) => UncompressedMetaBlockContents.Serializer.ToBits(writer, Contents, this);
            internal override void DeserializeContents(BitReader reader) => Contents = UncompressedMetaBlockContents.Serializer.FromBits(reader, this);
        }
        
        /// <inheritdoc />
        /// <summary>
        /// <code>ISLAST = ?, MLEN > 0, ISUNCOMPRESSED = 0</code>
        /// </summary>
        public class Compressed : MetaBlock{
            public CompressedMetaBlockContents Contents { get; set; }

            public Compressed(bool isLast, DataLength dataLength) : base(isLast, dataLength){}

            internal override void SerializeContents(BitWriter writer) => CompressedMetaBlockContents.Serializer.ToBits(writer, Contents, this);
            internal override void DeserializeContents(BitReader reader) => Contents = CompressedMetaBlockContents.Serializer.FromBits(reader, this);
        }

        // Serialization
        
        private static readonly IBitSerializer<MetaBlock, BrotliGlobalState> MetaBlockBaseSerializer = new BitSerializer<MetaBlock, BrotliGlobalState>(
            fromBits: (reader, context) => {
                bool isLast = reader.NextBit();
                bool isLastEmpty = isLast && reader.NextBit();

                if (isLastEmpty){
                    return new LastEmpty();
                }

                DataLength dataLength = DataLength.Serializer.FromBits(reader, null);

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

                DataLength.Serializer.ToBits(writer, obj.DataLength, null);

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
                mb.DeserializeContents(reader);
                return mb;
            },

            toBits: (writer, obj, context) => {
                MetaBlockBaseSerializer.ToBits(writer, obj, context);
                obj.SerializeContents(writer);
            }
        );
    }
}
