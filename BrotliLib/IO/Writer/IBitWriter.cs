using System;

namespace BrotliLib.IO.Writer{
    /// <summary>
    /// Provides a way to append complex values into a <see cref="BitStream"/> for convenience.
    /// </summary>
    public interface IBitWriter{
        /// <summary>
        /// Writes a single bit into the stream.
        /// </summary>
        /// <param name="bit">Input bit.</param>
        void WriteBit(bool bit);
        
        /// <summary>
        /// Writes all bits from the provided bit stream.
        /// </summary>
        /// <param name="bits">Input bit stream.</param>
        void WriteBits(BitStream bits);

        /// <summary>
        /// Writes a sequence of the specified amount of bits, using the least significant bits from the provided value.
        /// </summary>
        /// <param name="count">Amount of bits to write.</param>
        /// <param name="value">Integer source of the bits.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="count"/> parameter is larger than the amount of bits in an integer.</exception>
        void WriteChunk(int count, int value);

        /// <summary>
        /// If the current position is not already at a byte boundary (stream length divisible by 8), zeroes will be written to pad the rest of the byte.
        /// </summary>
        void AlignToByteBoundary();
        
        /// <summary>
        /// First aligns the position to a byte boundary, then writes all bytes into the aligned stream.
        /// </summary>
        void WriteAlignedBytes(byte[] bytes);
    }
}
