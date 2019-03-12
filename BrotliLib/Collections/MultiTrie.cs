using System;
using System.Collections.Generic;
using System.Linq;

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
                    var children = node.children ?? (node.children = new SortedList<K, MutableNode>(1));

                    if (!children.TryGetValue(ele, out node)){
                        node = children[ele] = new MutableNode();
                    }
                }

                (node.values ?? (node.values = new List<V>(1))).Add(value);
            }

            public MultiTrie<K, V> Build(){
                return new MultiTrie<K, V>(rootNode.Build());
            }

            private sealed class MutableNode{
                public SortedList<K, MutableNode> children;
                public List<V> values;

                public Node Build(){
                    return new Node{
                        children = this.children?.Select(kvp => new KeyValuePair<K, Node>(kvp.Key, kvp.Value.Build())).ToArray(),
                        values = this.values?.ToArray()
                    };
                }
            }
        }
    }
}
