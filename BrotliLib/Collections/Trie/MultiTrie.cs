using System;
using System.Collections.Generic;

namespace BrotliLib.Collections.Trie{
    public sealed class MultiTrie<K, V> where K : IComparable<K>{
        private static readonly KeyComparer<K, Node> KeyComparer = new KeyComparer<K, Node>();

        private readonly Node rootNode;

        internal MultiTrie(Node rootNode){
            this.rootNode = rootNode;
        }

        public IReadOnlyList<V> FindLongest(IEnumerable<K> key){
            Node node = rootNode;
            Node last = null;

            foreach(K ele in key){
                var children = node.children;
                int index = children == null ? -1 : Array.BinarySearch(children, new KeyValuePair<K, Node>(ele, default), KeyComparer);

                if (index < 0){
                    break;
                }

                node = children[index].Value;

                if (node.HasValues){
                    last = node;
                }
            }

            return last?.Values ?? new V[0];
        }

        internal class Node{
            public KeyValuePair<K, Node>[] children;

            public virtual bool HasValues => false;
            public virtual V[] Values => null;
        }

        internal sealed class NodeWithValue : Node{
            public V value;

            public override bool HasValues => true;
            public override V[] Values => new V[]{ value };
        }

        internal sealed class NodeWithValues : Node{
            public V[] values;

            public override bool HasValues => true;
            public override V[] Values => values;
        }
    }
}
