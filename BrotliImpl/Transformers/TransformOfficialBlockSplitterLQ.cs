using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.Encode.Build;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Brotli.Utils;
using BrotliLib.Collections;

namespace BrotliImpl.Transformers{
    public class TransformOfficialBlockSplitterLQ : BrotliTransformerCompressed{
        protected override (MetaBlock, BrotliGlobalState) Transform(MetaBlock.Compressed original, BrotliGlobalState state, BrotliCompressionParameters parameters){
            var builder = new CompressedMetaBlockBuilder(original, state);

            var literals = new List<Literal>(builder.GetTotalBlockLength(Category.Literal));
            var lengthCodes = new List<InsertCopyLengthCode>(builder.GetTotalBlockLength(Category.InsertCopy));
            var distanceCodes = new List<DistanceCode>(builder.GetTotalBlockLength(Category.Distance));

            var distanceFreq = new FrequencyList<DistanceCode>();
            var validDistanceCodes = new List<DistanceCode>(5);

            foreach(var command in original.Data.InsertCopyCommands){
                literals.AddRange(command.Literals);
                state.OutputLiterals(command.Literals);

                if (command.CopyDistance == DistanceInfo.EndsAfterLiterals){
                    lengthCodes.Add(command.Lengths.MakeCode(ImplicitDistanceCodeZero.PreferEnabled));
                    break;
                }

                if (!command.CopyDistance.FindCodes(original.Header.DistanceParameters, state, validDistanceCodes)){
                    lengthCodes.Add(command.Lengths.MakeCode(ImplicitDistanceCodeZero.ForceEnabled));
                }
                else{
                    DistanceCode distanceCode;

                    if (command.CopyDistance == DistanceInfo.ExplicitCodeZero){
                        distanceCode = DistanceCode.Zero;
                    }
                    else{
                        distanceCode = validDistanceCodes.Count > 1 ? parameters.DistanceCodePicker(validDistanceCodes, distanceFreq) : validDistanceCodes[0];
                    }

                    distanceFreq.Add(distanceCode);
                    distanceCodes.Add(distanceCode);
                    lengthCodes.Add(command.Lengths.MakeCode(ImplicitDistanceCodeZero.Disable));
                }
            }

            var origLitCtxMap = builder.LiteralCtxMap;

            if (origLitCtxMap.TreeCount == 1){
                Split(builder, Category.Literal, literals, 512, 400.0);

                builder.UseSameLiteralContextMode(LiteralContextMode.UTF8);
                builder.LiteralCtxMap = new ContextMapBuilder.Literals(builder).RepeatFirstBlockType(true).Build();
            }
            else{
                var literalContextMap = Enumerable.Range(0, origLitCtxMap.ContextsPerBlockType).Select(index => origLitCtxMap.DetermineTreeID(0, index)).ToArray();
                var literalContextMode = builder.LiteralContextModes[0];

                var literalBuffer = RingBufferFast<byte>.From(0, 0);
                
                Split(builder, Category.Literal, literals, 512, 400.0, new BlockSplitter<Literal>.ContextInfo(literalContextMap, literal => {
                    literalBuffer.Push(literal.Value);
                    return literalContextMode.DetermineContextID(literalBuffer.Front, literalBuffer.Back);
                }));

                builder.UseSameLiteralContextMode(literalContextMode);
                builder.LiteralCtxMap = new ContextMapBuilder.Literals(builder).Set(0, literalContextMap).RepeatFirstBlockType(true).Build();
            }

            Split(builder, Category.InsertCopy, lengthCodes, 1024, 500.0);
            Split(builder, Category.Distance, distanceCodes, 512, 100.0);

            builder.DistanceCtxMap = new ContextMapBuilder.Distances(builder).RepeatFirstBlockType(true).Build();

            return builder.Build(parameters);
        }

        private void Split<T>(CompressedMetaBlockBuilder builder, Category category, List<T> sequence, int minBlockSize, double splitThreshold, in BlockSplitter<T>.ContextInfo? context = null) where T : IComparable<T>{
            new BlockSplitter<T>(minBlockSize, splitThreshold).ProcessSequence(builder.BlockTypes[category], sequence, context);
        }

        /// <summary>
        /// Adapted from https://github.com/google/brotli/blob/master/c/enc/metablock_inc.h (BlockSplitter) and https://github.com/google/brotli/blob/master/c/enc/metablock.c (ContextBlockSplitter)
        /// </summary>
        private sealed class BlockSplitter<T> where T : IComparable<T>{
            /// <summary>
            /// Adapted from https://github.com/google/brotli/blob/master/c/enc/bit_cost.h (BitsEntropy, ShannonEntropy)
            /// </summary>
            private static double BitsEntropy(FrequencyList<T> histogram){
                int total = 0;
                double calc = 0.0;

                foreach(var symbol in histogram){
                    var freq = histogram[symbol];

                    total += freq;
                    calc -= freq * Math.Log(freq, 2.0);
                }

                if (total > 0){
                    calc += total * Math.Log(total, 2.0);
                }

                return Math.Max(calc, total);
            }

            private static void Swap<U>(U[] array, int index1, int index2){
                U tmp = array[index1];
                array[index1] = array[index2];
                array[index2] = tmp;
            }

            public readonly struct ContextInfo{
                public static ContextInfo Default { get; } = new ContextInfo(new byte[]{ 0 }, _ => 0);

                public byte[] Map { get; }
                public Func<T, int> GetIndex { get; }

                public ContextInfo(byte[] map, Func<T, int> getIndex){
                    this.Map = map;
                    this.GetIndex = getIndex;
                }
            }

            private readonly int minBlockSize;
            private readonly double splitThreshold;

            public BlockSplitter(int minBlockSize, double splitThreshold){
                this.minBlockSize = minBlockSize;
                this.splitThreshold = splitThreshold;
            }

            public void ProcessSequence(BlockSwitchBuilder builder, List<T> sequence, ContextInfo? contextParam = null){
                builder.Reset();

                ContextInfo ctx = contextParam ?? ContextInfo.Default;

                int numContexts = 1 + ctx.Map.Max();
                int maxBlockTypes = 256 / numContexts;

                int maxNumBlocks = sequence.Count / minBlockSize + 1;
                var histograms = FrequencyList<T>.Array(numContexts * Math.Min(maxNumBlocks, maxBlockTypes + 1));

                int numBlocks = 0;
                int blockSize = 0;
                int targetBlockSize = minBlockSize;
                int currHistogramIx = 0;
                var lastHistogramIx = new int[2];
                var lastEntropy = new double[2 * numContexts];
                int mergeLastCount = 0;

                void FinishBlock(bool isFinal){
                    blockSize = Math.Max(blockSize, minBlockSize);

                    if (numBlocks == 0){
                        builder.SetInitialLength(blockSize);

                        for(int context = 0; context < numContexts; context++){
                            lastEntropy[context] = BitsEntropy(histograms[context]);
                            lastEntropy[context + numContexts] = lastEntropy[context];
                        }

                        currHistogramIx += numContexts;

                        ++numBlocks;
                        blockSize = 0;
                    }
                    else if (blockSize > 0){
                        var entropy = new double[numContexts];
                        var combinedHisto = FrequencyList<T>.Array(2 * numContexts);
                        var combinedEntropy = new double[2 * numContexts];
                        var diff = new double[2];

                        for(int context = 0; context < numContexts; context++){
                            int currHistoIx = currHistogramIx + context;

                            entropy[context] = BitsEntropy(histograms[currHistoIx]);

                            for(int i = 0; i < 2; i++){
                                int ix = i * numContexts + context;
                                int lastHistogramIxValue = lastHistogramIx[i] + context;

                                var newHisto = new FrequencyList<T>{
                                    histograms[currHistoIx],
                                    histograms[lastHistogramIxValue]
                                };

                                combinedHisto[ix] = newHisto;
                                combinedEntropy[ix] = BitsEntropy(combinedHisto[ix]);
                                diff[i] += combinedEntropy[ix] - entropy[context] - lastEntropy[ix];
                            }
                        }

                        if (builder.TypeCount < maxBlockTypes && diff[0] > splitThreshold && diff[1] > splitThreshold){
                            byte nextBlockType = (byte)(builder.TypeCount);

                            builder.AddBlock(nextBlockType, blockSize);

                            lastHistogramIx[1] = lastHistogramIx[0];
                            lastHistogramIx[0] = nextBlockType * numContexts;

                            for(int context = 0; context < numContexts; context++){
                                lastEntropy[context + numContexts] = lastEntropy[context];
                                lastEntropy[context] = entropy[context];
                            }

                            currHistogramIx += numContexts;

                            if (currHistogramIx < histograms.Length){
                                histograms[currHistogramIx].Clear();
                            }
                            
                            ++numBlocks;
                            blockSize = 0;
                            mergeLastCount = 0;
                            targetBlockSize = minBlockSize;
                        }
                        else if (diff[1] < diff[0] - 20.0){
                            builder.AddBlock(builder.Commands.Count >= 2 ? builder.Commands[^2].Type : (byte)0, blockSize);

                            Swap(lastHistogramIx, 0, 1);

                            for(int context = 0; context < numContexts; context++){
                                histograms[lastHistogramIx[0] + context] = combinedHisto[context + numContexts];

                                lastEntropy[context + numContexts] = lastEntropy[context];
                                lastEntropy[context] = combinedEntropy[context + numContexts];

                                histograms[currHistogramIx + context].Clear();
                            }
                            
                            ++numBlocks;
                            blockSize = 0;
                            mergeLastCount = 0;
                            targetBlockSize = minBlockSize;
                        }
                        else{
                            builder.AddBlock(builder.LastCommand?.Type ?? 0, blockSize);
                            
                            for(int context = 0; context < numContexts; context++){
                                histograms[lastHistogramIx[0] + context] = combinedHisto[context];
                                lastEntropy[context] = combinedEntropy[context];

                                if (builder.TypeCount == 1){
                                    lastEntropy[context + numContexts] = lastEntropy[context];
                                }

                                histograms[currHistogramIx + context].Clear();
                            }

                            blockSize = 0;

                            if (++mergeLastCount > 1){
                                targetBlockSize += minBlockSize;
                            }
                        }
                    }

                    if (isFinal && builder.TotalLength < sequence.Count){
                        builder.AddFinalBlock((byte)(builder.TypeCount - 1));
                    }
                }

                foreach(T symbol in sequence){
                    histograms[currHistogramIx + ctx.Map[ctx.GetIndex(symbol)]].Add(symbol);

                    if (++blockSize == targetBlockSize){
                        FinishBlock(isFinal: false);
                    }
                }

                FinishBlock(isFinal: true);
            }
        }
    }
}
