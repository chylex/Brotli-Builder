using System.Collections.Generic;

namespace BrotliLib.Brotli.Dictionary.Format{
    /// <summary>
    /// Defines information about what kinds of words are in the dictionary, how the words are arranged, and how to decode packed values with word and transform indices.
    /// </summary>
    public interface IDictionaryFormat{
        /// <summary>
        /// Enumerates all possible word lengths that have at least one word assigned to them.
        /// </summary>
        IEnumerable<int> WordLengths { get; }

        /// <summary>
        /// Returns the amount of words of the specified length.
        /// </summary>
        int WordCount(int length);

        /// <summary>
        /// Calculates the word index from a packed value. The indexing starts at 0 within each word length.
        /// </summary>
        int UnpackWordIndex(int length, int packed);

        /// <summary>
        /// Calculates the transformation index from a packed value.
        /// </summary>
        int UnpackTransformIndex(int length, int packed);

        /// <summary>
        /// Returns the starting byte position of a word of the specified length.
        /// </summary>
        int GetWordPosition(int length, int word);

        /// <summary>
        /// Returns the packed value, combining the word position and transform index into one value.
        /// </summary>
        int GetPackedValue(int length, int word, int transform);
    }
}
