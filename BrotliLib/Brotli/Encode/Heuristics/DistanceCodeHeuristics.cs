using System.Linq;
using static BrotliLib.Brotli.Parameters.BrotliCompressionParameters;

namespace BrotliLib.Brotli.Encode.Heuristics{
    public static class DistanceCodeHeuristics{
        public static PickDistanceCode PickFirstOption    = (picks, previouslySeen) => picks[0];
        public static PickDistanceCode PickPreviouslySeen = (picks, previouslySeen) => picks.FirstOrDefault(previouslySeen.Contains) ?? picks[0];
    }
}
