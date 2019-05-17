using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BrotliLib.Brotli.Dictionary.Format;
using BrotliLib.Collections.Trie;
using DictTrie = BrotliLib.Collections.Trie.MultiTrie<byte, BrotliLib.Brotli.Dictionary.Index.DictionaryIndexEntry>;
using DictTrieNode = BrotliLib.Collections.Trie.MultiTrie<byte, BrotliLib.Brotli.Dictionary.Index.DictionaryIndexEntry>.ReadOnlyNode;

namespace BrotliLib.Brotli.Dictionary.Index{
    public sealed class BrotliDictionaryIndex : MultiTrieBase<byte, DictionaryIndexEntry, int>, IDisposable{
        private readonly Stream stream;
        private readonly DictionaryIndexHelper.Reader reader;
        private readonly int lengthBits;

        public BrotliDictionaryIndex(IDictionaryFormat format, Stream stream){
            this.stream = stream;
            this.reader = new DictionaryIndexHelper.Reader(stream, Encoding.ASCII, false);
            this.lengthBits = DictionaryIndexHelper.CalculateWordLengthBits(format);
        }

        public void Dispose(){
            reader.Dispose();
        }

        // Nodes

        private protected override int RootNodeIdentifier => 0;

        private protected override Node ReachNode(int position){
            stream.Position = position;
            return ReadNextNode();
        }

        private Node ReadNextNode(){
            return new Node{
                children = reader.ReadArray(ReadNextChildEntry),
                values = reader.ReadArray(ReadNextIndexEntry)
            };
        }

        private KeyValuePair<byte, int> ReadNextChildEntry(){
            return new KeyValuePair<byte, int>(reader.ReadByte(), reader.ReadInt());
        }

        private DictionaryIndexEntry ReadNextIndexEntry(){
            int value = reader.ReadVarInt();
            return new DictionaryIndexEntry(value & ((1 << lengthBits) - 1), value >> lengthBits);
        }

        // Writing

        public static void Write(IDictionaryFormat format, Stream stream, DictTrie trie){
            int lengthBits = DictionaryIndexHelper.CalculateWordLengthBits(format);

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

                    writer.WriteArray(values, value => {
                        var (length, packed) = value;
                        writer.WriteVarInt((packed * (1 << lengthBits)) | length); // use arithmetic to check for overflow
                    });
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
