using System;
using System.Collections.Generic;
using System.Linq;

namespace BrotliLib.Brotli.Dictionary.Format{
    /// <summary>
    /// Specifies a dictionary format defined by an array of values, where each index represents word length, and each value represents the amount of bits needed to count words of that length.
    /// Given a bit depth of N, the local position of a word is encoded in the lower N bits, and any bits above (up to the integer boundary) represent the transformation index.
    /// </summary>
    public sealed class BitDepthFormat : IDictionaryFormat{
        public IEnumerable<int> WordLengths => Enumerable.Range(minLength, maxLength - minLength + 1);
        public int WordCount(int length) => length <= maxLength ? wordCounts[length] : 0;
        
        private readonly int minLength, maxLength;
        private readonly int[] wordLengthBits;
        private readonly int[] wordCounts;
        private readonly int[] wordOffsets;
        
        public BitDepthFormat(int[] wordLengthBits){
            this.minLength = Array.FindIndex(wordLengthBits, bits => bits > 0);
            this.maxLength = Array.FindLastIndex(wordLengthBits, bits => bits > 0);
            
            this.wordLengthBits = wordLengthBits;
            this.wordCounts = wordLengthBits.Select(bits => bits == 0 ? 0 : 1 << bits).ToArray();
            this.wordOffsets = new int[wordLengthBits.Length];

            for(int length = 0; length < wordOffsets.Length - 1; length++){
                wordOffsets[length + 1] = wordOffsets[length] + length * wordCounts[length];
            }
        }
        
        public int UnpackWordIndex(int length, int packed){
            CheckLengthBounds(length);
            return packed % wordCounts[length];
        }

        public int UnpackTransformIndex(int length, int packed){
            CheckLengthBounds(length);
            return packed >> wordLengthBits[length];
        }

        public int GetWordPosition(int length, int word){
            CheckLengthBounds(length);
            return wordOffsets[length] + length * word;
        }

        public int GetPackedValue(int length, int word, int transform){
            int position = GetWordPosition(length, word);
            return (transform << wordLengthBits[length]) | position;
        }

        private void CheckLengthBounds(int length){
            if (length < minLength || length > maxLength){
                throw new ArgumentOutOfRangeException(nameof(length), "Dictionary word length must be between " + minLength + " and " + maxLength + ".");
            }
        }
    }
}
