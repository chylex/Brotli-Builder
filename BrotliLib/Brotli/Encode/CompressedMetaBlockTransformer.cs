﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.State;
using BrotliLib.IO;

namespace BrotliLib.Brotli.Encode{
    public abstract class CompressedMetaBlockTransformer : IBrotliTransformer{
        public IEnumerable<MetaBlock> Transform(MetaBlock original, BrotliGlobalState initialState){
            if (!(original is MetaBlock.Compressed compressed)){
                yield return original;
                yield break;
            }

            foreach(MetaBlock transformed in Transform(compressed, new CompressedMetaBlockBuilder(compressed, initialState), initialState)){
                yield return transformed;
            }
        }

        protected abstract IEnumerable<MetaBlock> Transform(MetaBlock.Compressed original, CompressedMetaBlockBuilder builder, BrotliGlobalState initialState);

        protected int CountBits(MetaBlock tested, BrotliGlobalState state){
            var stream = new BitStream();

            try{
                MetaBlock.Serializer.ToBits(stream.GetWriter(), tested, state.Clone());
                return stream.Length;
            }catch(Exception ex){
                Debug.WriteLine(ex.ToString());
                return int.MaxValue;
            }
        }
    }
}