﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BrotliLib.Brotli.Dictionary.Format;
using BrotliLib.Brotli.Dictionary.Transform;
using BrotliLib.Collections;

namespace BrotliLib.Brotli.Dictionary.Index{
    public sealed class BrotliDictionaryIndex{
        private readonly IDictionaryFormat format;
        private readonly IReadOnlyList<WordTransform> transforms;
        private readonly Dictionary<TransformType, PatriciaTree<(int, int)>> lookups;

        private readonly List<int> transformsNoPrefix;
        private readonly List<int> transformsWithPrefix;

        internal BrotliDictionaryIndex(BrotliDictionary dictionary){
            this.format = dictionary.Format;
            this.transforms = dictionary.Transforms;

            Stopwatch sw = Stopwatch.StartNew();

            this.lookups = TransformTypes.All.ToDictionary(type => type, type => {
                var tree = new PatriciaTree<(int, int)>();

                foreach(var length in format.WordLengths.Where(length => type.GetTransformedLength(length) >= 1)){
                    for(int word = 0, count = format.WordCount(length); word < count; word++){
                        byte[] bytes = type.Process(dictionary.ReadRaw(length, word));
                        tree.Insert(bytes, (length, word));
                    }
                }

                return tree;
            });

            this.transformsNoPrefix = Enumerable.Range(0, transforms.Count).Where(index => transforms[index].PrefixLength == 0).ToList();
            this.transformsWithPrefix = Enumerable.Range(0, transforms.Count).Where(index => transforms[index].PrefixLength > 0).ToList();

            sw.Stop();
            Debug.WriteLine("Constructed dictionary index in " + sw.ElapsedMilliseconds + " ms.");
        }

        /// <summary>
        /// Finds all <see cref="DictionaryIndexEntry"/> that match prefixes between <paramref name="minLength"/> and <paramref name="maxLength"/> long in the input <paramref name="bytes"/>.
        /// The order of results is undefined.
        /// </summary>
        public List<DictionaryIndexEntry> Find(ArraySegment<byte> bytes, int minLength = 1, int maxLength = int.MaxValue){
            var entries = new List<DictionaryIndexEntry>();

            // default dictionary guarantees executing 44 identity, 12 ferment first, 12 ferment all transformations
            var identityNoPrefixWords     = lookups[TransformType.Identity].FindAll(bytes, minLength);
            var fermentFirstNoPrefixWords = lookups[TransformType.FermentFirst].FindAll(bytes, minLength);
            var fermentAllNoPrefixWords   = lookups[TransformType.FermentAll].FindAll(bytes, minLength);

            IEnumerable<(int, int)> LookupWords(TransformType type, int prefixLength){
                if (prefixLength == 0){
                    switch(type){
                        case TransformType.Identity: return identityNoPrefixWords;
                        case TransformType.FermentFirst: return fermentFirstNoPrefixWords;
                        case TransformType.FermentAll: return fermentAllNoPrefixWords;
                    }
                }

                if (prefixLength <= bytes.Count){
                    return lookups[type].FindAll(bytes.Slice(prefixLength), Math.Max(1, minLength - prefixLength));
                }
                else{
                    return Enumerable.Empty<(int, int)>();
                }
            }

            var transformsMatchingPrefix = transformsNoPrefix.Concat(transformsWithPrefix.Where(index => transforms[index].MatchesPrefix(bytes)));

            foreach(var transform in transformsMatchingPrefix){
                var wt = transforms[transform];

                var type = wt.Type;
                int prefixLength = wt.PrefixLength;
                int suffixLength = wt.SuffixLength;

                foreach(var (length, word) in LookupWords(type, prefixLength)){
                    int transformedLength = type.GetTransformedLength(length);
                    int outputLength = transformedLength + prefixLength + suffixLength;

                    if (outputLength >= minLength && outputLength <= maxLength && wt.MatchesSuffix(bytes.Slice(prefixLength + transformedLength))){
                        entries.Add(new DictionaryIndexEntry(format.GetPackedValue(length, word, transform), length, outputLength));
                    }
                }
            }

            return entries;
        }
    }
}
