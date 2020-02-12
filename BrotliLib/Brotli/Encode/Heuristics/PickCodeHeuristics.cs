using System;
using System.Linq;
using static BrotliLib.Brotli.Parameters.BrotliCompressionParameters;

namespace BrotliLib.Brotli.Encode.Heuristics{
    public static class PickCodeHeuristics<T> where T : IComparable<T>{
        public static PickCode<T> PickFirstOption    { get; } = (picks, previouslySeen) => picks[0];
        public static PickCode<T> PickPreviouslySeen { get; } = (picks, previouslySeen) => picks.FirstOrDefault(previouslySeen.Contains) ?? picks[0];
    }
}
