using System.Collections.Generic;
using BrotliLib.IO;

namespace BrotliLib.Huffman{
    /// <summary>
    /// Represents a node inside a Huffman tree.
    /// </summary>
    /// <typeparam name="T">Type of values contained inside the tree.</typeparam>
    public abstract partial class HuffmanNode<T>{
        protected HuffmanNode(){}

        /// <summary>
        /// Traverses through the tree, consuming the <paramref name="bits"/> reader until it hits a <see cref="Leaf"/> node.
        /// Returns the found value, or <code>default(T)</code> if the <paramref name="bits"/> reader reaches the end.
        /// </summary>
        /// <param name="bits">Bit reader used to determine the <see cref="Path"/> to traverse to retrieve a value.</param>
        public abstract T LookupValue(BitReader bits);

        /// <summary>
        /// Recursively searches through all child nodes, and returns all values mapped to their respective bit sequences.
        /// </summary>
        /// <param name="prefix">Sequence of bits used to get to the current node. Every level of recursion on <see cref="Path"/> nodes appends a 0/1 bit, <see cref="Leaf"/> nodes return the final value.</param>
        protected abstract IEnumerable<KeyValuePair<T, BitStream>> ListValues(BitStream prefix);
        
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
    }
}
