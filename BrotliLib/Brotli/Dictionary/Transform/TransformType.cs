using System;
using System.Linq;
using BrotliLib.Collections;

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
        OmitFirst1 = OmitFirstN + 1,
        OmitFirst2 = OmitFirstN + 2,
        OmitFirst3 = OmitFirstN + 3,
        OmitFirst4 = OmitFirstN + 4,
        OmitFirst5 = OmitFirstN + 5,
        OmitFirst6 = OmitFirstN + 6,
        OmitFirst7 = OmitFirstN + 7,
        OmitFirst8 = OmitFirstN + 8,
        OmitFirst9 = OmitFirstN + 9,
        OmitLastN = 11, // OmitLastN + 1-9, range 12-20
        OmitLast1 = OmitLastN + 1,
        OmitLast2 = OmitLastN + 2,
        OmitLast3 = OmitLastN + 3,
        OmitLast4 = OmitLastN + 4,
        OmitLast5 = OmitLastN + 5,
        OmitLast6 = OmitLastN + 6,
        OmitLast7 = OmitLastN + 7,
        OmitLast8 = OmitLastN + 8,
        OmitLast9 = OmitLastN + 9
    }

    public static class TransformTypes{
        public static readonly TransformType[] All = Enumerable.Range(0, 21).Cast<TransformType>().ToArray();

        public static int GetTransformedLength(this TransformType type, int originalLength){
            if (type >= TransformType.OmitFirst1 && type <= TransformType.OmitFirst9){
                return Math.Max(0, originalLength - (type - TransformType.OmitFirstN));
            }
            else if (type >= TransformType.OmitLast1 && type <= TransformType.OmitLast9){
                return Math.Max(0, originalLength - (type - TransformType.OmitLastN));
            }
            else{
                return originalLength;
            }
        }

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
                return CollectionHelper.Slice(input, offset, input.Length - offset);
            }
            else if (type >= TransformType.OmitLast1 && type <= TransformType.OmitLast9){
                int count = Math.Max(0, input.Length - (type - TransformType.OmitLastN));
                return CollectionHelper.Slice(input, 0, count);
            }
            else{
                input = CollectionHelper.Clone(input);

                if (type == TransformType.FermentFirst){
                    Ferment(input, 0);
                }
                else if (type == TransformType.FermentAll){
                    for(int position = 0; position < input.Length;){
                        position += Ferment(input, position);
                    }
                }

                return input;
            }
        }

        /// <summary>
        /// Applies the Ferment operation to the input at a specific position, and returns how far the position should be advanced before the next Ferment call.
        /// </summary>
        public static int Ferment(byte[] input, int position){
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
                position += 1;

                if (position < input.Length){
                    // swap case on certain 2-byte UTF-8 characters
                    // probably intended for Latin-1 Supplement, Greek, and Cyrillic, but it will xor anything in the specified range
                    input[position] ^= 32;
                }

                return 2;
            }
            else{
                position += 2;

                if (position < input.Length){
                    // literally a magic number that just worked well when the devs were testing 100 languages
                    input[position] ^= 5;
                }

                return 3;
            }
        }
    }
}
