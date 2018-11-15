using System;
using System.Collections.Generic;

namespace BrotliLib.IO{
    /// <summary>
    /// Provides a way to traverse through a <see cref="BitStream"/> with additional functionality for convenience.
    /// </summary>
    public class BitReader{
        private const int ByteSize = 8;
        private const int MaxChunkSize = ByteSize * sizeof(int);

        private readonly IEnumerator<bool> enumerator;
        private long index = 0;

        /// <summary>
        /// Initializes a new <see cref="BitReader"/> at the beginning of the specified <see cref="BitStream"/>.
        /// </summary>
        /// <param name="stream">Input bit stream.</param>
        public BitReader(BitStream stream){
            this.enumerator = stream.GetEnumerator();
            this.index = 0;
        }

        /// <summary>
        /// Returns the next bit as a boolean.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">Thrown when there are no more bits left in the stream.</exception>
        public bool NextBit(){
            if (!enumerator.MoveNext()){
                throw new IndexOutOfRangeException("No more bits left in the stream.");
            }
            
            ++index;
            return enumerator.Current;
        }

        /// <summary>
        /// Returns an integer constructed from a sequence of bits of the specified size.
        /// For example, when reading 3 bits from a sequence 110001, the method reads [110] and effectively reverses the bits into 011.
        /// </summary>
        /// <param name="bits">Amount of bits to read.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown when the <paramref name="bits"/> parameter is larger than the amount of bits left in the stream.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="bits"/> parameter is larger than the amount of bits in an integer.</exception>
        public int NextChunk(int bits){
            if (bits > MaxChunkSize){
                throw new ArgumentOutOfRangeException(nameof(bits), "Cannot retrieve a chunk larger than " + MaxChunkSize + " bits in one call.");
            }
            
            int value = 0;

            for(int bit = 0; bit < bits; bit++){
                if (NextBit()){
                    value |= 1 << bit;
                }
            }

            return value;
        }

        /// <summary>
        /// If the current position is not already at a byte boundary (position divisible by 8), enough bits will be skipped to align it.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">Thrown when there are not enough bits left in the stream for the alignment.</exception>
        public void AlignToByteBoundary(){
            long relativeIndex = index % ByteSize;

            if (relativeIndex > 0){
                for(long bitsLeft = ByteSize - relativeIndex; bitsLeft > 0; bitsLeft--){
                    NextBit();
                }
            }
        }

        /// <summary>
        /// First aligns the position to a byte boundary, then reads the following byte.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">Thrown when there are not enough bits in the stream to align the position and read a byte.</exception>
        public byte NextAlignedByte(){
            AlignToByteBoundary();
            return (byte)NextChunk(ByteSize);
        }
    }
}
