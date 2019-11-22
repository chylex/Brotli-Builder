using System.Collections.Generic;
using BrotliLib.Brotli.Components;

namespace BrotliLib.Brotli.Encode{
    public interface IBrotliTransformer{
        IEnumerable<MetaBlock> Transform(MetaBlock original, BrotliGlobalState initialState);
    }
}
