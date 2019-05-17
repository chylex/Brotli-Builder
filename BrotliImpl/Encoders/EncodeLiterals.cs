using System;
using System.Collections.Generic;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Contents.Compressed;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Encode;
using BrotliLib.Collections;

namespace BrotliImpl.Encoders{
    /// <summary>
    /// Encodes bytes into a series of compressed meta-blocks, where each contains a single insert&amp;copy command with each byte stored as a literal.
    /// </summary>
    public class EncodeLiterals : IBrotliEncoder{
        public IEnumerable<MetaBlock> GenerateMetaBlocks(BrotliFileParameters parameters, byte[] bytes){
            int length = bytes.Length;
            var builder = new CompressedMetaBlockBuilder(parameters);

            for(int index = 0, nextIndex; index < length; index = nextIndex){
                nextIndex = index + DataLength.MaxUncompressedBytes;

                int mbBytes = Math.Min(length - index, DataLength.MaxUncompressedBytes);
                byte[] mbData = CollectionHelper.Slice(bytes, index, mbBytes);
                
                var (mb, next) = builder.AddInsertCopy(new InsertCopyCommand(Literal.FromBytes(mbData), InsertCopyLengths.MinCopyLength))
                                        .Build();
                
                if (nextIndex < length){
                    builder = next();
                }

                yield return mb;
            }

            if (length == 0){
                yield return new MetaBlock.LastEmpty();
            }
        }
    }
}
