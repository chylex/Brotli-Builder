using System;
using System.Linq;

namespace BrotliLib.Brotli.Dictionary.Transform{
    /// <summary>
    /// https://tools.ietf.org/html/rfc7932#section-8
    /// <para/>
    /// <see cref="Identity"/> returns the input with no change.<para/>
    /// <see cref="FermentFirst"/> performs a custom uppercasing algorithm on the first character.<para/>
    /// <see cref="FermentAll"/> performs a custom uppercasing algorithm on all characters.<para/>
    /// <see cref="OmitFirstN"/> returns a substring that omits the first N characters (add 1-9 to the enum value).<para/>
    /// <see cref="OmitLastN"/> returns a substring that omits the last N characters (add 1-9 to the enum value).<para/>
    /// </summary>
    public enum TransformType{
        Identity = 0,
        FermentFirst = 1,
        FermentAll = 2,
        OmitFirstN = 2, // OmitFirstN + 1-9, range 3-11
        OmitFirst1 = 3,
        OmitFirst9 = 11,
        OmitLastN = 11, // OmitLastN + 1-9, range 12-20
        OmitLast1 = 12,
        OmitLast9 = 20
    }

    public static class TransformTypes{
        public static readonly TransformType[] All = Enumerable.Range(0, 21).Cast<TransformType>().ToArray();

        /// <summary>
        /// Returns a word transformed according to the <see cref="TransformType"/> rule defined in the format specification.
        /// </summary>
        /// <param name="input">Input bytes representing the word.</param>
        /// <param name="type">Type of the transformation.</param>
        public static byte[] Process(this TransformType type, byte[] input){
            if (type == TransformType.Identity){
                return input;
            }
            else if (type >= TransformType.OmitFirst1 && type <= TransformType.OmitFirst9){
                int offset = Math.Min(input.Length, type - TransformType.OmitFirstN);
                return Slice(input, offset, input.Length - offset);
            }
            else if (type >= TransformType.OmitLast1 && type <= TransformType.OmitLast9){
                int count = Math.Max(0, input.Length - (type - TransformType.OmitLastN));
                return Slice(input, 0, count);
            }
            else{
                input = (byte[])input.Clone();
                
                int Ferment(int position){
                    byte value = input[position];

                    if (value >= 97 && value <= 122){
                        // uppercase ASCII characters (a-z)
                        input[position] ^= 32;
                        return 1;
                    }
                    else if (value < 192){
                        // ignore other 1-byte UTF-8 characters
                        return 1;
                    }
                    else if (value < 224){
                        // swap case on certain 2-byte UTF-8 characters
                        // probably intended for Latin-1 Supplement, Greek, and Cyrillic, but it will xor anything in the specified range
                        Xor(input, position + 1, 32);
                        return 2;
                    }
                    else{
                        // literally a magic number that just worked well when the devs were testing 100 languages
                        Xor(input, position + 2, 5);
                        return 3;
                    }
                }

                if (type == TransformType.FermentFirst){
                    Ferment(0);
                }
                else if (type == TransformType.FermentAll){
                    for(int position = 0; position < input.Length;){
                        position += Ferment(position);
                    }
                }

                return input;
            }
        }

        private static void Xor(byte[] output, int position, byte by){
            if (position < output.Length){
                output[position] ^= by;
            }
        }

        private static byte[] Slice(byte[] input, int start, int count){
            byte[] slice = new byte[count];
            Buffer.BlockCopy(input, start, slice, 0, count);
            return slice;
        }
    }
}
