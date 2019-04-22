using System.Collections.Generic;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.State;

namespace BrotliLib.Brotli.Encode{
    public interface IBrotliTransformer{
        IEnumerable<MetaBlock> Transform(MetaBlock original, BrotliGlobalState initialState);
    }
}
