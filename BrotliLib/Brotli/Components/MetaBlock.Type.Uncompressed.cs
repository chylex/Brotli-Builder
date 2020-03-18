using System;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Collections;
using BrotliLib.Markers;
using BrotliLib.Markers.Serialization;
using BrotliLib.Markers.Types;
using BrotliLib.Serialization;

namespace BrotliLib.Brotli.Components{
    partial class MetaBlock{
        /// <inheritdoc />
        /// <summary>
        /// <code>ISLAST = 0, MLEN > 0, ISUNCOMPRESSED = 1</code>
        /// </summary>
        public class Uncompressed : MetaBlock{
            public byte[] UncompressedData => CollectionHelper.Clone(uncompressedData);

            private readonly byte[] uncompressedData;

            public Uncompressed(byte[] uncompressedData) : base(new DataLength(uncompressedData.Length)){
                this.uncompressedData = CollectionHelper.Clone(uncompressedData);
            }

            public Uncompressed(byte[] uncompressedData, int start, int count) : base(new DataLength(count)){
                this.uncompressedData = CollectionHelper.Slice(uncompressedData, start, count);
            }

            public Uncompressed(ArraySegment<byte> uncompressedData) : base(new DataLength(uncompressedData.Count)){
                this.uncompressedData = uncompressedData.ToArray();
            }

            public override void Decompress(BrotliGlobalState state){
                state.OutputBytes(uncompressedData);
            }
            
            public override bool Equals(object obj){
                return obj is Uncompressed other &&
                       base.Equals(other) &&
                       CollectionHelper.Equal(uncompressedData, other.uncompressedData);
            }

            public override int GetHashCode(){
                return HashCode.Combine(ParentHashCode(), CollectionHelper.HashCode(uncompressedData));
            }

            // Serialization
            
            internal new static readonly BitDeserializer<Uncompressed, ReadContext> Deserialize = MarkedBitDeserializer.Title<Uncompressed, ReadContext>(
                "Contents",

                (reader, context) => {
                    byte[] bytes = new byte[context.DataLength.UncompressedBytes];
                    int length = bytes.Length;

                    if (length == 0){
                        throw new InvalidOperationException("Uncompressed meta-block must not be empty.");
                    }

                    reader.AlignToByteBoundary();
                    reader.MarkStart();

                    if (reader.MarkerLevel == MarkerLevel.Verbose){
                        for(int index = 0; index < length; index++){
                            bytes[index] = reader.NextAlignedByte("byte");
                        }

                        reader.MarkEndTitle("Uncompressed Bytes");
                    }
                    else{
                        for(int index = 0; index < length; index++){
                            bytes[index] = reader.NextAlignedByte();
                        }

                        reader.MarkEnd(new TextMarker("(" + length + " uncompressed byte" + (length == 1 ? ")" : "s)")));
                    }

                    context.State.OutputBytes(bytes);
                    return new Uncompressed(bytes);
                }
            );

            internal new static readonly BitSerializer<Uncompressed, BrotliGlobalState> Serialize = (writer, obj, context) => {
                byte[] bytes = obj.uncompressedData;

                if (bytes.Length == 0){
                    throw new InvalidOperationException("Uncompressed meta-block must not be empty.");
                }

                writer.AlignToByteBoundary();
                writer.WriteAlignedBytes(bytes);

                context.OutputBytes(bytes);
            };
        }
    }
}
