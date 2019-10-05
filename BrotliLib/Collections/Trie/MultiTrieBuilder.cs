using System;
using System.Collections.Generic;

namespace BrotliLib.Collections.Trie{
    sealed class MultiTrieBuilder<K, V> where K : IComparable<K> where V : IEquatable<V>{
        private static readonly KeyComparer<K, MutableNode> KeyComparer = new KeyComparer<K, MutableNode>();

        private MutableNode rootNode = new MutableNode();

        public void Insert(IEnumerable<K> key, V value){
            MutableNode node = rootNode;

            foreach(K ele in key){
                var children = node.children;

                if (children == null){
                    node.children = new List<KeyValuePair<K, MutableNode>>(4){
                        new KeyValuePair<K, MutableNode>(ele, node = new MutableNode())
                    };
                }
                else{
                    int index = children.BinarySearch(new KeyValuePair<K, MutableNode>(ele, null), KeyComparer);

                    if (index >= 0){
                        node = children[index].Value;
                    }
                    else{
                        children.Insert(~index, new KeyValuePair<K, MutableNode>(ele, node = new MutableNode()));
                    }
                }
            }
            
            node.AddValue(value);
        }

        public MultiTrie<K, V> Build(MultiTrieCache<K, V> cache = null){
            var result = new MultiTrie<K, V>(rootNode.Build(cache ?? new MultiTrieCache<K, V>()));
            rootNode = null; // prevent accessing the builder again
            return result;
        }

        private sealed class MutableNode{
            public List<KeyValuePair<K, MutableNode>> children;
            private V[] values;

            public void AddValue(V value){
                if (values == null){
                    values = new V[]{ value };
                }
                else{
                    Array.Resize(ref values, values.Length + 1);
                    values[values.Length - 1] = value;
                }
            }

            public MultiTrie<K, V>.Node Build(MultiTrieCache<K, V> cache){
                MultiTrie<K, V>.Node node;

                if (values == null){
                    node = new MultiTrie<K, V>.Node();
                }
                else if (values.Length == 1){
                    node = new MultiTrie<K, V>.NodeWithValue{ value = this.values[0] };
                }
                else{
                    node = new MultiTrie<K, V>.NodeWithValues{ values = this.values };
                }

                if (children != null){
                    var copy = new KeyValuePair<K, MultiTrie<K, V>.Node>[children.Count];

                    for(int index = 0, count = children.Count; index < count; index++){
                        var kvp = children[index];
                        copy[index] = new KeyValuePair<K, MultiTrie<K, V>.Node>(kvp.Key, kvp.Value.Build(cache));
                    }
                    
                    node.children = copy;
                }

                return cache.Cache(node);
            }
        }
    }
}
