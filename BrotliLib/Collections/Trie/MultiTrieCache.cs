using System;
using System.Collections.Generic;

namespace BrotliLib.Collections.Trie{
    sealed class MultiTrieCache<K, V> where K : IComparable<K> where V : IEquatable<V>{
        private readonly Dictionary<MultiTrie<K, V>.Node, MultiTrie<K, V>.Node> nodes = new Dictionary<MultiTrie<K, V>.Node, MultiTrie<K, V>.Node>();

        public MultiTrie<K, V>.Node Cache(MultiTrie<K, V>.Node node){
            if (nodes.TryGetValue(node, out var cached)){
                return cached;
            }
            else{
                return nodes[node] = node;
            }
        }
    }
}
