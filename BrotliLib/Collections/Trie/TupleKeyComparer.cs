using System;
using System.Collections.Generic;

namespace BrotliLib.Collections.Trie{
    internal sealed class TupleKeyComparer<K, V> : IComparer<ValueTuple<K, V>> where K : IComparable<K>{
        public int Compare(ValueTuple<K, V> x, ValueTuple<K, V> y){
            return x.Item1.CompareTo(y.Item1);
        }
    }
}
