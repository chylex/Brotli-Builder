using System;
using System.Collections.Generic;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Contents.Compressed;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;

namespace BrotliLib.Brotli.Encode.Impl{
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

                byte[] mbData = new byte[mbBytes];
                Buffer.BlockCopy(bytes, index, mbData, 0, mbBytes);
                
                var (mb, next) = builder.AddInsertCopy(new InsertCopyCommand(Literal.FromBytes(mbData), InsertCopyLengths.MinimumCopyLength))
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
