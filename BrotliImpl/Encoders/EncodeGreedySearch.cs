using System;
using System.Collections.Generic;
using BrotliImpl.Encoders.Utils;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Compressed;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Encode;

namespace BrotliImpl.Encoders{
    /// <summary>
    /// Encodes bytes into a series of compressed meta-blocks. For each byte, it attempts to find the nearest and longest copy within the sliding window, or dictionary word.
    /// </summary>
    public abstract class EncodeGreedySearch : IBrotliEncoder{
        private protected abstract Copy? FindCopy(BrotliFileParameters parameters, byte[] bytes, int start, int maxLength);

        // Implementations

        public sealed class OnlyBackReferences : EncodeGreedySearch{
            private readonly int minLength;

            public OnlyBackReferences(int minLength = 0){
                this.minLength = Math.Max(minLength, InsertCopyLengths.MinCopyLength);
            }

            private protected override Copy? FindCopy(BrotliFileParameters parameters, byte[] bytes, int start, int maxLength){
                int length = bytes.Length;

                if (start < InsertCopyLengths.MinCopyLength || start >= length - InsertCopyLengths.MinCopyLength || maxLength < InsertCopyLengths.MinCopyLength){
                    return null;
                }

                maxLength = Math.Min(maxLength, InsertCopyLengths.MaxCopyLength);
                int maxDistance = Math.Min(start, parameters.WindowSize.Bytes);

                for(int distance = 1; distance <= maxDistance; distance++){
                    int match = 0;

                    while(match < maxLength && start + match < length && bytes[start + match] == bytes[start + match - distance]){
                        ++match;
                    }

                    if (match >= minLength){
                        return new Copy.BackReference(match, distance);
                    }
                }

                return null;
            }
        }

        public sealed class OnlyDictionary : EncodeGreedySearch{
            private protected override Copy? FindCopy(BrotliFileParameters parameters, byte[] bytes, int start, int maxLength){
                var entries = parameters.Dictionary.Index.Find(bytes, start, maxLength);

                if (entries.Count == 0){
                    return null;
                }

                return new Copy.Dictionary(entries[0]);
            }
        }

        public sealed class Mixed : EncodeGreedySearch{
            private readonly EncodeGreedySearch findBackReferences;
            private readonly EncodeGreedySearch findDictionary;

            public Mixed(int minCopyLength = 0){
                this.findBackReferences = new OnlyBackReferences(minCopyLength);
                this.findDictionary = new OnlyDictionary();
            }

            private protected override Copy? FindCopy(BrotliFileParameters parameters, byte[] bytes, int start, int maxLength){
                Copy? found1 = findBackReferences.FindCopy(parameters, bytes, start, maxLength);
                Copy? found2 = findDictionary.FindCopy(parameters, bytes, start, maxLength);

                return (found1?.OutputLength ?? 0) >= (found2?.OutputLength ?? 0) ? found1 : found2;
            }
        }

        // Generation

        public IEnumerable<MetaBlock> GenerateMetaBlocks(BrotliFileParameters parameters, byte[] bytes){
            var builder = new CompressedMetaBlockBuilder(parameters);
            int length = bytes.Length;

            var nextLiteralBatch = new List<Literal>();

            for(int index = 0; index < length;){
                var copy = FindCopy(parameters, bytes, index, DataLength.MaxUncompressedBytes - nextLiteralBatch.Count);
                int mbSize;

                if (copy == null){
                    nextLiteralBatch.Add(new Literal(bytes[index]));
                    index++;

                    mbSize = nextLiteralBatch.Count;
                }
                else{
                    index += copy.AddCommand(parameters, builder, nextLiteralBatch);
                    nextLiteralBatch.Clear();
                    mbSize = builder.OutputSize;
                }

                if (mbSize == DataLength.MaxUncompressedBytes){
                    var (mb, next) = builder.Build();
                    builder = next();
                    yield return mb;
                }
            }

            if (nextLiteralBatch.Count > 0){
                builder.AddInsertCopy(new InsertCopyCommand(nextLiteralBatch));
            }

            yield return builder.Build().MetaBlock;
        }
    }
}
