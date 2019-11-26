using System;
using System.Collections.Generic;

namespace BrotliLib.Collections.Trie{
    sealed class MultiTrieBuilder<K, V> where K : IComparable<K> where V : IEquatable<V>{
        private static readonly TupleKeyComparer<K, MutableNode> KeyComparer = new TupleKeyComparer<K, MutableNode>();

        private MutableNode? rootNode = new MutableNode();

        public void Insert(IEnumerable<K> key, V value){
            MutableNode node = rootNode!;

            foreach(K ele in key){
                var children = node.children;

                if (children == null){
                    node.children = new List<(K, MutableNode)>(4){
                        (ele, node = new MutableNode())
                    };
                }
                else{
                    int index = children.BinarySearch((ele, null!), KeyComparer);

                    if (index >= 0){
                        node = children[index].child;
                    }
                    else{
                        children.Insert(~index, (ele, node = new MutableNode()));
                    }
                }
            }
            
            node.AddValue(value);
        }

        public MultiTrie<K, V> Build(MultiTrieCache<K, V>? cache = null){
            var result = new MultiTrie<K, V>(rootNode!.Build(cache ?? new MultiTrieCache<K, V>()));
            rootNode = null; // prevent accessing the builder again
            return result;
        }

        private sealed class MutableNode{
            public List<(K key, MutableNode child)>? children;
            private V[]? values;

            public void AddValue(V value){
                if (values == null){
                    values = new V[]{ value };
                }
                else{
                    Array.Resize(ref values, values.Length + 1);
                    values[^1] = value;
                }
            }

            public MultiTrie<K, V>.Node Build(MultiTrieCache<K, V> cache){
                MultiTrie<K, V>.Node node;

                if (values == null){
                    node = new MultiTrie<K, V>.Node();
                }
                else if (values.Length == 1){
                    node = new MultiTrie<K, V>.NodeWithValue(this.values[0]);
                }
                else{
                    node = new MultiTrie<K, V>.NodeWithValues(this.values);
                }

                if (children != null){
                    var copy = new (K, MultiTrie<K, V>.Node)[children.Count];

                    for(int index = 0, count = children.Count; index < count; index++){
                        var (key, child) = children[index];
                        copy[index] = (key, child.Build(cache));
                    }
                    
                    node.children = copy;
                }

                return cache.Cache(node);
            }
        }
    }
}
