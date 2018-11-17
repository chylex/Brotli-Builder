using System;
using BrotliLib.IO;

namespace BrotliLib.Brotli.Components.Contents{
    public sealed class PaddedEmptyMetaBlockContents{
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

        public byte[] HiddenData{
            get{
                byte[] copy = new byte[hiddenData.Length];
                Buffer.BlockCopy(hiddenData, 0, copy, 0, hiddenData.Length);
                return copy;
            }
        }

        private readonly byte[] hiddenData;

        public PaddedEmptyMetaBlockContents(byte[] hiddenData){
            if (hiddenData.Length > MaxSkippableBytes){
                throw new ArgumentOutOfRangeException(nameof(hiddenData), "The hidden data length must be at most " + MaxSkippableBytes + " bytes.");
            }

            this.hiddenData = hiddenData;
        }

        // Serialization

        internal static readonly IBitSerializer<PaddedEmptyMetaBlockContents, MetaBlock.Context> Serializer = new BitSerializer<PaddedEmptyMetaBlockContents, MetaBlock.Context>(
            fromBits: (reader, context) => {
                if (reader.NextBit()){
                    throw new InvalidOperationException("Reserved bit in empty meta-block must be 0.");
                }

                int lengthDescriptionBytes = reader.NextChunk(2);
                byte[] bytes = new byte[(lengthDescriptionBytes == 0) ? 0 : 1 + reader.NextChunk(8 * lengthDescriptionBytes)];
                
                reader.AlignToByteBoundary();
                
                for(int index = 0; index < bytes.Length; index++){
                    bytes[index] = reader.NextAlignedByte();
                }

                return new PaddedEmptyMetaBlockContents(bytes);
            },

            toBits: (writer, obj, context) => {
                writer.WriteBit(false);

                byte[] bytes = obj.hiddenData;
                int lengthDescriptionBytes = CalculateBytesRequired(bytes.Length);

                writer.WriteChunk(2, lengthDescriptionBytes);
                writer.WriteChunk(8 * lengthDescriptionBytes, bytes.Length - 1);
                writer.WriteAlignedBytes(bytes);
            }
        );
    }
}
