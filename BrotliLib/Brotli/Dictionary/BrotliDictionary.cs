using System;
using System.Collections;
using System.Collections.Generic;
using BrotliLib.Brotli.Dictionary.Format;
using BrotliLib.Brotli.Dictionary.Index;
using BrotliLib.Brotli.Dictionary.Source;
using BrotliLib.Brotli.Dictionary.Transform;

namespace BrotliLib.Brotli.Dictionary{
    /// <summary>
    /// Defines a Brotli dictionary, and provides methods to read and transform words using the provided <see cref="Format"/> and list of <see cref="Transforms"/>.
    /// </summary>
    public class BrotliDictionary : IDisposable, IEnumerable<byte[]>{
        public IDictionaryFormat Format { get; }
        public IReadOnlyList<WordTransform> Transforms { get; }

        private readonly IDictionarySource source;
        
        public BrotliDictionary(IDictionaryFormat format, IReadOnlyList<WordTransform> transforms, IDictionarySource source){
            this.Format = format;
            this.Transforms = transforms;
            this.source = source;
        }
        
        public void Dispose(){
            source.Dispose();
        }
        
        /// <summary>
        /// Reads a word of the specified length without performing any transformation on it.
        /// </summary>
        /// <param name="length">Length of the word.</param>
        /// <param name="word">Word ID.</param>
        public byte[] ReadRaw(int length, int word){
            return source.ReadBytes(Format.GetWordPosition(length, word), length);
        }
        
        /// <summary>
        /// Reads a word of the specified length and performs a transformation on it.
        /// Note that transformations may cause the final word to be shorter than the specified length.
        /// </summary>
        /// <param name="length">Length of the word.</param>
        /// <param name="word">Word ID.</param>
        /// <param name="transform">Transformation ID.</param>
        public byte[] ReadTransformed(int length, int word, int transform){
            return Transforms[transform].Process(ReadRaw(length, word));
        }

        /// <summary>
        /// Reads a word of the specified length and performs a transformation on it.
        /// Note that transformations may cause the final word to be shorter than the specified length.
        /// </summary>
        /// <param name="length">Length of the word.</param>
        /// <param name="packed">Packed word and transformation IDs.</param>
        public byte[] ReadTransformed(int length, int packed){
            int word = Format.UnpackWordIndex(length, packed);
            int transform = Format.UnpackTransformIndex(length, packed);
            
            return ReadTransformed(length, word, transform);
        }

        /// <summary>
        /// Enumerates all words in the dictionary from shortest to longest.
        /// </summary>
        public IEnumerator<byte[]> GetEnumerator(){
            foreach(int length in Format.WordLengths){
                for(int word = 0, count = Format.WordCount(length); word < count; word++){
                    yield return source.ReadBytes(Format.GetWordPosition(length, word), length);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
