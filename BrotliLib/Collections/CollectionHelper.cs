using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Numbers;

namespace BrotliLib.Collections{
    public static class CollectionHelper{
        public static byte[] Slice(byte[] input, int start, int count){
            byte[] slice = new byte[count];
            Buffer.BlockCopy(input, start, slice, 0, count);
            return slice;
        }

        public static byte[] Clone(byte[] input){
            return Slice(input, 0, input.Length);
        }

        public static int FindRangeIndex(IntRange[] ranges, int value){
            for(int index = 0; index < ranges.Length; index++){
                if (ranges[index].Contains(value)){
                    return index;
                }
            }

            return -1;
        }
        
        public static ArraySegment<byte> SliceAtMost(ArraySegment<byte> input, int count){
            return input.Slice(0, Math.Min(input.Count, count));
        }

        public static bool ContainsAt(ArraySegment<byte> input, int start, byte[] contents){
            if (contents.Length == 0){
                return true;
            }

            if (input.Count - start < contents.Length){
                return false;
            }

            for(int offset = 0; offset < contents.Length; offset++){
                if (input[start + offset] != contents[offset]){
                    return false;
                }
            }

            return true;
        }

        public static bool Equal(byte[] a, byte[] b){
            return new ReadOnlySpan<byte>(a).SequenceEqual(b);
        }

        public static bool Equal<T>(IReadOnlyList<T>? a, IReadOnlyList<T>? b){
            if (a == null && b == null){
                return true;
            }
            else if (ReferenceEquals(a, b)){
                return true;
            }
            else{
                return a != null && b != null && a.Count == b.Count && a.SequenceEqual(b);
            }
        }

        public static int HashCode<T>(IReadOnlyList<T> collection){
            var hash = new HashCode();

            // ReSharper disable once ForCanBeConvertedToForeach
            for(int index = 0; index < collection.Count; index++){
                hash.Add(collection[index]);
            }

            return hash.ToHashCode();
        }
    }
}
