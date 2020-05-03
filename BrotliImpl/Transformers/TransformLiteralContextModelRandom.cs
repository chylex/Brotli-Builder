using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Encode;

namespace BrotliImpl.Transformers{
    public class TransformLiteralContextModelRandom : CompressedMetaBlockTransformer{
        private const int Attempts = 1000;
        private static readonly LiteralContextMode[] Modes = (LiteralContextMode[])Enum.GetValues(typeof(LiteralContextMode));
        
        private readonly int minTrees, maxTrees;

        public TransformLiteralContextModelRandom(int minTrees, int maxTrees){
            this.minTrees = minTrees;
            this.maxTrees = maxTrees;
        }

        protected override IEnumerable<MetaBlock> Transform(MetaBlock.Compressed original, CompressedMetaBlockBuilder builder, BrotliGlobalState initialState){
            int originalSize = MetaBlockSizeTracker.CountBits(original, initialState);

            int minResultTrees = 1;
            LiteralContextMode? minResultMode = null;
            MetaBlockSizeTracker minResultInst = null;

            for(int i = minTrees; i <= maxTrees; i++){
                int trees = i;

                var results = Modes.AsParallel()
                                   .WithDegreeOfParallelism(Modes.Length)
                                   .Select(mode => PerformRun(mode, trees, original, initialState))
                                   .OrderBy(result => result.tracker.SmallestSize)
                                   .ToArray();

                foreach(var (mode, tracker) in results){
                    Debug.WriteLine("Min [" + mode + "/" + trees + "] = " + tracker.SmallestSize + " bits");
                }
                
                Debug.WriteLine("");

                var localBest = results[0];

                if (minResultInst == null || localBest.tracker.SmallestSize < minResultInst.SmallestSize){
                    minResultMode = localBest.mode;
                    minResultInst = localBest.tracker;
                    minResultTrees = trees;
                }
            }

            Debug.WriteLine("Original = " + originalSize + " bits");
            Debug.WriteLine("Best = " + minResultInst?.SmallestSize + " bits (" + minResultTrees + " trees with " + minResultMode + " mode)");

            if (minResultInst != null && minResultInst.SmallestSize < originalSize){
                yield return minResultInst.Smallest;
            }
            else{
                yield return original;
            }
        }

        private (LiteralContextMode mode, MetaBlockSizeTracker tracker) PerformRun(LiteralContextMode mode, int trees, MetaBlock.Compressed original, BrotliGlobalState initialState){
            var rand = new Random();
            var modes = original.Header.LiteralCtxModes.Select(_ => mode).ToArray();

            MetaBlockSizeTracker tracker = new MetaBlockSizeTracker(initialState);

            for(int attempt = 0; attempt < Attempts; attempt++){
                tracker.Test(Generate(original, initialState, trees, modes, rand));
            }

            return (mode, tracker);
        }

        private CompressedMetaBlockBuilder Generate(MetaBlock.Compressed original, BrotliGlobalState initialState, int trees, LiteralContextMode[] modes, Random rand){
            return new CompressedMetaBlockBuilder(original, initialState){
                LiteralCtxMap = GenerateContextMap1(trees, modes.Length, rand),
                LiteralContextModes = modes
            };
        }

        // Algorithms

        private static ContextMap GenerateContextMap1(int trees, int blocks, Random rand){
            var map = new ContextMap.Literals(trees, blocks);

            for(int index = 0; index < map.Length; index++){
                map[index] = (byte)rand.Next(trees);
            }

            return map.Build();
        }

        private static ContextMap GenerateContextMap2(int trees, int blocks, Random rand){
            var map = new ContextMap.Literals(trees, blocks);

            List<int> breakpoints = new List<int>();
            List<int> indices = Enumerable.Range(1, map.Length - 1).ToList();

            for(int bp = 0; bp < trees - 1; bp++){
                int picked = indices[rand.Next(indices.Count)];

                breakpoints.Add(picked);
                indices.Remove(picked);
            }

            breakpoints.Sort();
            breakpoints.Add(map.Length);

            for(byte tree = 1; tree < trees; tree++){
                for(int index = breakpoints[tree - 1]; index < breakpoints[tree]; index++){
                    map[index] = tree;
                }
            }

            return map.Build();
        }
    }
}
