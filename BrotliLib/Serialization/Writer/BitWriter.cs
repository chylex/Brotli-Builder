using System;
using BrotliLib.Collections.Huffman;

namespace BrotliLib.Serialization.Writer{
    public class BitWriter : IBitWriter{
        private const int ByteSize = 8;
        private const int MaxChunkSize = ByteSize * sizeof(int);

        private readonly BitStream stream;

        /// <summary>
        /// Initializes a new <see cref="BitWriter"/> at the end of the specified <see cref="BitStream"/>.
        /// </summary>
        /// <param name="stream">Input bit stream.</param>
        public BitWriter(BitStream stream){
            this.stream = stream;
        }

        public void WriteBit(bool bit){
            stream.Add(bit);
        }
        
        public void WriteBits(BitStream bits){
            stream.AddAll(bits);
        }

        public void WriteBits(in BitPath bits){
            for(int index = 0; index < bits.Length; index++){
                stream.Add(bits[index]);
            }
        }

        public void WriteChunk(int count, int value){
            if (count > MaxChunkSize){
                throw new ArgumentOutOfRangeException(nameof(count), "Cannot write a chunk larger than " + MaxChunkSize + " bits in one call.");
            }

            for(int index = 0; index < count; index++){
                stream.Add((value & (1 << index)) != 0);
            }
        }

        public void AlignToByteBoundary(){
            long relativeIndex = stream.Length % ByteSize;

            if (relativeIndex > 0){
                for(long bitsLeft = ByteSize - relativeIndex; bitsLeft > 0; bitsLeft--){
                    stream.Add(false);
                }
            }
        }

        public void WriteAlignedBytes(byte[] bytes){
            AlignToByteBoundary();

            int index = 0;

            while(index < bytes.Length && stream.Length % BitStream.BitEntrySize != 0){
                stream.AddByte(bytes[index]);
                ++index;
            }

            while(index < bytes.Length - BitStream.BytesPerEntry){
                ulong value = bytes[index];
                value |= (ulong)bytes[index + 1] << 8;
                value |= (ulong)bytes[index + 2] << 16;
                value |= (ulong)bytes[index + 3] << 24;
                value |= (ulong)bytes[index + 4] << 32;
                value |= (ulong)bytes[index + 5] << 40;
                value |= (ulong)bytes[index + 6] << 48;
                value |= (ulong)bytes[index + 7] << 56;

                stream.AddLong(value);
                index += BitStream.BytesPerEntry;
            }

            while(index < bytes.Length){
                stream.AddByte(bytes[index]);
                ++index;
            }
        }
    }
}
