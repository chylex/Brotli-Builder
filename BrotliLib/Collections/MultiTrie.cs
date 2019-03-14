using System;
using System.Collections.Generic;

namespace BrotliLib.Collections{
    public sealed class MultiTrie<K, V> where K : IComparable<K>{
        private static readonly KeyComparer<K, Node> KeyComparer = new KeyComparer<K, Node>();

        private readonly Node rootNode;

        private MultiTrie(Node rootNode){
            this.rootNode = rootNode;
        }

        public IReadOnlyList<V> Find(IEnumerable<K> key){
            Node node = rootNode;

            foreach(K ele in key){
                var children = node.children;
                int index = children == null ? -1 : Array.BinarySearch(children, new KeyValuePair<K, Node>(ele, null), KeyComparer);

                if (index < 0){
                    return new V[0];
                }

                node = children[index].Value;
            }

            return node.values;
        }

        private sealed class Node{
            public KeyValuePair<K, Node>[] children;
            public V[] values;
        }

        public sealed class Builder{
            private readonly MutableNode rootNode = new MutableNode();

            public void Insert(IEnumerable<K> key, V value){
                MutableNode node = rootNode;

                foreach(K ele in key){
                    var children = node.children ?? (node.children = new Dictionary<K, MutableNode>(1));

                    if (!children.TryGetValue(ele, out node)){
                        node = children[ele] = new MutableNode();
                    }
                }
                
                node.AddValue(value);
            }

            public MultiTrie<K, V> Build(){
                return new MultiTrie<K, V>(rootNode.Build());
            }

            private sealed class MutableNode{
                public Dictionary<K, MutableNode> children;
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

                public Node Build(){
                    var node = new Node{ values = this.values };
                    
                    if (children != null){
                        var sorted = new KeyValuePair<K, Node>[children.Count];
                        int index = 0;
                        
                        foreach(var kvp in children){
                            sorted[index++] = new KeyValuePair<K, Node>(kvp.Key, kvp.Value.Build());
                        }
                        
                        Array.Sort(sorted, KeyComparer);
                        node.children = sorted;
                    }

                    return node;
                }
            }
        }
    }
}
