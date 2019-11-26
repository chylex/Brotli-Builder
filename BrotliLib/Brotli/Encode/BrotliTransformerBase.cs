using System;
using System.Collections.Generic;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Compressed;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Serialization.Writer;

namespace BrotliLib.Brotli.Encode{
    public abstract class BrotliTransformerBase : IBrotliTransformer{
        (IList<MetaBlock> MetaBlocks, BrotliGlobalState NextState) IBrotliTransformer.Transform(MetaBlock original, BrotliGlobalState state, BrotliCompressionParameters parameters){
            return original switch{
                MetaBlock.LastEmpty le   => TransformLastEmpty(le, state, parameters),
                MetaBlock.PaddedEmpty pe => TransformPaddedEmpty(pe, state, parameters),
                MetaBlock.Uncompressed u => TransformUncompressed(u, state, parameters),
                MetaBlock.Compressed c   => TransformCompressed(c, state, parameters),
                _ => throw new InvalidOperationException("Unknown meta-block type: " + original.GetType().Name)
            };
        }

        protected virtual (IList<MetaBlock>, BrotliGlobalState) TransformLastEmpty(MetaBlock.LastEmpty original, BrotliGlobalState state, BrotliCompressionParameters parameters){
            return (Array.Empty<MetaBlock>(), state);
        }

        protected virtual (IList<MetaBlock>, BrotliGlobalState) TransformPaddedEmpty(MetaBlock.PaddedEmpty original, BrotliGlobalState state, BrotliCompressionParameters parameters){
            return (new MetaBlock[]{ original }, state);
        }

        protected virtual (IList<MetaBlock>, BrotliGlobalState) TransformUncompressed(MetaBlock.Uncompressed original, BrotliGlobalState state, BrotliCompressionParameters parameters){
            state.OutputBytes(original.UncompressedData);
            return (new MetaBlock[]{ original }, state);
        }

        protected virtual (IList<MetaBlock>, BrotliGlobalState) TransformCompressed(MetaBlock.Compressed original, BrotliGlobalState state, BrotliCompressionParameters parameters){
            MetaBlockCompressionData.Serialize(new BitWriterNull(), original.Data, new MetaBlockCompressionData.Context(original.Header, original.DataLength, state));
            return (new MetaBlock[]{ original }, state);
        }
    }
}
