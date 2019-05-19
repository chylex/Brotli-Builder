using System;
using System.Collections.Generic;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Contents.Compressed;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Encode;

namespace BrotliImpl.Encoders{
    /// <summary>
    /// Encodes bytes into a series of compressed meta-blocks. For each byte, it attempts to find the nearest copy within the sliding window.
    /// </summary>
    public class EncodeGreedyCopySearch : IBrotliEncoder{
        public IEnumerable<MetaBlock> GenerateMetaBlocks(BrotliFileParameters parameters, byte[] bytes){
            var builder = new CompressedMetaBlockBuilder(parameters);
            int length = bytes.Length;

            (int copyLength, int copyDistance)? FindCopy(int start, int limitLength){
                if (start < InsertCopyLengths.MinCopyLength || start >= length - InsertCopyLengths.MinCopyLength || limitLength < InsertCopyLengths.MinCopyLength){
                    return null;
                }

                int maxLength = Math.Min(limitLength, InsertCopyLengths.MaxCopyLength);
                int maxDistance = Math.Min(start, parameters.WindowSize.Bytes);

                for(int distance = 1; distance <= maxDistance; distance++){
                    int match = 0;

                    while(match < maxLength && start + match < length && bytes[start + match] == bytes[start + match - distance]){
                        ++match;
                    }

                    if (match >= InsertCopyLengths.MinCopyLength){
                        return (match, distance);
                    }
                }

                return null;
            }

            var nextLiteralBatch = new List<Literal>();

            for(int index = 0; index < length;){
                var copy = FindCopy(index, DataLength.MaxUncompressedBytes - nextLiteralBatch.Count);
                int mbSize;

                if (copy == null){
                    nextLiteralBatch.Add(new Literal(bytes[index]));
                    index++;

                    mbSize = nextLiteralBatch.Count;
                }
                else{
                    var (copyLength, copyDistance) = copy.Value;
                    builder.AddInsertCopy(new InsertCopyCommand(nextLiteralBatch, copyLength, copyDistance));

                    nextLiteralBatch.Clear();
                    index += copyLength;

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
