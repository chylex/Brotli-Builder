using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BrotliLib.Collections.Trie{
    public sealed class MultiTrie<K, V> where K : IComparable<K> where V : IEquatable<V>{
        private static readonly TupleKeyComparer<K, Node?> KeyComparer = new TupleKeyComparer<K, Node?>();

        private readonly Node rootNode;

        internal MultiTrie(Node rootNode){
            this.rootNode = rootNode;
        }

        public IReadOnlyList<V> FindLongest(IEnumerable<K> key){
            Node node = rootNode;
            Node? last = null;

            foreach(K ele in key){
                var children = node.children;
                int index = children == null ? -1 : Array.BinarySearch(children!, (ele, default), KeyComparer);

                if (index < 0){
                    break;
                }

                // ReSharper disable once PossibleNullReferenceException
                node = children![index].child;

                if (node.HasValues){
                    last = node;
                }
            }

            return last?.Values ?? Array.Empty<V>();
        }

        internal class Node{
            public (K key, Node child)[]? children;

            public virtual bool HasValues => false;
            public virtual V[]? Values => null;

            private int hashCode = int.MinValue;

            public override bool Equals(object obj){
                if (!(obj is Node node) || HasValues != node.HasValues){
                    return false;
                }

                var children1 = children;
                var children2 = node.children;

                if (!CollectionHelper.Equal(children1, children2)){
                    return false;
                }

                var values1 = Values;
                var values2 = node.Values;

                if (!CollectionHelper.Equal(values1, values2)){
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
                        foreach(var (key, child) in children){
                            result = (prime * result) + key.GetHashCode();
                            result = (prime * result) + child.GetHashCode();
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
            private readonly V value;

            public NodeWithValue(V value){
                this.value = value;
            }

            public override bool HasValues => true;
            public override V[]? Values => new V[]{ value };
        }

        internal sealed class NodeWithValues : Node{
            private readonly V[] values;

            public NodeWithValues(V[] values){
                this.values = values;
            }

            public override bool HasValues => true;
            public override V[]? Values => values;
        }
    }
}
