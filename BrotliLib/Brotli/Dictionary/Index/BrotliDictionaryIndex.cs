using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BrotliLib.Brotli.Dictionary.Format;
using BrotliLib.Brotli.Dictionary.Transform;
using BrotliLib.Collections;
using BrotliLib.Collections.Trie;

namespace BrotliLib.Brotli.Dictionary.Index{
    public sealed class BrotliDictionaryIndex{
        private const int MinEntryLength = 4; // TODO test to find whichever length is the most reasonable

        private readonly IDictionaryFormat format;
        private readonly IReadOnlyList<WordTransform> transforms;
        private readonly Dictionary<TransformType, MultiTrie<byte, (int, int)>> lookups;

        private readonly List<int> transformsNoPrefix;
        private readonly List<int> transformsWithPrefix;

        internal BrotliDictionaryIndex(BrotliDictionary dictionary){
            this.format = dictionary.Format;
            this.transforms = dictionary.Transforms;

            Stopwatch sw = Stopwatch.StartNew();

            MultiTrieCache<byte, (int, int)> cache = new MultiTrieCache<byte, (int, int)>();

            this.lookups = TransformTypes.All.ToDictionary(type => type, type => {
                var builder = new MultiTrieBuilder<byte, (int, int)>();

                foreach(var length in format.WordLengths.Where(length => type.GetTransformedLength(length) >= MinEntryLength)){
                    for(int word = 0, count = format.WordCount(length); word < count; word++){
                        byte[] bytes = type.Process(dictionary.ReadRaw(length, word));
                        builder.Insert(bytes, (length, word));
                    }
                }

                return builder.Build(cache);
            });

            this.transformsNoPrefix = Enumerable.Range(0, transforms.Count).Where(index => transforms[index].Prefix.Length == 0).ToList();
            this.transformsWithPrefix = Enumerable.Range(0, transforms.Count).Where(index => transforms[index].Prefix.Length > 0).ToList();

            sw.Stop();
            Debug.WriteLine("Constructed dictionary index in " + sw.ElapsedMilliseconds + " ms.");
        }

        public List<DictionaryIndexEntry> Find(byte[] bytes, int start, int maxLength){
            var entries = new List<DictionaryIndexEntry>();

            // default dictionary guarantees executing 44 identity, 12 ferment first, 12 ferment all transformations
            // TODO longest may not find correct entries w/ suffix, but it seems to work well enough
            var identityNoPrefixWords     = lookups[TransformType.Identity].FindLongest(CollectionHelper.Skip(bytes, start));
            var fermentFirstNoPrefixWords = lookups[TransformType.FermentFirst].FindLongest(CollectionHelper.Skip(bytes, start));
            var fermentAllNoPrefixWords   = lookups[TransformType.FermentAll].FindLongest(CollectionHelper.Skip(bytes, start));

            IEnumerable<(int, int)> LookupWords(TransformType type, int prefixLength){
                if (prefixLength == 0){
                    switch(type){
                        case TransformType.Identity: return identityNoPrefixWords;
                        case TransformType.FermentFirst: return fermentFirstNoPrefixWords;
                        case TransformType.FermentAll: return fermentAllNoPrefixWords;
                    }
                }

                return lookups[type].FindLongest(CollectionHelper.Skip(bytes, start + prefixLength));
            }

            var transformsMatchingPrefix = transformsNoPrefix.Concat(transformsWithPrefix.Where(index => CollectionHelper.ContainsAt(bytes, start, transforms[index].Prefix)));

            foreach(var transform in transformsMatchingPrefix){
                var wt = transforms[transform];

                var type = wt.Type;
                int prefixLength = wt.Prefix.Length;

                foreach(var (length, word) in LookupWords(type, prefixLength)){
                    var transformedLength = type.GetTransformedLength(length);

                    if (transformedLength <= maxLength && CollectionHelper.ContainsAt(bytes, start + prefixLength + transformedLength, wt.Suffix)){
                        int packedValue = format.GetPackedValue(length, word, transform);
                        int outputLength = transformedLength + prefixLength + wt.Suffix.Length;

                        entries.Add(new DictionaryIndexEntry(packedValue, length, outputLength));
                    }
                }
            }

            entries.Sort((e1, e2) => e2.OutputLength.CompareTo(e1.OutputLength));
            return entries;
        }
    }
}
