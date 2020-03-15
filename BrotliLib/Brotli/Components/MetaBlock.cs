using System;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Markers.Serialization;
using BrotliLib.Serialization;

namespace BrotliLib.Brotli.Components{
    /// <summary>
    /// Describes both the header and data of a Brotli meta-block.
    /// https://tools.ietf.org/html/rfc7932#section-9.2
    /// https://tools.ietf.org/html/rfc7932#section-9.3
    /// </summary>
    public abstract partial class MetaBlock{
        internal sealed class ReadContext{
            public BrotliGlobalState State { get; }
            public DataLength DataLength { get; }

            public ReadContext(BrotliGlobalState state, DataLength dataLength){
                this.State = state;
                this.DataLength = dataLength;
            }
        }

        // Data

        public DataLength DataLength { get; }
        
        protected MetaBlock(DataLength dataLength){
            this.DataLength = dataLength;
        }

        public abstract void Decompress(BrotliGlobalState state);

        protected bool Equals(MetaBlock other){
            return DataLength.Equals(other.DataLength);
        }

        protected int ParentHashCode(){
            return HashCode.Combine(DataLength);
        }

        // Serialization

        public Marked Mark(bool isLast){
            return new Marked(this, isLast);
        }

        public readonly struct Marked{
            public MetaBlock MetaBlock { get; }
            public bool IsLast { get; }

            public Marked(MetaBlock metaBlock, bool isLast){
                this.MetaBlock = metaBlock;
                this.IsLast = isLast;
            }

            public void Deconstruct(out MetaBlock metaBlock, out bool isLast){
                metaBlock = MetaBlock;
                isLast = IsLast;
            }
        }

        public static readonly BitDeserializer<Marked, BrotliGlobalState> Deserialize = MarkedBitDeserializer.Title<Marked, BrotliGlobalState>(
            "Meta-Block",

            (reader, context) => {
                bool isLast = reader.NextBit("ISLAST");
                bool isLastEmpty = isLast && reader.NextBit("ISLASTEMPTY");

                if (isLastEmpty){
                    return LastEmpty.Marked;
                }

                DataLength dataLength = DataLength.Deserialize(reader, NoContext.Value);

                if (dataLength.UncompressedBytes == 0){
                    return new Marked(PaddedEmpty.Deserialize(reader, NoContext.Value), isLast);
                }
                
                bool isUncompressed = !isLast && reader.NextBit("ISUNCOMPRESSED");

                if (isUncompressed){
                    return new Marked(Uncompressed.Deserialize(reader, new ReadContext(context, dataLength)), false);
                }
                else{
                    return new Marked(Compressed.Deserialize(reader, new ReadContext(context, dataLength)), isLast);
                }
            }
        );

        public static readonly BitSerializer<Marked, BrotliGlobalState, BrotliSerializationParameters> Serialize = (writer, obj, context, parameters) => {
            var (metaBlock, isLast) = obj;

            if (metaBlock is LastEmpty){
                writer.WriteBit(true); // ISLAST
                writer.WriteBit(true); // ISLASTEMPTY
                return;
            }

            if (isLast){
                writer.WriteBit(true); // ISLAST
                writer.WriteBit(false); // ISLASTEMPTY
            }
            else{
                writer.WriteBit(false); // ISLAST
            }

            DataLength.Serialize(writer, metaBlock.DataLength, NoContext.Value);

            switch(metaBlock){
                case PaddedEmpty pe:
                    PaddedEmpty.Serialize(writer, pe, NoContext.Value);
                    break;

                case Uncompressed u:
                    if (isLast){
                        throw new InvalidOperationException("An uncompressed meta-block cannot also be the last.");
                    }
                    else{
                        writer.WriteBit(true); // ISUNCOMPRESSED
                    }

                    Uncompressed.Serialize(writer, u, context);
                    break;

                case Compressed c:
                    if (!isLast){
                        writer.WriteBit(false); // ISUNCOMPRESSED
                    }

                    Compressed.Serialize(writer, c, context, parameters);
                    break;
            }
        };
    }
}
