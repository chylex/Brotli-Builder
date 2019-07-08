using System;
using System.Linq;
using BrotliLib.Brotli.Markers.Data;
using BrotliLib.Brotli.Markers.Reader;
using BrotliLib.Huffman;
using BrotliLib.IO;

namespace BrotliLib.Brotli.Markers{
    static class MarkedBitExtensions{
        public static T MarkTitle<T>(this IMarkedBitReader reader, string title, Func<T> action){
            return reader.MarkCall(action, _ => new TitleMarker(title));
        }

        public static T MarkValue<T>(this IMarkedBitReader reader, string title, Func<T> action){
            return reader.MarkCall(action, result => new ValueMarker(title, result));
        }

        // Marking overloads

        public static bool NextBit(this IMarkedBitReader reader, string name){
            return reader.MarkValue(name, reader.NextBit);
        }

        public static int NextChunk(this IMarkedBitReader reader, int bits, string name){
            return reader.MarkValue(name, () => reader.NextChunk(bits));
        }

        public static T NextChunk<T>(this IMarkedBitReader reader, int bits, string name, Func<int, T> mapper){
            return reader.MarkValue(name, () => mapper(reader.NextChunk(bits)));
        }

        public static byte NextAlignedByte(this IMarkedBitReader reader, string name){
            return reader.MarkValue(name, reader.NextAlignedByte);
        }

        // Complex structures

        public static O ReadValue<T, C, O>(this IMarkedBitReader reader, BitDeserializer<T, C> deserialize, C context, string name, Func<T, O> mapper){
            return reader.MarkCall(() => mapper(deserialize(reader, context)), result => new ValueMarker(name, result));
        }

        public static T ReadValue<T, C>(this IMarkedBitReader reader, BitDeserializer<T, C> deserialize, C context, string name){
            return reader.ReadValue(deserialize, context, name, result => result);
        }

        public static O ReadValue<T, O>(this IMarkedBitReader reader, HuffmanNode<T> tree, string name, Func<T, O> mapper) where T : IComparable<T>{
            return reader.MarkCall(() => mapper(tree.LookupValue(reader)), result => new ValueMarker(name, result));
        }

        public static T ReadValue<T>(this IMarkedBitReader reader, HuffmanNode<T> tree, string name) where T : IComparable<T>{
            return reader.ReadValue(tree, name, result => result);
        }

        public static T ReadStructure<T, C>(this IMarkedBitReader reader, BitDeserializer<T, C> deserialize, C context, string title){
            return reader.MarkCall(() => deserialize(reader, context), _ => new TitleMarker(title));
        }

        public static T[] ReadValueArray<T>(this IMarkedBitReader reader, int length, string name, Func<T> supplier){
            return Enumerable.Range(0, length)
                             .Select(counter => reader.MarkValue(name + " " + (counter + 1) + "/" + length, supplier))
                             .ToArray();
        }

        public static T[] ReadStructureArray<T, C>(this IMarkedBitReader reader, int length, BitDeserializer<T, C> deserialize, C context, string title){
            return Enumerable.Range(0, length)
                             .Select(counter => reader.MarkTitle(title + " " + (counter + 1) + "/" + length, () => deserialize(reader, context)))
                             .ToArray();
        }
    }
}
