using BrotliImpl.Combined.Hashers;
using BrotliLib.Numbers;

namespace BrotliImpl.Combined{
    partial class CompressQuality0{
        private class HashTable{
            private const uint MaxTableSize = 1 << 15;
            private const uint HashMul32 = 0x1E35A7BD;
            
            /// <summary>
            /// Adapted from https://github.com/google/brotli/blob/master/c/enc/encode.c (HashTableSize).
            /// </summary>
            private static int HashTableSize(int inputSize){
                int size = 256;

                while(size < MaxTableSize && size < inputSize){
                    size *= 2;
                }

                return size;
            }

            /// <summary>
            /// Adapted from https://github.com/google/brotli/blob/master/c/enc/encode.c (GetHashTable).
            /// </summary>
            private static int[] GetHashTable(int inputSize){
                int size = HashTableSize(inputSize);

                if ((size & 0xAAAAA) == 0){ // 0xAAAAA = 10101010101010101010
                    size *= 2;
                }

                return new int[size];
            }
            
            /// <summary>
            /// Adapted from https://github.com/google/brotli/blob/master/c/enc/compress_fragment.c (Hash).
            /// </summary>
            private static uint Hash(byte[] bytes, int index, int shift){
                ulong h = unchecked((HasherCommon.Load64LE(bytes, index) << 24) * HashMul32);
                return (uint)(h >> shift);
            }
            
            /// <summary>
            /// Adapted from https://github.com/google/brotli/blob/master/c/enc/compress_fragment.c (HashBytesAtOffset).
            /// </summary>
            private static uint HashBytesAtOffset(ulong bytes, int offset, int shift){
                ulong h = unchecked(((bytes >> (8 * offset)) << 24) * HashMul32);
                return (uint)(h >> shift);
            }

            // Instance

            public int this[uint hash]{
                get => table[hash];
                set => table[hash] = value;
            }

            private readonly byte[] input;

            private readonly int[] table;
            private readonly int shift;

            public HashTable(byte[] input){
                this.input = input;

                this.table = GetHashTable(input.Length);
                this.shift = 64 - Log2.Floor(table.Length);
            }

            public uint Hash(int ip){
                return Hash(input, ip, shift);
            }
            
            /// <summary>
            /// Adapted from https://github.com/google/brotli/blob/master/c/enc/compress_fragment.c (BrotliCompressFragmentFastImpl, two occurrences).
            /// </summary>
            public int UpdateAndGetCandidate(int ip, int baseIp){
                ulong inputBytes = HasherCommon.Load64LE(input, ip - 3);
                uint prevHash = HashBytesAtOffset(inputBytes, 0, shift);
                uint curHash = HashBytesAtOffset(inputBytes, 3, shift);

                table[prevHash] = ip - baseIp - 3;
                prevHash = HashBytesAtOffset(inputBytes, 1, shift);
                table[prevHash] = ip - baseIp - 2;
                prevHash = HashBytesAtOffset(inputBytes, 2, shift);
                table[prevHash] = ip - baseIp - 1;

                int candidate = baseIp + table[curHash];
                table[curHash] = ip - baseIp;
                return candidate;
            }
        }
    }
}
