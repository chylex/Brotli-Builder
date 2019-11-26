﻿using System;
using System.Collections.Generic;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Collections;

namespace BrotliImpl.Encoders{
    /// <summary>
    /// Encodes bytes into a series of uncompressed meta-blocks, with an empty meta-block at the end.
    /// </summary>
    public class EncodeUncompressedOnly : IBrotliEncoder{
        public IEnumerable<MetaBlock> GenerateMetaBlocks(BrotliFileParameters parameters, byte[] bytes){
            for(int index = 0; index < bytes.Length; index += DataLength.MaxUncompressedBytes){
                int mbBytes = Math.Min(bytes.Length - index, DataLength.MaxUncompressedBytes);
                byte[] mbData = CollectionHelper.Slice(bytes, index, mbBytes);

                yield return new MetaBlock.Uncompressed(mbData);
            }

            yield return new MetaBlock.LastEmpty();
        }
    }
}
