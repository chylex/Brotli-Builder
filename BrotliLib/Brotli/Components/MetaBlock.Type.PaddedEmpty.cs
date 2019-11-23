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
        /// <code>ISLAST = 0, MLEN = 0</code>
        /// </summary>
        public class PaddedEmpty : MetaBlock{
            private const int MaxLengthDescriptionBytes = 3;
            private const int MaxSkippableBytes = 1 << (8 * MaxLengthDescriptionBytes);

            private static int CalculateBytesRequired(int hiddenBytes){
                if (hiddenBytes < 0){
                    throw new ArgumentOutOfRangeException(nameof(hiddenBytes), "The amount of bytes must be at least 0.");
                }
                else if (hiddenBytes == 0){
                    return 0;
                }

                for(int bytes = 1; bytes <= MaxLengthDescriptionBytes; bytes++){
                    int maxValue = 1 << (8 * bytes);

                    if (hiddenBytes <= maxValue){
                        return bytes;
                    }
                }

                throw new ArgumentOutOfRangeException(nameof(hiddenBytes), "The amount of bytes (" + hiddenBytes + ") cannot be expressed with at most " + MaxLengthDescriptionBytes + " bytes.");
            }

            // Instance

            public byte[] HiddenData => CollectionHelper.Clone(hiddenData);

            private readonly byte[] hiddenData;

            public PaddedEmpty(byte[] hiddenData) : base(false, DataLength.Empty){
                if (hiddenData.Length > MaxSkippableBytes){
                    throw new ArgumentOutOfRangeException(nameof(hiddenData), "The hidden data length must be at most " + MaxSkippableBytes + " bytes.");
                }

                this.hiddenData = CollectionHelper.Clone(hiddenData);
            }
            
            public override bool Equals(object obj){
                return obj is PaddedEmpty other &&
                       base.Equals(other) &&
                       CollectionHelper.Equal(hiddenData, other.hiddenData);
            }

            public override int GetHashCode(){
                return HashCode.Combine(ParentHashCode(), CollectionHelper.HashCode(hiddenData));
            }

            // Serialization

            internal new static readonly BitDeserializer<PaddedEmpty, NoContext> Deserialize = MarkedBitDeserializer.Wrap<PaddedEmpty, NoContext>(
                (reader, context) => {
                    if (reader.NextBit("reserved")){
                        throw new InvalidOperationException("Reserved bit in empty meta-block must be 0.");
                    }
                
                    int skipDescriptionBytes = reader.NextChunk(2, "MSKIPBYTES");
                    int skipLength = (skipDescriptionBytes == 0) ? 0 : reader.NextChunk(8 * skipDescriptionBytes, "MSKIPLEN", value => 1 + value);

                    byte[] bytes = new byte[skipLength];
                
                    reader.AlignToByteBoundary();
                    reader.MarkStart();
                
                    for(int index = 0; index < bytes.Length; index++){
                        bytes[index] = reader.NextAlignedByte("byte");
                    }

                    reader.MarkEnd(() => new TitleMarker("Skipped Bytes"));

                    return new PaddedEmpty(bytes);
                }
            );

            internal new static readonly BitSerializer<PaddedEmpty, NoContext> Serialize = (writer, obj, context) => {
                writer.WriteBit(false);

                byte[] bytes = obj.hiddenData;
                int lengthDescriptionBytes = CalculateBytesRequired(bytes.Length);

                writer.WriteChunk(2, lengthDescriptionBytes);
                writer.WriteChunk(8 * lengthDescriptionBytes, bytes.Length - 1);
                writer.WriteAlignedBytes(bytes);
            };
        }
    }
}
