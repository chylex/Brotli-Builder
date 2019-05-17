using System;

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
    }
}
