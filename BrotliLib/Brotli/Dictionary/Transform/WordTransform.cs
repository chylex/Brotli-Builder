using System;

namespace BrotliLib.Brotli.Dictionary.Transform{
    /// <summary>
    /// Represents a combined transformation that modifies the bytes of a word using transform <see cref="Type"/>, and then surrounds it with a <see cref="Prefix"/> and a <see cref="Suffix"/>.
    /// </summary>
    public sealed class WordTransform{
        public TransformType Type { get; }
        internal byte[] Prefix { get; }
        internal byte[] Suffix { get; }

        public WordTransform(byte[] prefix, TransformType transform, byte[] suffix){
            this.Type = transform;
            this.Prefix = prefix;
            this.Suffix = suffix;
        }

        public byte[] Process(byte[] rawWord){
            byte[] middle = Type.Process(rawWord);
            byte[] fullWord = new byte[Prefix.Length + middle.Length + Suffix.Length];
            
            Buffer.BlockCopy(Prefix, 0, fullWord, 0, Prefix.Length);
            Buffer.BlockCopy(middle, 0, fullWord, Prefix.Length, middle.Length);
            Buffer.BlockCopy(Suffix, 0, fullWord, Prefix.Length + middle.Length, Suffix.Length);

            return fullWord;
        }
    }
}
