using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Brotli.Encode;
using BrotliLib.Collections;

namespace BrotliImpl.Transformers{
    public class TransformTestDistanceContextModel : CompressedMetaBlockTransformer{
        protected override IEnumerable<MetaBlock> Transform(MetaBlock.Compressed original, CompressedMetaBlockBuilder builder, BrotliGlobalState initialState){
            var tracker = new MetaBlockSizeTracker(initialState);

            var header = original.Header;

            int blockTypeCount = header.BlockTypes[Category.Distance].Count;
            int mapSize = blockTypeCount * ContextMap.Distances.TreesPerBlockType;
            int maxTrees;

            switch(blockTypeCount){
                case 1: maxTrees = mapSize; break;
                case 2: maxTrees = mapSize; break;
                case 3: maxTrees = 2; break;
                default: throw new NotImplementedException();
            }

            foreach(byte[] values in GenerateMaps(mapSize, maxTrees)){
                int trees = values.Distinct().Count();
                var map = new ContextMap.Distances(trees, blockTypeCount);

                for(int i = 0; i < values.Length; i++){
                    map[i] = values[i];
                }

                builder.DistanceCtxMap = map.Build();
                tracker.Test(builder, "[Trees = " + trees + ", Map = " + string.Join(", ", values) + "]");
            }

            yield return tracker.Smallest;
        }

        private static List<byte[]> GenerateMaps(int mapSize, int maxTrees = -1){
            if (maxTrees == -1){
                maxTrees = mapSize;
            }

            --maxTrees;

            var list = new List<byte[]>();
            byte[] temp = new byte[mapSize];

            void Rec(int index, int max){
                if (index == mapSize){
                    list.Add(CollectionHelper.Clone(temp));
                    return;
                }

                for(byte i = 0; i <= max; i++){
                    temp[index] = i;
                    Rec(index + 1, Math.Min(maxTrees, Math.Max(i + 1, max)));
                }
            }

            Rec(0, 0);
            return list;
        }

        private static string TimeIndexGeneration(int n){
            Stopwatch sw = Stopwatch.StartNew();
            var _ = GenerateMaps(n).Count;
            sw.Stop();
            return sw.ElapsedMilliseconds + " ms";
        }
    }
}
