using System;

namespace BrotliLib.Serialization.Reader{
    /// <summary>
    /// Provides a way to traverse through a <see cref="BitStream"/> with additional functionality for convenience.
    /// </summary>
    public interface IBitReader{
        /// <summary>
        /// The current position in the <see cref="BitStream"/>.
        /// </summary>
        int Index { get; }

        /// <summary>
        /// Returns the next bit as a boolean.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">Thrown when there are no more bits left in the stream.</exception>
        bool NextBit();

        /// <summary>
        /// Returns an integer constructed from a sequence of bits of the specified size.
        /// For example, when reading 3 bits from a sequence 110001, the method reads [110] and effectively reverses the bits into 011.
        /// </summary>
        /// <param name="bits">Amount of bits to read.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown when the <paramref name="bits"/> parameter is larger than the amount of bits left in the stream.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="bits"/> parameter is larger than the amount of bits in an integer.</exception>
        int NextChunk(int bits);

        /// <summary>
        /// If the current position is not already at a byte boundary (position divisible by 8), enough bits will be skipped to align it.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">Thrown when there are not enough bits left in the stream for the alignment.</exception>
        void AlignToByteBoundary();

        /// <summary>
        /// First aligns the position to a byte boundary, then reads the following byte.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">Thrown when there are not enough bits in the stream to align the position and read a byte.</exception>
        byte NextAlignedByte();
    }
}
