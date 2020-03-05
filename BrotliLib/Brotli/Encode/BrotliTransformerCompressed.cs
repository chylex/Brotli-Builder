using System.Collections.Generic;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Parameters;

namespace BrotliLib.Brotli.Encode{
    public abstract class BrotliTransformerCompressed : BrotliTransformerBase{
        protected sealed override IList<(MetaBlock MetaBlock, BrotliGlobalState NextState)> TransformLastEmpty(MetaBlock.LastEmpty original, BrotliGlobalState state, BrotliCompressionParameters parameters){
            return base.TransformLastEmpty(original, state, parameters);
        }

        protected sealed override IList<(MetaBlock MetaBlock, BrotliGlobalState NextState)> TransformPaddedEmpty(MetaBlock.PaddedEmpty original, BrotliGlobalState state, BrotliCompressionParameters parameters){
            return base.TransformPaddedEmpty(original, state, parameters);
        }

        protected sealed override IList<(MetaBlock MetaBlock, BrotliGlobalState NextState)> TransformUncompressed(MetaBlock.Uncompressed original, BrotliGlobalState state, BrotliCompressionParameters parameters){
            return base.TransformUncompressed(original, state, parameters);
        }

        protected sealed override IList<(MetaBlock MetaBlock, BrotliGlobalState NextState)> TransformCompressed(MetaBlock.Compressed original, BrotliGlobalState state, BrotliCompressionParameters parameters){
            return new []{ Transform(original, state, parameters) };
        }
        
        protected abstract (MetaBlock, BrotliGlobalState) Transform(MetaBlock.Compressed original, BrotliGlobalState state, BrotliCompressionParameters parameters);
    }
}
