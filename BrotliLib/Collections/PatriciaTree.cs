using System;
using System.Collections.Generic;

namespace BrotliLib.Collections{
    /// <summary>
    /// Compact tree structure that maps <code>byte[]</code> keys to one or more values of type <typeparamref name="V"/>.
    /// Each node splits the path at a specific bit into 2 branches. The search ends when a branch leads back into its owner or one of the parents.
    /// </summary>
    public sealed class PatriciaTree<V>{
        /// <summary>
        /// Returns the value of the bit at position <paramref name="bit"/> in the provided <paramref name="key"/>.
        /// </summary>
        private static bool Bit(ArraySegment<byte> key, short bit){
            int index = bit / 8;
            return index < key.Count && (key[index] & (1 << (bit % 8))) != 0;
        }

        private readonly Node root = new Node();

        public void Insert(byte[] key, V value){
            // the tree is traversed in an attempt to find an existing node
            var parentBit = root.bit;
            var found = root.left;

            while(found.bit > parentBit){
                parentBit = found.bit;
                found = Bit(key, found.bit) ? found.right : found.left;
            }

            // if the key matches, the array of values is updated (and it turns out to be a lot faster and more memory-efficient than a List)
            var foundKey = found.key;

            if (CollectionHelper.Equal(key, foundKey)){
                Array.Resize(ref found.values, found.values!.Length + 1);
                found.values[^1] = value;
                return;
            }

            // if the key does not match, the common length (in bits) is found with the found key
            int maxLength = 8 * Math.Max(key.Length, foundKey.Length);
            short commonLength = 0;

            while(commonLength < maxLength && Bit(key, commonLength) == Bit(foundKey, commonLength)){
                ++commonLength;
            }

            // the tree is traversed again to find the node to split
            bool lastPath = false;
            var parent = root;
            var current = parent.left;

            while(true){
                var bit = current.bit;

                if (bit >= commonLength || bit <= parent.bit){
                    break;
                }

                lastPath = Bit(key, bit);

                parent = current;
                current = lastPath ? current.right : current.left;
            }

            // new node containing the inserted key and value, and its paths are setup for insertion above the split node
            var newNode = new Node(commonLength, key, value);

            if (Bit(key, commonLength)){
                newNode.left = current;
                newNode.right = newNode;
            }
            else{
                newNode.left = newNode;
                newNode.right = current;
            }

            // the new node is inserted above the split node
            if (lastPath){
                parent.right = newNode;
            }
            else{
                parent.left = newNode;
            }
        }

        public IReadOnlyList<V> FindLongest(ArraySegment<byte> key){
            var parentBit = root.bit;
            var node = root.left;

            Node best = root; // root has an empty key & null value array

            while(node.bit > parentBit){
                if (node.key.Length > best.key.Length && node.CheckMatch(key)){
                    best = node;
                }

                node = Bit(key, parentBit = node.bit) ? node.right : node.left;
            }

            return best.values ?? Array.Empty<V>();
        }

        private sealed class Node{
            public readonly short bit;
            public Node left;
            public Node right;

            public readonly byte[] key;
            public V[]? values;

            /// <summary>
            /// Constructs the root node of a tree.
            /// </summary>
            public Node(){
                this.bit = -1;
                this.left = this;
                this.right = this;

                this.key = Array.Empty<byte>();
                this.values = null;
            }

            /// <summary>
            /// Constructs a node with the specified bit position, key, and the first value.
            /// </summary>
            public Node(short bit, byte[] key, V value){
                this.bit = bit;
                this.left = this;
                this.right = this;

                this.key = CollectionHelper.Clone(key);
                this.values = new V[]{ value };
            }

            /// <summary>
            /// Returns true if the <see cref="key"/> in this node is a prefix to the provided <paramref name="segment"/>.
            /// </summary>
            public bool CheckMatch(ArraySegment<byte> segment){
                if (segment.Count < key.Length){
                    return false;
                }

                for(int index = 0; index < key.Length; index++){
                    if (segment[index] != key[index]){
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
