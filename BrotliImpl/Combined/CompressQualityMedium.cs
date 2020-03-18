using System;
using System.Linq;
using BrotliImpl.Combined.Hashers;
using BrotliImpl.Utils;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.Encode.Build;
using BrotliLib.Brotli.Parameters;

namespace BrotliImpl.Combined{
    /// <summary>
    /// Reimplementation of official Brotli compression, with common code used by medium compression qualities (2-9).
    ///
    /// Currently, none of the advanced <see cref="Features"/> are actually implemented,
    /// but they change some properties of the backward reference search and meta-block continuation.
    /// </summary>
    public abstract class CompressQualityMedium : BrotliEncodePipeline{
        [Flags]
        public enum Features{
            None = 0,
            BlockSplit = 1, // TODO implement
            ExtensiveReferenceSearch = 2, // TODO implement
        }

        protected virtual Features SupportedFeatures => Features.None;
        
        protected override WindowSize DetermineWindowSize(byte[] bytes){
            return WindowSize.ForFileSize(bytes.Length);
        }

        protected sealed override IBrotliEncoder CreateEncoder(byte[] bytes, BrotliFileParameters fileParameters){
            return new Encoder(bytes, CreateHasher(bytes, fileParameters), fileParameters, SupportedFeatures);
        }

        private protected abstract IHasher CreateHasher(byte[] bytes, BrotliFileParameters fileParameters);
        
        private class Encoder : IBrotliEncoder{
            private const int MaxDistanceWithDefaultParams = 0x3FFFFFC;
            private const int LiteralSpreeLengthForSparseSearch = 64;
            private const int CostDiffLazy = 175;
            
            /// <summary>
            /// Adapted from https://github.com/google/brotli/blob/master/c/enc/quality.h (ComputeLgBlock).
            /// </summary>
            private static int ComputeLgBlock(Features features){
                return features.HasFlag(Features.BlockSplit) ? 16 : 14;
            }
            
            /// <summary>
            /// Adapted from https://github.com/google/brotli/blob/master/c/enc/quality.h (MaxMetablockSize, ComputeRbBits).
            /// </summary>
            private static int MaxMetaBlockSize(BrotliFileParameters fileParameters, int lgBlock){
                int rb = 1 + Math.Max(fileParameters.WindowSize.Bits, lgBlock);
                int bits = Math.Min(rb, WindowSize.MaxBits);

                return 1 << bits;
            }
            
            /// <summary>
            /// Adapted from https://github.com/google/brotli/blob/master/c/enc/encode.c (InputBlockSize).
            /// </summary>
            private static int InputBlockSize(int lgBlock){
                return 1 << lgBlock;
            }

            private readonly byte[] input;
            private readonly IHasher hasher;
            private readonly BrotliFileParameters fileParameters;

            private readonly Features features;
            private readonly int lgBlock;

            private int position;
            private int lastInsertLen;
            private int lastProcessedPos;

            public Encoder(byte[] input, IHasher hasher, BrotliFileParameters fileParameters, Features features){
                this.input = input;
                this.hasher = hasher;
                this.fileParameters = fileParameters;

                this.features = features;
                this.lgBlock = ComputeLgBlock(features);
            }
            
            /// <summary>
            /// Adapted from https://github.com/google/brotli/blob/master/c/enc/encode.c (EncodeData).
            /// </summary>
            public (MetaBlock MetaBlock, BrotliEncodeInfo Next) Encode(BrotliEncodeInfo info){
                var builder = info.NewBuilder();

                do{
                    position = lastProcessedPos;
                    // TODO extend last command somehow???

                    int chunkLength = Math.Min(input.Length - position - lastInsertLen, InputBlockSize(lgBlock));

                    if (chunkLength == 0){
                        break;
                    }

                    hasher.StitchToPreviousBlock(chunkLength, position);

                    lastProcessedPos = position + chunkLength;
                    CreateBackwardReferences(builder, chunkLength);
                }while(lastProcessedPos + lastInsertLen < input.Length && ShouldContinueThisBlock(builder));

                if (lastInsertLen > 0){
                    builder.AddInsert(Literal.FromBytes(info.Bytes.Slice(builder.OutputSize, lastInsertLen)));
                    lastInsertLen = 0;
                }

                return builder.Build(info);
            }
            
            /// <summary>
            /// Adapted from https://github.com/google/brotli/blob/master/c/enc/encode.c (EncodeData).
            /// </summary>
            private bool ShouldContinueThisBlock(CompressedMetaBlockBuilder builder){
                int maxLength = MaxMetaBlockSize(fileParameters, lgBlock);

                if (builder.OutputSize + InputBlockSize(lgBlock) > maxLength){
                    return false;
                }
                
                int totalCommands = builder.InsertCopyCommands.Count;
                int totalLiterals = builder.InsertCopyCommands.Sum(icCommand => icCommand.Literals.Count);

                if (!features.HasFlag(Features.BlockSplit) && totalCommands + totalLiterals >= 0x2FFF /* 12287 */){
                    return false;
                }
                
                int maxCommands = maxLength / 8;
                int maxLiterals = maxLength / 8;

                return totalCommands < maxCommands && totalLiterals < maxLiterals;
            }
            
            /// <summary>
            /// Adapted from https://github.com/google/brotli/blob/master/c/enc/backward_references.c (BrotliCreateBackwardReferences).
            /// </summary>
            private void CreateBackwardReferences(CompressedMetaBlockBuilder builder, int chunkLength){
                int maxBackwardLimit = fileParameters.WindowSize.Bytes;
                int insertLength = lastInsertLen;

                int posEnd = position + chunkLength;
                int storeEnd = chunkLength >= hasher.StoreLookahead ? posEnd - hasher.StoreLookahead + 1 : position;

                int applyRandomHeuristics = position + LiteralSpreeLengthForSparseSearch;
                
                while(position + hasher.HashTypeLength < posEnd){
                    int maxLength = posEnd - position;
                    int maxDistance = Math.Min(position, maxBackwardLimit);
                    int dictionaryStart = Math.Min(position, maxBackwardLimit);

                    var result = hasher.FindLongestMatch(position, maxLength, maxDistance, dictionaryStart, builder.LastDistance, 0);

                    if (result.FoundAnything){
                        int delayedBackwardReferencesInRow = 0;
                        --maxLength;

                        while(true){
                            maxDistance = Math.Min(position + 1, maxBackwardLimit);
                            dictionaryStart = Math.Min(position + 1, maxBackwardLimit);

                            var bestLenIn = features.HasFlag(Features.ExtensiveReferenceSearch) ? 0 : Math.Min(result.Copy.OutputLength - 1, maxLength);
                            var result2 = hasher.FindLongestMatch(position + 1, maxLength, maxDistance, dictionaryStart, builder.LastDistance, bestLenIn);

                            if (result2.Score >= result.Score + CostDiffLazy){
                                ++position;
                                ++insertLength;
                                result = result2;

                                if (++delayedBackwardReferencesInRow < 4 && position + hasher.HashTypeLength < posEnd){
                                    --maxLength;
                                    continue;
                                }
                            }

                            break;
                        }

                        var copy = result.Copy;
                        int len = copy.OutputLength;

                        applyRandomHeuristics = position + (2 * len) + LiteralSpreeLengthForSparseSearch;

                        copy.AddCommand(builder, Literal.FromBytes(input, position - insertLength, insertLength));
                        insertLength = 0;

                        int rangeStart = position + 2;
                        int rangeEnd = Math.Min(position + len, storeEnd);

                        if (copy is Copy.BackReference backReference && backReference.Distance < len >> 2){
                            rangeStart = Math.Min(rangeEnd, Math.Max(rangeStart, position + len - (backReference.Distance << 2)));
                        }

                        hasher.StoreRange(rangeStart, rangeEnd);
                        position += len;
                    }
                    else{
                        ++insertLength;
                        ++position;

                        if (position > applyRandomHeuristics){
                            int skipStep;
                            int nextStopOffset;

                            if (position > applyRandomHeuristics + (4 * LiteralSpreeLengthForSparseSearch)){
                                skipStep = 4;
                                nextStopOffset = 16;
                            }
                            else{
                                skipStep = 2;
                                nextStopOffset = 8;
                            }

                            int margin = Math.Max(hasher.StoreLookahead - 1, skipStep);
                            int posJump = Math.Min(position + nextStopOffset, posEnd - margin);

                            while(position < posJump){
                                hasher.Store(position);
                                insertLength += skipStep;
                                position += skipStep;
                            }
                        }
                    }
                }

                insertLength += posEnd - position;
                lastInsertLen = insertLength;
            }
        }
    }
}
