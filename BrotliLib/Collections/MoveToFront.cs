using System;
using System.Linq;

namespace BrotliLib.Collections{
    public static class MoveToFront{
        /// <summary>
        /// Performs an in-place move-to-front transformation on the input byte array.
        /// </summary>
        public static readonly Action<byte[]> Encode = contextMap => {
            byte[] mtf = CreateAlphabet(contextMap.Max() + 1);

            for(int index = 0; index < contextMap.Length; index++){
                byte mtfIndex = (byte)Array.IndexOf(mtf, contextMap[index]);
                byte value = mtf[mtfIndex];

                contextMap[index] = mtfIndex;
                MoveValueToFront(mtf, mtfIndex, value);
            }
        };
        
        /// <summary>
        /// Performs an in-place inverse-move-to-front transformation on the input byte array.
        /// </summary>
        public static readonly Action<byte[]> Decode = contextMap => {
            byte[] mtf = CreateAlphabet(256);

            for(int index = 0; index < contextMap.Length; index++){
                byte mapIndex = contextMap[index];
                byte value = mtf[mapIndex];

                contextMap[index] = value;
                MoveValueToFront(mtf, mapIndex, value);
            }
        };

        // Helpers

        private static byte[] CreateAlphabet(int size){
            byte[] mtf = new byte[size];

            for(int index = 0; index < mtf.Length; index++){
                mtf[index] = (byte)index;
            }

            return mtf;
        }

        private static void MoveValueToFront(byte[] mtf, byte index, byte value){
            while(index > 0){
                mtf[index] = mtf[--index];
            }

            mtf[0] = value;
        }
    }
}
