using System;
using System.Collections.Generic;
using System.Linq;

namespace BrotliLib.Collections{
    public static class CollectionHelper{
        public static byte[] Slice(byte[] input, int index, int count){
            byte[] slice = new byte[count];
            Buffer.BlockCopy(input, index, slice, 0, count);
            return slice;
        }

        public static byte[] Clone(byte[] input){
            return Slice(input, 0, input.Length);
        }

        public static bool ContainsAt(byte[] input, int start, byte[] contents){
            if (contents.Length == 0){
                return true;
            }

            if (input.Length - start < contents.Length){
                return false;
            }

            for(int offset = 0; offset < contents.Length; offset++){
                if (input[start + offset] != contents[offset]){
                    return false;
                }
            }

            return true;
        }

        public static IEnumerable<T> Skip<T>(T[] array, int start){
            for(int index = start; index < array.Length; index++){
                yield return array[index];
            }
        }

        public static bool Equal(byte[] a, byte[] b){
            return new ReadOnlySpan<byte>(a).SequenceEqual(b);
        }

        public static bool Equal<T>(IReadOnlyList<T> a, IReadOnlyList<T> b){
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
