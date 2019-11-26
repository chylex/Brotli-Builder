using System;
using BrotliLib.Brotli.Components.Compressed;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Serialization;
using BrotliLib.Markers.Serialization;
using BrotliLib.Serialization;

namespace BrotliLib.Brotli.Components{
    partial class MetaBlock{
        /// <inheritdoc />
        /// <summary>
        /// <code>ISLAST = ?, MLEN > 0, ISUNCOMPRESSED = 0</code>
        /// </summary>
        public class Compressed : MetaBlock{
            public MetaBlockCompressionHeader Header { get; }
            public MetaBlockCompressionData Data { get; }

            public Compressed(bool isLast, DataLength dataLength, MetaBlockCompressionHeader header, MetaBlockCompressionData data) : base(isLast, dataLength){
                this.Header = header;
                this.Data = data;
            }

            public override bool Equals(object obj){
                return obj is Compressed other &&
                       base.Equals(other) &&
                       Header.Equals(other.Header) &&
                       Data.Equals(other.Data);
            }

            public override int GetHashCode(){
                return HashCode.Combine(ParentHashCode(), Header, Data);
            }

            // Serialization
        
            internal new static readonly BitDeserializer<Compressed, Context> Deserialize = MarkedBitDeserializer.Wrap<Compressed, Context>(
                (reader, context) => {
                    var header = reader.ReadStructure(MetaBlockCompressionHeader.Deserialize, NoContext.Value, "Compression Header");
                    var data = reader.ReadStructure(MetaBlockCompressionData.Deserialize, new MetaBlockCompressionData.Context(header, context.DataLength, context.State), "Compression Data");

                    return new Compressed(context.IsLast, context.DataLength, header, data);
                }
            );

            internal new static readonly BitSerializer<Compressed, Context, BrotliSerializationParameters> Serialize = (writer, obj, context, parameters) => {
                MetaBlockCompressionHeader.Serialize(writer, obj.Header, NoContext.Value, parameters);
                MetaBlockCompressionData.Serialize(writer, obj.Data, new MetaBlockCompressionData.Context(obj.Header, context.DataLength, context.State));
            };
        }
    }
}
