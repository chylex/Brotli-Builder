using System;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Parameters;

namespace BrotliLib.Brotli.Streaming{
    /// <summary>
    /// Provides a streaming meta-block generator.
    /// </summary>
    public interface IBrotliFileStream{
        BrotliFileParameters Parameters { get; }
        BrotliGlobalState State { get; }

        MetaBlock? NextMetaBlock();

        void ForEachRemainingMetaBlock(Action<MetaBlock> action){
            MetaBlock? metaBlock;

            while((metaBlock = NextMetaBlock()) != null){
                action(metaBlock);
            }
        }
    }

    public static class IBrotliFileStreamExtensions{
        public static void ForEachRemainingMetaBlock(this IBrotliFileStream me, Action<MetaBlock> action){
            me.ForEachRemainingMetaBlock(action);
        }
    }
}
