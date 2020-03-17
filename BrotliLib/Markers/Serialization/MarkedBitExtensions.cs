using System;
using System.Linq;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Collections.Huffman;
using BrotliLib.Markers.Serialization.Reader;
using BrotliLib.Serialization;

namespace BrotliLib.Markers.Serialization{
    static class MarkedBitExtensions{
        public static T MarkTitle<T>(this IMarkedBitReader reader, string title, Func<T> action){
            reader.MarkStart();

            T result = action();

            reader.MarkEndTitle(title);
            return result;
        }

        public static T MarkValue<T>(this IMarkedBitReader reader, string title, Func<T> action){
            reader.MarkStart();

            T result = action();

            reader.MarkEndValue(title, result!);
            return result;
        }

        // Marking overloads

        public static bool NextBit(this IMarkedBitReader reader, string name){
            reader.MarkStart();

            bool result = reader.NextBit();

            reader.MarkEndValue(name, result);
            return result;
        }

        public static int NextChunk(this IMarkedBitReader reader, int bits, string name){
            reader.MarkStart();

            int result = reader.NextChunk(bits);

            reader.MarkEndValue(name, result);
            return result;
        }

        public static T NextChunk<T>(this IMarkedBitReader reader, int bits, string name, Func<int, T> mapper){
            reader.MarkStart();

            T result = mapper(reader.NextChunk(bits));

            reader.MarkEndValue(name, result!);
            return result;
        }

        public static T NextChunk<T, U>(this IMarkedBitReader reader, int bits, string name, U context, Func<int, U, T> mapper){
            reader.MarkStart();

            T result = mapper(reader.NextChunk(bits), context);

            reader.MarkEndValue(name, result!);
            return result;
        }

        public static byte NextAlignedByte(this IMarkedBitReader reader, string name){
            reader.MarkStart();

            byte result = reader.NextAlignedByte();

            reader.MarkEndValue(name, new Literal(result));
            return result;
        }

        // Complex structures

        public static O ReadValue<T, C, O>(this IMarkedBitReader reader, BitDeserializer<T, C> deserialize, C context, string name, Func<T, O> mapper){
            reader.MarkStart();

            O result = mapper(deserialize(reader, context));

            reader.MarkEndValue(name, result!);
            return result;
        }

        public static T ReadValue<T, C>(this IMarkedBitReader reader, BitDeserializer<T, C> deserialize, C context, string name){
            return reader.ReadValue(deserialize, context, name, result => result);
        }

        public static O ReadValue<T, O>(this IMarkedBitReader reader, HuffmanNode<T> tree, string name, Func<T, O> mapper) where T : IComparable<T>{
            reader.MarkStart();

            O result = mapper(tree.LookupValue(reader));

            reader.MarkEndValue(name, result!);
            return result;
        }

        public static T ReadValue<T>(this IMarkedBitReader reader, HuffmanNode<T> tree, string name) where T : IComparable<T>{
            return reader.ReadValue(tree, name, result => result);
        }

        public static T ReadStructure<T, C>(this IMarkedBitReader reader, BitDeserializer<T, C> deserialize, C context, string title){
            reader.MarkStart();

            T result = deserialize(reader, context);

            reader.MarkEndTitle(title);
            return result;
        }

        public static T[] ReadValueArray<T>(this IMarkedBitReader reader, int length, string name, Func<T> supplier){
            return Enumerable.Range(0, length)
                             .Select(counter => reader.MarkValue(name + " " + (counter + 1) + "/" + length, supplier))
                             .ToArray();
        }

        public static T[] ReadStructureArray<T, C>(this IMarkedBitReader reader, int length, BitDeserializer<T, C> deserialize, C context, string title){
            return Enumerable.Range(0, length)
                             .Select(counter => reader.ReadStructure(deserialize, context, title + " " + (counter + 1) + "/" + length))
                             .ToArray();
        }
    }
}
