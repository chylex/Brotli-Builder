using System;
using System.Collections.Generic;
using System.Linq;

namespace BrotliLib.Collections{
    public sealed class MultiTrie<K, V> : MultiTrieBase<K, V, MultiTrie<K, V>.NodeRef> where K : IComparable<K>{
        internal ReadOnlyNode Root => RootNodeIdentifier.AsReadOnly();

        private protected override NodeRef RootNodeIdentifier { get; }

        private MultiTrie(NodeRef rootNode){
            this.RootNodeIdentifier = rootNode;
        }

        private protected override Node ReachNode(NodeRef identifier){
            return identifier.node;
        }

        // TODO everything about this is utter hell

        public sealed class NodeRef{
            internal readonly Node node;

            internal NodeRef(Node node){
                this.node = node;
            }

            internal ReadOnlyNode AsReadOnly(){
                var clonedChildren = node.children?.Select(kvp => new KeyValuePair<K, ReadOnlyNode>(kvp.Key, kvp.Value.AsReadOnly())).ToArray();
                var clonedValues = (V[])node.values?.Clone();

                return new ReadOnlyNode(clonedChildren, clonedValues);
            }
        }

        internal sealed class ReadOnlyNode{
            public IReadOnlyList<KeyValuePair<K, ReadOnlyNode>> Children { get; }
            public IReadOnlyList<V> Values { get; }

            public ReadOnlyNode(IReadOnlyList<KeyValuePair<K, ReadOnlyNode>> children, IReadOnlyList<V> values){
                Children = children;
                Values = values;
            }
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

                public NodeRef Build(){
                    var node = new Node();

                    if (children != null){
                        var sorted = new KeyValuePair<K, NodeRef>[children.Count];
                        int index = 0;
                        
                        foreach(var kvp in children){
                            sorted[index++] = new KeyValuePair<K, NodeRef>(kvp.Key, kvp.Value.Build());
                        }
                        
                        Array.Sort(sorted, KeyComparer);
                        node.children = sorted;
                    }

                    if (values != null){
                        Array.Sort(values);
                        node.values = values;
                    }

                    return new NodeRef(node);
                }
            }
        }
    }
}
