using System;

namespace BrotliLib.IO{
    /// <summary>
    /// Provides a way to append complex values into a <see cref="BitStream"/> for convenience.
    /// </summary>
    public class BitWriter{
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

        /// <summary>
        /// Writes a single bit into the stream.
        /// </summary>
        /// <param name="bit">Input bit.</param>
        public void WriteBit(bool bit){
            stream.Add(bit);
        }

        /// <summary>
        /// Writes a sequence of the specified amount of bits, using the least significant bits from the provided value.
        /// </summary>
        /// <param name="count">Amount of bits to write.</param>
        /// <param name="value">Integer source of the bits.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="count"/> parameter is larger than the amount of bits in an integer.</exception>
        public void WriteChunk(int count, int value){
            if (count > MaxChunkSize){
                throw new ArgumentOutOfRangeException(nameof(count), "Cannot write a chunk larger than " + MaxChunkSize + " bits in one call.");
            }

            for(int index = 0; index < count; index++){
                stream.Add((value & (1 << index)) != 0);
            }
        }

        /// <summary>
        /// If the current position is not already at a byte boundary (stream length divisible by 8), zeroes will be written to pad the rest of the byte.
        /// </summary>
        public void AlignToByteBoundary(){
            long relativeIndex = stream.Length % ByteSize;

            if (relativeIndex > 0){
                for(long bitsLeft = ByteSize - relativeIndex; bitsLeft > 0; bitsLeft--){
                    stream.Add(false);
                }
            }
        }
        
        /// <summary>
        /// First aligns the position to a byte boundary, then writes all bytes into the aligned stream.
        /// </summary>
        public void WriteAlignedBytes(byte[] bytes){
            AlignToByteBoundary();

            foreach(byte b in bytes){
                WriteChunk(ByteSize, b);
            }
        }
    }
}
