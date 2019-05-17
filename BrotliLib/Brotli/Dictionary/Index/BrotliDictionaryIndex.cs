using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BrotliLib.Collections.Trie;
using DictTrie = BrotliLib.Collections.MultiTrie<byte, int>;
using DictTrieNode = BrotliLib.Collections.MultiTrie<byte, int>.ReadOnlyNode;

namespace BrotliLib.Brotli.Dictionary.Index{
    public sealed class BrotliDictionaryIndex : MultiTrieBase<byte, int, int>, IDisposable{
        private readonly Stream stream;
        private readonly DictionaryIndexHelper.Reader reader;

        public BrotliDictionaryIndex(Stream stream){
            this.stream = stream;
            this.reader = new DictionaryIndexHelper.Reader(stream, Encoding.ASCII, false);
        }

        public void Dispose(){
            reader.Dispose();
        }

        // Nodes

        private protected override int RootNodeIdentifier => 0;

        private protected override Node ReachNode(int position){
            stream.Position = position;
            return ReadNodeAtCurrentPosition();
        }

        private Node ReadNodeAtCurrentPosition(){
            return new Node{
                children = reader.ReadArray(() => new KeyValuePair<byte, int>(reader.ReadByte(), reader.ReadInt())),
                values = reader.ReadArray(reader.ReadVarInt)
            };
        }

        // Writing

        public static void Write(Stream stream, DictTrie trie){
            using(var writer = new DictionaryIndexHelper.Writer(stream, Encoding.ASCII, true)){
                var remainingNodes = new Stack<DictTrieNode>();

                var nodePositions = new Dictionary<DictTrieNode, int>();
                var referencePositions = new Dictionary<int, DictTrieNode>();

                void WriteNode(DictTrieNode node){
                    var children = node.Children;
                    var values = node.Values;

                    nodePositions.Add(node, writer.Position);

                    writer.WriteArray(children, child => {
                        writer.WriteByte(child.Key);

                        remainingNodes.Push(child.Value);
                        referencePositions.Add(writer.Position, child.Value);
                        writer.WriteInt(0);
                    });

                    writer.WriteArray(values, writer.WriteVarInt);
                }

                stream.Position = 0;
                WriteNode(trie.Root);

                while(remainingNodes.Count > 0){
                    WriteNode(remainingNodes.Pop());
                }

                foreach(var kvp in referencePositions){
                    stream.Position = kvp.Key;
                    writer.WriteInt(nodePositions[kvp.Value]);
                }
            }
        }
    }
}
