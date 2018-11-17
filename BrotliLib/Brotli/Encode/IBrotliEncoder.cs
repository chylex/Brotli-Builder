﻿using System.Collections.Generic;
using BrotliLib.Brotli.Components;

namespace BrotliLib.Brotli.Encode{
    /// <summary>
    /// Allows converting bytes into a series of <see cref="MetaBlock"/> objects.
    /// </summary>
    public interface IBrotliEncoder{
        IEnumerable<MetaBlock> GenerateMetaBlocks(byte[] bytes);
    }
}