using System;
using BrotliLib.Brotli.Components.Compressed;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Markers.Serialization;
using BrotliLib.Serialization;

namespace BrotliLib.Brotli.Components{
    partial class MetaBlock{
        /// <inheritdoc />
        /// <summary>
        /// <code>ISLAST = ?, MLEN > 0, ISUNCOMPRESSED = 0</code>
        /// </summary>
        public class Compressed : MetaBlock{
            public CompressedHeader Header { get; }
            public CompressedData Data { get; }

            public Compressed(DataLength dataLength, CompressedHeader header, CompressedData data) : base(dataLength){
                this.Header = header;
                this.Data = data;
            }

            public override void Decompress(BrotliGlobalState state){
                foreach(InsertCopyCommand icCommand in Data.InsertCopyCommands){
                    state.OutputLiterals(icCommand.Literals);

                    if (icCommand.CopyDistance != DistanceInfo.EndsAfterLiterals){
                        state.OutputCopy(icCommand.CopyLength, icCommand.CopyDistance);
                    }
                }
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
            
            internal new static readonly BitDeserializer<Compressed, ReadContext> Deserialize = MarkedBitDeserializer.Wrap<Compressed, ReadContext>(
                (reader, context) => {
                    var header = reader.ReadStructure(CompressedHeader.Deserialize, NoContext.Value, "Header");
                    var data = reader.ReadStructure(CompressedData.Deserialize, new CompressedData.Context(header, context.DataLength, context.State), "Data");

                    return new Compressed(context.DataLength, header, data);
                }
            );

            internal new static readonly BitSerializer<Compressed, BrotliGlobalState, BrotliSerializationParameters> Serialize = (writer, obj, context, parameters) => {
                CompressedHeader.Serialize(writer, obj.Header, NoContext.Value, parameters);
                CompressedData.Serialize(writer, obj.Data, new CompressedData.Context(obj.Header, obj.DataLength, context));
            };
        }
    }
}
