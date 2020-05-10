using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.Encode.Build;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Brotli.Utils;

namespace BrotliImpl.Transformers{
    public class TransformGreedyTopDownBlockSplitter : BrotliTransformerCompressed{
        protected override (MetaBlock, BrotliGlobalState) Transform(MetaBlock.Compressed original, BrotliGlobalState state, BrotliCompressionParameters parameters){
            var builder = new CompressedMetaBlockBuilder(original, state){
                LiteralCtxMap = ContextMapBuilder.Literals.Simple,
                DistanceCtxMap = ContextMapBuilder.Distances.Simple
            };

            foreach(var category in Categories.LID){
                builder.BlockTypes[category].Reset();
            }

            builder.UseSameLiteralContextMode(LiteralContextMode.UTF8);
            
            var tracker = new MetaBlockSizeTracker(state);
            tracker.Test(builder, parameters);
            
            foreach(var category in Categories.LID){
                TestBlockSplits(builder, parameters, tracker, category);
            }

            tracker.Test(original);
            return tracker.Smallest ?? throw new InvalidOperationException("Transformation did not generate any meta-blocks.");
        }

        private void TestBlockSplits(CompressedMetaBlockBuilder builder, BrotliCompressionParameters parameters, MetaBlockSizeTracker tracker, Category category){
            int totalBlockLength = builder.GetTotalBlockLength(category);

            if (totalBlockLength == 0){
                return;
            }

            int step = Math.Max(1, (int)Math.Floor(Math.Log(totalBlockLength, 1.15)));
            int stepTwice = step * 2;

            if (totalBlockLength < stepTwice){
                return;
            }

            var lengths = new List<int>{ totalBlockLength };
            var queue = new Queue<int>();
            queue.Enqueue(0);

            while(queue.TryDequeue(out int leftIndex)){
                if (lengths[leftIndex] < stepTwice){
                    continue;
                }

                int rightIndex = leftIndex + 1;

                lengths.Insert(rightIndex, lengths[leftIndex] - step);
                lengths[leftIndex] = step;

                ApplyBlockSplit(builder.BlockTypes[category], lengths);
                PrepareContextMap(builder, category, lengths.Count);
                tracker.Test(builder, parameters);

                while(lengths[leftIndex] + step < lengths[rightIndex] && lengths[rightIndex] >= stepTwice){
                    lengths[leftIndex] += step;
                    lengths[rightIndex] -= step;

                    ApplyBlockSplit(builder.BlockTypes[category], lengths);
                    PrepareContextMap(builder, category, lengths.Count);
                    tracker.Test(builder, parameters);
                }

                var smallest = tracker.Smallest;

                if (smallest == null){
                    return;
                }

                var mb = smallest.Value.Item1;
                int prev = lengths.Count;

                lengths.Clear();
                lengths.Add(mb.Header.BlockTypes[category].InitialLength);
                lengths.AddRange(mb.Data.BlockSwitchCommands[category].Select(command => command.Length));

                var finalLength = totalBlockLength - lengths.Sum();

                if (finalLength > 0){
                    lengths.Add(totalBlockLength - lengths.Sum());
                }

                if (lengths.Count >= prev){
                    queue.Enqueue(leftIndex);
                    queue.Enqueue(rightIndex);
                }
            }
        }

        private void ApplyBlockSplit(BlockSwitchBuilder builder, List<int> lengths){
            builder.Reset();

            if (lengths.Count < 2){
                return;
            }

            for(int i = 0; i < lengths.Count; i++){
                builder.AddBlock((byte)(i % 256), lengths[i]);
            }
        }

        private void PrepareContextMap(CompressedMetaBlockBuilder builder, Category category, int lengthSplits){
            int blockTypes = Math.Min(lengthSplits, 256);

            if (category == Category.Literal){
                if (builder.LiteralCtxMap.BlockTypes != blockTypes){
                    builder.UseSameLiteralContextMode(LiteralContextMode.UTF8);
                    builder.LiteralCtxMap = new ContextMapBuilder.Literals(blockTypes).RepeatFirstBlockType(lengthSplits <= blockTypes).Build();
                }
            }
            else if (category == Category.Distance){
                if (builder.DistanceCtxMap.BlockTypes != blockTypes){
                    builder.DistanceCtxMap = new ContextMapBuilder.Distances(blockTypes).RepeatFirstBlockType(lengthSplits <= blockTypes).Build();
                }
            }
        }
    }
}
