using System.Collections.Generic;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Parameters;

namespace BrotliLib.Brotli.Encode{
    public abstract class BrotliTransformerCompressed : BrotliTransformerBase{
        protected sealed override (IList<MetaBlock>, BrotliGlobalState) TransformLastEmpty(MetaBlock.LastEmpty original, BrotliGlobalState state, BrotliCompressionParameters parameters){
            return base.TransformLastEmpty(original, state, parameters);
        }

        protected sealed override (IList<MetaBlock>, BrotliGlobalState) TransformPaddedEmpty(MetaBlock.PaddedEmpty original, BrotliGlobalState state, BrotliCompressionParameters parameters){
            return base.TransformPaddedEmpty(original, state, parameters);
        }

        protected sealed override (IList<MetaBlock>, BrotliGlobalState) TransformUncompressed(MetaBlock.Uncompressed original, BrotliGlobalState state, BrotliCompressionParameters parameters){
            return base.TransformUncompressed(original, state, parameters);
        }

        protected sealed override (IList<MetaBlock>, BrotliGlobalState) TransformCompressed(MetaBlock.Compressed original, BrotliGlobalState state, BrotliCompressionParameters parameters){
            var (metaBlock, nextState) = Transform(original, state, parameters);
            return (new MetaBlock[]{ metaBlock }, nextState);
        }
        
        protected abstract (MetaBlock, BrotliGlobalState) Transform(MetaBlock.Compressed original, BrotliGlobalState state, BrotliCompressionParameters parameters);
    }
}
