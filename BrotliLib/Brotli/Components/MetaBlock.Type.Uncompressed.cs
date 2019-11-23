using System;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Collections;
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

            public Uncompressed(byte[] uncompressedData) : base(false, new DataLength(uncompressedData.Length)){
                this.uncompressedData = CollectionHelper.Clone(uncompressedData);
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
        
            internal new static readonly BitDeserializer<Uncompressed, Context> Deserialize = MarkedBitDeserializer.Wrap<Uncompressed, Context>(
                (reader, context) => {
                    byte[] bytes = new byte[context.DataLength.UncompressedBytes];

                    if (bytes.Length == 0){
                        throw new InvalidOperationException("Uncompressed meta-block must not be empty.");
                    }

                    reader.AlignToByteBoundary();
                    reader.MarkStart();

                    for(int index = 0; index < bytes.Length; index++){
                        bytes[index] = reader.NextAlignedByte("byte");
                    }

                    reader.MarkEnd(() => new TitleMarker("Uncompressed Bytes"));

                    context.State.OutputBytes(bytes);
                    return new Uncompressed(bytes);
                }
            );

            internal new static readonly BitSerializer<Uncompressed, Context> Serialize = (writer, obj, context) => {
                byte[] bytes = obj.uncompressedData;

                if (bytes.Length == 0){
                    throw new InvalidOperationException("Uncompressed meta-block must not be empty.");
                }
                else if (bytes.Length != context.DataLength.UncompressedBytes){
                    throw new InvalidOperationException("Uncompressed meta-block has invalid data length (" + bytes.Length + " != " + context.DataLength.UncompressedBytes + ")");
                }

                writer.AlignToByteBoundary();
                writer.WriteAlignedBytes(bytes);

                context.State.OutputBytes(bytes);
            };
        }
    }
}
