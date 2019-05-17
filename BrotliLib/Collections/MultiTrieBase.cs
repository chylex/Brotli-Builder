using System;
using System.Collections.Generic;

namespace BrotliLib.Collections{
    public abstract class MultiTrieBase<K, V, N> where K : IComparable<K>{
        private protected static readonly KeyComparer<K, N> KeyComparer = new KeyComparer<K, N>();

        private protected abstract N RootNodeIdentifier { get; }

        public IReadOnlyList<V> Find(IEnumerable<K> key){
            Node node = ReachNode(RootNodeIdentifier);

            foreach(K ele in key){
                var children = node.children;
                int index = children == null ? -1 : Array.BinarySearch(children, new KeyValuePair<K, N>(ele, default), KeyComparer);

                if (index < 0){
                    return new V[0];
                }

                node = ReachNode(children[index].Value);
            }

            return node.values;
        }

        private protected abstract Node ReachNode(N identifier);

        internal sealed class Node{
            public KeyValuePair<K, N>[] children;
            public V[] values;
        }
    }
}
