using System;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Collections;

namespace BrotliLib.Brotli.Parameters.Heuristics{
    public static class HuffmanTreeHeuristics{
        public delegate HuffmanTree<T> Generate<T>(FrequencyList<T> frequencies) where T : IComparable<T>;
        public delegate HuffmanTree<T> GenerateLimited<T>(FrequencyList<T> frequencies, byte maxDepth) where T : IComparable<T>;
        public delegate HuffmanTreeLengthCode.RunResolution DecideRuns(HuffmanTreeLengthCode.RunDecider decider);

        public static class RLE{
            public static DecideRuns Disable { get; } = decider => decider.Resolve(run => run.Reject());
            public static DecideRuns KeepAll { get; } = decider => decider.Resolve(run => run.Accept());

            public static DecideRuns SplitOneAboveBoundary { get; } = decider => decider.Resolve(run => {
                int multiplier = 1 << (run.Symbol == 0 ? HuffmanTreeLengthCode.SkipCodeExtraBits : HuffmanTreeLengthCode.RepeatCodeExtraBits);

                if (run.Length - HuffmanTreeLengthCode.Run.MinSpecialCodeLength == multiplier){
                    // when the amount of repetitions equals the first value that requires a second repetition code to encode,
                    // it's more efficient to write it as 1 literal code and 1 repetition code
                    // TODO official compressor (and this) only works for the first value that crosses the boundary... potential point for improvement?
                    return run.Split(run.Length - 1);
                }

                return run.Accept();
            });

            /// <summary>
            /// Adapted from https://github.com/google/brotli/blob/master/c/enc/entropy_encode.c (BrotliWriteHuffmanTree, DecideOverRleUse, BrotliWriteHuffmanTreeRepetitions[Zeros]).
            /// </summary>
            public static DecideRuns OfficialHeuristic { get; } = decider => {
                if (decider.AlphabetSize.SymbolCount <= 50){
                    return decider.Resolve(run => run.Reject());
                }

                bool DecideOverCode(Func<byte, int, bool> countSymbolReps){
                    int totalReps = 0;
                    int countReps = 1;

                    for(int index = 0; index < decider.TrimmedSymbolCount;){
                        byte bits = decider.GetSymbolLength(index);
                        int reps = 1;

                        for(int i2 = index + 1; i2 < decider.TrimmedSymbolCount && decider.GetSymbolLength(i2) == bits; i2++){
                            ++reps;
                        }

                        if (countSymbolReps(bits, reps)){
                            totalReps += reps;
                            countReps += 1;
                        }

                        index += reps;
                    }

                    return totalReps > (countReps * 2);
                }

                bool useSkipCodes   = DecideOverCode((bits, reps) => bits == 0 && reps >= 3);
                bool useRepeatCodes = DecideOverCode((bits, reps) => bits != 0 && reps >= 4);

                return decider.Resolve(run => {
                    if (run.Symbol == 0 && useSkipCodes){
                        return run.Length == 11 ? run.Split(10) : run.Accept();
                    }
                    else if (run.Symbol != 0 && useRepeatCodes){
                        return run.Length == 7 ? run.Split(6) : run.Accept();
                    }
                    else{
                        return run.Reject();
                    }
                });
            };
        }
    }
}
