using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli.Markers.Data;
using BrotliLib.Huffman;
using BrotliLib.IO;
using BrotliLib.Markers;

namespace BrotliLib.Brotli.Markers{
    class MarkedBitReader : BitReader.Wrapped{
        public MarkerRoot MarkerRoot { get; } = new MarkerRoot();
        
        private readonly Stack<MarkerNode> nodes = new Stack<MarkerNode>();
        private readonly Stack<int> starts = new Stack<int>();
        
        public MarkedBitReader(BitReader wrapped) : base(wrapped){}

        // General

        public void MarkStart(){
            MarkerNode added = new MarkerNode{ Depth = nodes.Count };

            if (nodes.Count == 0){
                MarkerRoot.AddChild(added);
            }
            else{
                nodes.Peek().AddChildOrSibling(added);
            }

            starts.Push(Index);
            nodes.Push(added);
        }

        public void MarkEnd(IMarkerInfo info){
            int start = starts.Pop();
            int end = Index;
            nodes.Pop().Marker = new Marker(start, end, info);
        }

        public void MarkOne(IMarkerInfo info){
            MarkStart();
            MarkEnd(info);
        }

        // Marking helpers

        private T MarkCall<T>(Func<T> supplier, Func<T, IMarkerInfo> marker){
            MarkStart();
            T result = supplier();
            MarkEnd(marker(result));
            return result;
        }

        public T MarkTitle<T>(string title, Func<T> action){
            return MarkCall(action, _ => new TitleMarker(title));
        }

        public T MarkValue<T>(string title, Func<T> action){
            return MarkCall(action, result => new TextMarker(title, result));
        }

        // Marking overloads
        
        public bool NextBit(string name){
            return MarkValue(name, NextBit);
        }

        public int NextChunk(int bits, string name){
            return MarkValue(name, () => NextChunk(bits));
        }

        public T NextChunk<T>(int bits, string name, Func<int, T> mapper){
            return MarkValue(name, () => mapper(NextChunk(bits)));
        }

        public new void AlignToByteBoundary(){
            MarkStart();
            base.AlignToByteBoundary();
            MarkEnd(new TextMarker("byte boundary alignment"));
        }

        public byte NextAlignedByte(string name){
            return MarkValue(name, base.NextAlignedByte);
        }

        // Utilities

        public O ReadValue<T, C, O>(IBitSerializer<T, C> serializer, C context, string name, Func<T, O> mapper){
            return MarkCall(() => mapper(serializer.FromBits(this, context)), result => new TextMarker(name, result));
        }

        public T ReadValue<T, C>(IBitSerializer<T, C> serializer, C context, string name){
            return ReadValue(serializer, context, name, result => result);
        }

        public O ReadValue<T, O>(HuffmanNode<T> tree, string name, Func<T, O> mapper) where T : IComparable<T>{
            return MarkCall(() => mapper(tree.LookupValue(this)), result => new TextMarker(name, result));
        }

        public T ReadValue<T>(HuffmanNode<T> tree, string name) where T : IComparable<T>{
            return ReadValue(tree, name, result => result);
        }

        public T ReadStructure<T, C>(IBitSerializer<T, C> serializer, C context, string title){
            return MarkCall(() => serializer.FromBits(this, context), _ => new TitleMarker(title));
        }

        public T[] ReadValueArray<T>(int length, string name, Func<T> supplier){
            return Enumerable.Range(0, length)
                             .Select(counter => MarkValue(name + " " + (counter + 1) + "/" + length, supplier))
                             .ToArray();
        }

        public T[] ReadStructureArray<T, C>(int length, IBitSerializer<T, C> serializer, C context, string title){
            return Enumerable.Range(0, length)
                             .Select(counter => MarkTitle(title + " " + (counter + 1) + "/" + length, () => serializer.FromBits(this, context)))
                             .ToArray();
        }
    }
}
