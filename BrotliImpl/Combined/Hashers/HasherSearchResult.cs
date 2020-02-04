using BrotliImpl.Utils;
using BrotliLib.Brotli.Dictionary.Index;
using BrotliLib.Numbers;

namespace BrotliImpl.Combined.Hashers{
    /// <summary>
    /// Adapted from https://github.com/google/brotli/blob/master/c/enc/hash.h
    /// </summary>
    class HasherSearchResult{
        private const int LiteralByteScore = 135;
        private const int DistanceBitPenalty = 30;

        private const int ScoreBase = DistanceBitPenalty * 8 * sizeof(ulong);
        private const int MinScore = ScoreBase + 100;

        public static int BackwardReferenceScore(int copyLength, int backwardReferenceOffset){
            return ScoreBase + (LiteralByteScore * copyLength) - (DistanceBitPenalty * Log2.Floor(backwardReferenceOffset));
        }

        public static int BackwardReferenceScoreUsingLastDistance(int copyLength){
            return ScoreBase + (LiteralByteScore * copyLength) + 15;
        }

        public Copy Copy { get; }
        public int Score { get; }

        public bool FoundAnything => Score > MinScore;

        private HasherSearchResult(BackReferenceBuilder builder){
            this.Copy = new Copy.BackReference(builder.Len, builder.Distance);
            this.Score = builder.Score;
        }

        public HasherSearchResult(DictionaryIndexEntry entry, int score){
            this.Copy = new Copy.Dictionary(entry);
            this.Score = score;
        }

        public class BackReferenceBuilder{
            public int Len { get; set; }
            public int Distance { get; set; }
            public int Score { get; set; } = MinScore;

            public bool FoundAnything => Score > MinScore;

            public HasherSearchResult Build(){
                return new HasherSearchResult(this);
            }
        }
    }
}
