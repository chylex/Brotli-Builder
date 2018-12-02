using System;

namespace BrotliLib.Brotli.Dictionary.Transform{
    /// <summary>
    /// Represents a combined transformation that modifies the bytes of a word using <see cref="transform"/>, and then surrounds it with a <see cref="prefix"/> and a <see cref="suffix"/>.
    /// </summary>
    public sealed class WordTransform{
        private readonly byte[] prefix, suffix;
        private readonly TransformType transform;

        public WordTransform(byte[] prefix, TransformType transform, byte[] suffix){
            this.prefix = prefix;
            this.transform = transform;
            this.suffix = suffix;
        }

        public byte[] Process(byte[] rawWord){
            byte[] middle = transform.Process(rawWord);
            byte[] fullWord = new byte[prefix.Length + middle.Length + suffix.Length];
            
            Buffer.BlockCopy(prefix, 0, fullWord, 0, prefix.Length);
            Buffer.BlockCopy(middle, 0, fullWord, prefix.Length, middle.Length);
            Buffer.BlockCopy(suffix, 0, fullWord, prefix.Length + middle.Length, suffix.Length);

            return fullWord;
        }
    }
}
