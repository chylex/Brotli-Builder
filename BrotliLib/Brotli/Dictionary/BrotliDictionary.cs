using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli.Dictionary.Format;
using BrotliLib.Brotli.Dictionary.Source;
using BrotliLib.Brotli.Dictionary.Transform;
using BrotliLib.Collections.Trie;
using BrotliLib.Numbers;

namespace BrotliLib.Brotli.Dictionary{
    /// <summary>
    /// Defines a Brotli dictionary, and provides methods to read and transform words using the provided <see cref="Format"/> and list of <see cref="Transforms"/>.
    /// </summary>
    public class BrotliDictionary : IDisposable, IEnumerable<byte[]>{
        public IDictionaryFormat Format { get; }
        public IReadOnlyList<WordTransform> Transforms { get; }

        private readonly IDictionarySource source;
        private readonly int lengthBits;
        
        public BrotliDictionary(IDictionaryFormat format, IReadOnlyList<WordTransform> transforms, IDictionarySource source){
            this.Format = format;
            this.Transforms = transforms;

            this.source = source;
            this.lengthBits = (int)Math.Ceiling(Math.Log(Format.WordLengths.Max(), 2.0));
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

        /// <summary>
        /// Generates an index lookup trie for all words in the dictionary, including transformations, matching the criteria specified by the parameters.
        /// </summary>
        public MultiTrie<byte, int> GenerateIndex(in Range transformIndexRange = default, in Range transformLengthRange = default){
            var trie = new MultiTrie<byte, int>.Builder();

            int minTransformIndex = Math.Max(transformIndexRange.First, 0);
            int maxTransformIndex = Math.Min(transformIndexRange.Last, Transforms.Count - 1);

            foreach(int length in Format.WordLengths){
                for(int word = 0, count = Format.WordCount(length); word < count; word++){
                    byte[] raw = source.ReadBytes(Format.GetWordPosition(length, word), length);

                    for(int transform = minTransformIndex; transform <= maxTransformIndex; transform++){
                        byte[] transformed = Transforms[transform].Process(raw);

                        if (transformLengthRange.Contains(transformed.Length)){
                            var packed = Format.GetPackedValue(length, word, transform);
                            var index = (packed * (1 << lengthBits)) | length; // use arithmetic to check for overflow

                            trie.Insert(transformed, index);
                        }
                    }
                }
            }

            return trie.Build();
        }

        /// <summary>
        /// Takes an index from the lookup trie, and returns which word length and packed value it represents.
        /// </summary>
        public (int length, int packed) TranslateIndex(int index){
            return (index & ((1 << lengthBits) - 1), index >> lengthBits);
        }
    }
}
