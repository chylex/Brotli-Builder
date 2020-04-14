using System;
using BrotliLib.Collections;

namespace BrotliLib.Brotli.Dictionary.Transform{
    /// <summary>
    /// Represents a combined transformation that modifies the bytes of a word using transform <see cref="Type"/>, and then surrounds it with a <see cref="Prefix"/> and a <see cref="Suffix"/>.
    /// </summary>
    public sealed class WordTransform{
        public byte[] Prefix => CollectionHelper.Clone(prefix);
        public byte[] Suffix => CollectionHelper.Clone(suffix);

        public int PrefixLength => prefix.Length;
        public int SuffixLength => suffix.Length;

        public TransformType Type { get; }
        private readonly byte[] prefix;
        private readonly byte[] suffix;

        public WordTransform(byte[] prefix, TransformType transform, byte[] suffix){
            this.Type = transform;
            this.prefix = prefix;
            this.suffix = suffix;
        }

        public byte[] Process(byte[] rawWord){
            byte[] middle = Type.Process(rawWord);
            byte[] fullWord = new byte[prefix.Length + middle.Length + suffix.Length];
            
            Buffer.BlockCopy(prefix, 0, fullWord, 0, prefix.Length);
            Buffer.BlockCopy(middle, 0, fullWord, prefix.Length, middle.Length);
            Buffer.BlockCopy(suffix, 0, fullWord, prefix.Length + middle.Length, suffix.Length);

            return fullWord;
        }

        public bool MatchesPrefix(in ArraySegment<byte> input){
            return CollectionHelper.ContainsAt(input, 0, prefix);
        }

        public bool MatchesSuffix(in ArraySegment<byte> input){
            return CollectionHelper.ContainsAt(input, 0, suffix);
        }
    }
}
