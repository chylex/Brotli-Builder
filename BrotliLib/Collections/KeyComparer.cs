using System;
using System.Collections.Generic;

namespace BrotliLib.Collections{
    internal sealed class KeyComparer<K, V> : IComparer<KeyValuePair<K, V>> where K : IComparable<K>{
        public int Compare(KeyValuePair<K, V> x, KeyValuePair<K, V> y){
            return x.Key.CompareTo(y.Key);
        }
    }
}
