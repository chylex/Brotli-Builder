using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace BrotliLib.Collections.Trie{
    public sealed class MultiTrie<K, V> where K : IComparable<K> where V : IEquatable<V>{
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

            private int hashCode = int.MinValue;

            public override bool Equals(object obj){
                if (!(obj is Node node) || HasValues != node.HasValues){
                    return false;
                }

                var children1 = Values;
                var children2 = node.Values;

                if (!((children1 == null && children2 == null) || (children1 != null && children2 != null && children1.SequenceEqual(children2)))){
                    return false;
                }

                var values1 = Values;
                var values2 = node.Values;

                if (!((values1 == null && values2 == null) || (values1 != null && values2 != null && values1.SequenceEqual(values2)))){
                    return false;
                }

                return true;
            }

            [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
            public override int GetHashCode(){
                if (hashCode != int.MinValue){
                    return hashCode;
                }

                const int prime = 31;

                unchecked{
                    int result = 1;

                    if (children != null){
                        foreach(var kvp in children){
                            result = (prime * result) + kvp.Key.GetHashCode();
                            result = (prime * result) + kvp.Value.GetHashCode();
                        }
                    }

                    var values = Values;

                    if (values != null){
                        foreach(var value in values){
                            result = (prime * result) + value.GetHashCode();
                        }
                    }

                    if (result == int.MinValue){
                        ++result;
                    }

                    return hashCode = result;
                }
            }
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
