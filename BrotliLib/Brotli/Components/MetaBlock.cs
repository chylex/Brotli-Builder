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
        internal class Context{
            public bool IsLast { get; }
            public DataLength DataLength { get; }
            public BrotliGlobalState State { get; }

            public Context(bool isLast, DataLength dataLength, BrotliGlobalState state){
                this.IsLast = isLast;
                this.DataLength = dataLength;
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

        protected bool Equals(MetaBlock other){
            return IsLast == other.IsLast &&
                   DataLength.Equals(other.DataLength);
        }

        protected int ParentHashCode(){
            return HashCode.Combine(IsLast, DataLength);
        }

        // Serialization

        public static readonly BitDeserializer<MetaBlock, BrotliGlobalState> Deserialize = MarkedBitDeserializer.Title<MetaBlock, BrotliGlobalState>(
            "Meta-Block",

            (reader, context) => {
                bool isLast = reader.NextBit("ISLAST");
                bool isLastEmpty = isLast && reader.NextBit("ISLASTEMPTY");

                if (isLastEmpty){
                    return new LastEmpty();
                }

                DataLength dataLength = DataLength.Deserialize(reader, NoContext.Value);

                if (dataLength.UncompressedBytes == 0){
                    return reader.ReadStructure(PaddedEmpty.Deserialize, NoContext.Value, "Contents");
                }
                
                bool isUncompressed = !isLast && reader.NextBit("ISUNCOMPRESSED");

                if (isUncompressed){
                    return reader.ReadStructure(Uncompressed.Deserialize, new Context(isLast: false, dataLength, context), "Contents");
                }
                else{
                    return reader.ReadStructure(Compressed.Deserialize, new Context(isLast, dataLength, context), "Contents");
                }
            }
        );

        public static readonly BitSerializer<MetaBlock, BrotliGlobalState, BrotliSerializationParameters> Serialize = (writer, obj, context, parameters) => {
            if (obj is LastEmpty){
                writer.WriteBit(true); // ISLAST
                writer.WriteBit(true); // ISLASTEMPTY
                return;
            }

            if (obj.IsLast){
                writer.WriteBit(true); // ISLAST
                writer.WriteBit(false); // ISLASTEMPTY
            }
            else{
                writer.WriteBit(false); // ISLAST
            }

            DataLength.Serialize(writer, obj.DataLength, NoContext.Value);

            switch(obj){
                case PaddedEmpty pe:
                    PaddedEmpty.Serialize(writer, pe, NoContext.Value);
                    break;

                case Uncompressed u:
                    if (u.IsLast){
                        throw new InvalidOperationException("An uncompressed meta-block cannot also be the last.");
                    }
                    else{
                        writer.WriteBit(true); // ISUNCOMPRESSED
                    }

                    Uncompressed.Serialize(writer, u, new Context(u.IsLast, u.DataLength, context));
                    break;

                case Compressed c:
                    if (!c.IsLast){
                        writer.WriteBit(false); // ISUNCOMPRESSED
                    }

                    Compressed.Serialize(writer, c, new Context(c.IsLast, c.DataLength, context), parameters);
                    break;
            }
        };
    }
}
