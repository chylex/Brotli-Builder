using System;
using BrotliImpl.Utils;
using BrotliLib.Brotli.Dictionary;
using BrotliLib.Brotli.Dictionary.Index;

namespace BrotliImpl.Combined.Hashers.Types{
    /// <summary>
    /// Adapted from https://github.com/google/brotli/blob/master/c/enc/hash_longest_match_quickly_inc.h
    /// </summary>
    abstract class HashLongestMatchQuickly : HasherCommon{
        private const ulong HashMul64 = (0x1E35A7BDUL << 32) | 0x1E35A7BD;

        protected int HashLen => 5;

        public override int StoreLookahead => 8;
        public override int HashTypeLength => 8;

        protected readonly byte[] input;
        protected readonly int[] table;

        protected readonly int bucketBits;
        protected readonly int bucketSize;
        protected readonly int bucketMask;

        private readonly BrotliDictionary? dictionary;
        private int dictionaryMatches;
        private int dictionaryLookups;

        protected HashLongestMatchQuickly(byte[] input, Configure configuration){
            this.bucketBits = configuration.BucketBits ?? throw Configure.Incomplete(nameof(configuration.BucketBits));
            this.bucketSize = 1 << bucketBits;
            this.bucketMask = bucketSize - 1;

            this.dictionary = configuration.Dictionary;

            this.input = input;
            this.table = new int[bucketSize];

            // partial hasher preparation is only triggered for one-shot compression
        }

        public class Configure{
            public int? BucketBits { get; set; } = null;
            public int? SweepBits { get; set; } = null;
            public BrotliDictionary? Dictionary { get; set; } = null;

            public HashLongestMatchQuickly Build(byte[] input){
                if (SweepBits == null || SweepBits == 0){
                    return new NoSweep(input, this);
                }
                else{
                    return new WithSweep(input, this);
                }
            }

            internal static ArgumentException Incomplete(string parameter){
                return new ArgumentException("incomplete hasher configuration (" + parameter + ")");
            }
        }

        // Common

        protected uint HashBytes(byte[] bytes, int ip){
            ulong h = unchecked((Load64LE(bytes, ip) << (64 - 8 * HashLen)) * HashMul64);
            return (uint)(h >> (64 - bucketBits));
        }

        public sealed override void StoreRange(int ipStart, int ipEnd){
            for(int ip = ipStart; ip < ipEnd; ip++){
                Store(ip);
            }
        }

        public sealed override void StitchToPreviousBlock(int chunkLength, int ip){
            if (chunkLength >= HashTypeLength - 1 && ip >= 3){
                Store(ip - 3);
                Store(ip - 2);
                Store(ip - 1);
            }
        }

        public sealed override HasherSearchResult FindLongestMatch(int ip, int maxLength, int maxDistance, int dictionaryStart, int lastDistance, int bestLenIn){
            var result = FindLongestBackReference(ip, maxLength, maxDistance, lastDistance, bestLenIn);

            if (dictionary != null && !result.FoundAnything && dictionaryMatches >= (dictionaryLookups >> 7)){
                DictionaryIndexEntry? bestEntry = null;
                int bestScore = int.MinValue;

                foreach(var entry in dictionary.Index.Find(new ArraySegment<byte>(input, ip, input.Length - ip), minLength: 4, maxLength)){ // TODO
                    int distance = dictionaryStart + entry.Packed;
                    int score = HasherSearchResult.BackwardReferenceScore(entry.OutputLength, distance);

                    // TODO investigate what the "cutoff" does

                    if (score > bestScore){ // TODO check distance to make sure it doesn't go beyond what can be represented?
                        bestEntry = entry;
                        bestScore = score;
                    }
                }

                ++dictionaryLookups;

                if (bestEntry != null){
                    ++dictionaryMatches;
                    return new HasherSearchResult(bestEntry.Value, bestScore);
                }
            }

            return result.Build();
        }

        protected abstract HasherSearchResult.BackReferenceBuilder FindLongestBackReference(int ip, int maxLength, int maxDistance, int lastDistance, int bestLenIn);

        // Types

        private class NoSweep : HashLongestMatchQuickly{
            internal NoSweep(byte[] input, Configure configuration) : base(input, configuration){}

            public override void Store(int ip){
                table[HashBytes(input, ip)] = ip;
            }

            protected override HasherSearchResult.BackReferenceBuilder FindLongestBackReference(int ip, int maxLength, int maxDistance, int lastDistance, int bestLenIn){
                byte compareChar = input[ip + bestLenIn];
                uint key = HashBytes(input, ip);

                var result = new HasherSearchResult.BackReferenceBuilder{
                    Len = bestLenIn
                };

                int bestScore = result.Score;
                int bestLen = result.Len;

                int cachedBackward = lastDistance;
                int prevIp = ip - cachedBackward;
                int len;

                if (prevIp < ip && prevIp >= 0 && compareChar == input[prevIp + bestLen]){
                    len = Match.DetermineLength(input, prevIp, ip, maxLength);

                    if (len >= 4){
                        int score = HasherSearchResult.BackwardReferenceScoreUsingLastDistance(len);

                        if (bestScore < score){
                            result.Len = len;
                            result.Distance = cachedBackward;
                            result.Score = score;

                            table[key] = ip;
                            return result;
                        }
                    }
                }

                prevIp = table[key];
                table[key] = ip;

                int backward = ip - prevIp;

                if (compareChar != input[prevIp + bestLenIn]){
                    return result;
                }

                if (backward == 0 || backward > maxDistance){
                    return result;
                }

                len = Match.DetermineLength(input, prevIp, ip, maxLength);

                if (len >= 4){
                    int score = HasherSearchResult.BackwardReferenceScore(len, backward);

                    if (bestScore < score){
                        result.Len = len;
                        result.Score = score;
                        result.Distance = backward;
                        return result;
                    }
                }

                return result;
            }
        }

        private class WithSweep : HashLongestMatchQuickly{
            private readonly int sweep;
            private readonly int sweepMask;

            public WithSweep(byte[] input, Configure configuration) : base(input, configuration){
                int sweepBits = configuration.SweepBits ?? throw Configure.Incomplete(nameof(configuration.SweepBits));
                this.sweep = 1 << sweepBits;
                this.sweepMask = (sweep - 1) << 3;
            }

            public override void Store(int ip){
                table[(HashBytes(input, ip) + (ip & sweepMask)) & bucketMask] = ip;
            }

            protected override HasherSearchResult.BackReferenceBuilder FindLongestBackReference(int ip, int maxLength, int maxDistance, int lastDistance, int bestLenIn){
                byte compareChar = input[ip + bestLenIn];
                uint key = HashBytes(input, ip);

                var result = new HasherSearchResult.BackReferenceBuilder{
                    Len = bestLenIn
                };

                int bestScore = result.Score;
                int bestLen = result.Len;

                int cachedBackward = lastDistance;
                int prevIp = ip - cachedBackward;

                if (prevIp < ip && prevIp >= 0 && compareChar == input[prevIp + bestLen]){
                    int len = Match.DetermineLength(input, prevIp, ip, maxLength);

                    if (len >= 4){
                        int score = HasherSearchResult.BackwardReferenceScoreUsingLastDistance(len);

                        if (bestScore < score){
                            result.Len = len;
                            result.Distance = cachedBackward;
                            result.Score = score;

                            bestLen = len;
                            bestScore = score;
                            compareChar = input[ip + len];
                        }
                    }
                }

                var keys = new uint[sweep];
                
                for(int i = 0; i < sweep; i++){
                    keys[i] = (uint)((key + (i << 3)) & bucketMask);
                }

                uint keyOut = keys[(ip & sweepMask) >> 3];

                for(int i = 0; i < sweep; i++){
                    prevIp = table[keys[i]];
                    
                    int backward = ip - prevIp;

                    if (compareChar != input[prevIp + bestLen]){
                        continue;
                    }

                    if (backward == 0 || backward > maxDistance){
                        continue;
                    }

                    int len = Match.DetermineLength(input, prevIp, ip, maxLength);

                    if (len >= 4){
                        int score = HasherSearchResult.BackwardReferenceScore(len, backward);

                        if (bestScore < score){
                            result.Len = bestLen = len;
                            result.Score = bestScore = score;
                            result.Distance = backward;

                            compareChar = input[ip + len];
                        }
                    }
                }

                table[keyOut] = ip;
                return result;
            }
        }
    }
}
