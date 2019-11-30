using System.Collections.Generic;
using BrotliLib.Serialization;
using BrotliLib.Serialization.Reader;

namespace BrotliLib.Collections.Huffman{
    /// <summary>
    /// Represents a node inside a Huffman tree.
    /// </summary>
    /// <typeparam name="T">Type of values contained inside the tree.</typeparam>
    public abstract partial class HuffmanNode<T>{
        /// <summary>
        /// Traverses through the tree, consuming the <paramref name="bits"/> reader until it hits a <see cref="Leaf"/> node.
        /// Returns the found value, or <code>default(T)</code> if the <paramref name="bits"/> reader reaches the end.
        /// </summary>
        /// <param name="bits">Bit reader used to determine the <see cref="Path"/> to traverse to retrieve a value.</param>
        public abstract T LookupValue(IBitReader bits);

        /// <summary>
        /// Recursively searches through all child nodes, and returns all values mapped to their respective bit sequences.
        /// </summary>
        /// <param name="prefix">Sequence of bits used to get to the current node. Every level of recursion on <see cref="Path"/> nodes appends a 0/1 bit, <see cref="Leaf"/> nodes return the final value.</param>
        protected abstract IEnumerable<KeyValuePair<T, BitStream>> ListValues(BitStream prefix);

        /// <summary>
        /// Recursively searches through all child nodes, and returns all values mapped to their respective bit sequences.
        /// </summary>
        /// <param name="prefix">Sequence of bits used to get to the current node. Every level of recursion on <see cref="Path"/> nodes appends a 0/1 bit, <see cref="Leaf"/> nodes return the final value.</param>
        protected abstract IEnumerable<KeyValuePair<T, BitPath>> ListValues(BitPath prefix);
        
        /// <summary>
        /// Generates a map of values to their respective bit sequences.
        /// </summary>
        public Dictionary<T, BitStream> GenerateValueMap(){
            Dictionary<T, BitStream> map = new Dictionary<T, BitStream>();

            foreach(KeyValuePair<T, BitStream> kvp in ListValues(new BitStream())){
                map.Add(kvp.Key, kvp.Value);
            }

            return map;
        }
        
        /// <summary>
        /// Generates a map of values to their respective bit sequences. The map is more memory-efficient than <see cref="GenerateValueMap"/>, but limited by the maximum possible length of <see cref="BitPath"/>.
        /// </summary>
        public Dictionary<T, BitPath> GenerateValueMapOptimized(){
            Dictionary<T, BitPath> map = new Dictionary<T, BitPath>();

            foreach(KeyValuePair<T, BitPath> kvp in ListValues(new BitPath())){
                map.Add(kvp.Key, kvp.Value);
            }

            return map;
        }
    }
}
