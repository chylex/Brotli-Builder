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
        /// <summary>
        /// Constructs and initializes an array of empty <see cref="FrequencyList{T}"/> objects.
        /// </summary>
        public static FrequencyList<T>[] Array(int size){
            var array = new FrequencyList<T>[size];

            for(int index = 0; index < array.Length; index++){
                array[index] = new FrequencyList<T>();
            }

            return array;
        }

        /// <summary>
        /// Constructs a <see cref="FrequencyList{T}"/> and immediately counts all symbols in the <paramref name="sequence"/>.
        /// </summary>
        public static FrequencyList<T> FromSequence(List<T> sequence){
            return new FrequencyList<T>{ sequence };
        }

        // Instance

        public int Count => frequencies.Count;
        public int Sum => frequencies.Sum(kvp => kvp.Value);

        public IList<HuffmanGenerator<T>.SymbolFreq> HuffmanFreq{
            get => frequencies.Select(kvp => new HuffmanGenerator<T>.SymbolFreq(kvp.Key, kvp.Value)).ToArray();
        }

        public int this[T symbol]{
            get{
                return frequencies.TryGetValue(symbol, out int count) ? count : 0;
            }
            set{
                if (value < 0){
                    throw new ArgumentOutOfRangeException(nameof(value), "Symbol cannot have a negative frequency.");
                }
                else if (value == 0){
                    frequencies.Remove(symbol);
                }
                else{
                    frequencies[symbol] = value;
                }
            }
        }

        private readonly Dictionary<T, int> frequencies = new Dictionary<T, int>();

        public bool Contains(T symbol){
            return frequencies.ContainsKey(symbol);
        }

        public void Clear(){
            frequencies.Clear();
        }

        public void Add(T symbol){
            if (frequencies.ContainsKey(symbol)){
                ++frequencies[symbol];
            }
            else{
                frequencies[symbol] = 1;
            }
        }

        public void Add(List<T> sequence){
            foreach(T symbol in sequence){
                Add(symbol);
            }
        }

        public void Add(FrequencyList<T> other){
            foreach(var (symbol, freq) in other.frequencies){
                if (frequencies.ContainsKey(symbol)){
                    frequencies[symbol] += freq;
                }
                else{
                    frequencies[symbol] = freq;
                }
            }
        }

        public IEnumerator<T> GetEnumerator() => frequencies.Keys.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
