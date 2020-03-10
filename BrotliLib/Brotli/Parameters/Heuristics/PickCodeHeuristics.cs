using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Collections;

namespace BrotliLib.Brotli.Parameters.Heuristics{
    public static class PickCodeHeuristics<T> where T : class, IComparable<T>{
        public delegate T Picker(List<T> picks, FrequencyList<T> previouslySeen);

        public static Picker PickFirstOption    { get; } = (picks, previouslySeen) => picks[0];
        public static Picker PickPreviouslySeen { get; } = (picks, previouslySeen) => picks.FirstOrDefault(previouslySeen.Contains) ?? picks[0];
    }
}
