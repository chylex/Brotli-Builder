using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Collections.Huffman;

namespace BrotliLib.Collections{
    /// <summary>
    /// Collection that counts the amount of symbol occurrences.
    /// </summary>
    public sealed class FrequencyList<T> : IReadOnlyCollection<T> where T : IComparable<T>{
        public int Count => frequencies.Count;

        public int this[T symbol]{
            get => frequencies.TryGetValue(symbol, out int count) ? count : 0;
            set => frequencies[symbol] = value;
        }

        public IList<HuffmanGenerator<T>.SymbolFreq> HuffmanFreq{
            get{
                return frequencies.Select(kvp => new HuffmanGenerator<T>.SymbolFreq(kvp.Key, kvp.Value)).ToArray();
            }
        }

        private readonly Dictionary<T, int> frequencies = new Dictionary<T, int>();

        public FrequencyList(){}

        public FrequencyList(List<T> source){
            foreach(T symbol in source){
                Add(symbol);
            }
        }

        public bool Contains(T symbol){
            return frequencies.ContainsKey(symbol);
        }

        public void Add(T symbol){
            if (frequencies.ContainsKey(symbol)){
                ++frequencies[symbol];
            }
            else{
                frequencies[symbol] = 1;
            }
        }

        public IEnumerator<T> GetEnumerator() => frequencies.Keys.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
