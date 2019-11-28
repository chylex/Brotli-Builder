using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrotliLib.Collections;
using BrotliLib.Serialization.Reader;
using BrotliLib.Serialization.Writer;

namespace BrotliLib.Serialization{
    public class BitStream : IEnumerable<bool>{
        private const char False = '0';
        private const char True = '1';
        
        private const int CombinationBits = 8;
        private const int CombinationMask = (1 << CombinationBits) - 1;
        private static readonly string[] Combinations = Enumerable.Range(0, 1 << CombinationBits).Select(i => string.Join("", Convert.ToString(i, 2).PadLeft(CombinationBits, '0').Reverse())).ToArray();

        private const int ByteSize = 8;
        internal const int BytesPerEntry = sizeof(ulong);
        internal const int BitEntrySize = ByteSize * BytesPerEntry;
        private const int BitEntryMask = BitEntrySize - 1;

        // Instance

        public int Length { get; private set; }
        
        private readonly List<ulong> entryCollection = new List<ulong>(128);
        private int LastEntryIndex => entryCollection.Count - 1;
        
        #region Construction

        /// <summary>
        /// Initializes an empty <see cref="BitStream"/>.
        /// </summary>
        public BitStream(){
            this.entryCollection.Add(0UL);
        }

        /// <summary>
        /// Initializes a <see cref="BitStream"/> from a string consisting of 0s and 1s.
        /// </summary>
        /// <param name="bits">Input string. Must be either empty, or only contain the characters 0 and 1.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the input <paramref name="bits"/> string contains a character that is not 0 or 1.</exception>
        public BitStream(string bits) : this(){
            foreach(char chr in bits){
                switch(chr){
                    case False: this.Add(false); break;
                    case True: this.Add(true); break;
                    default: throw new ArgumentOutOfRangeException(nameof(bits), "Invalid character found in input string: " + chr);
                }
            }
        }

        /// <summary>
        /// Initializes a <see cref="BitStream"/> from a byte array.
        /// </summary>
        /// <param name="bytes">Input byte array segment.</param>
        public BitStream(byte[] bytes) : this(){
            foreach(byte value in bytes){
                int offset = PrepareOffsetForNextBit();
                
                entryCollection[LastEntryIndex] |= (ulong)value << offset;
                Length += ByteSize;
            }
        }

        /// <summary>
        /// Initializes a new <see cref="BitStream"/> as a clone of <paramref name="source"/>.
        /// </summary>
        /// <param name="source">Source stream.</param>
        private BitStream(BitStream source){
            this.entryCollection.AddRange(source.entryCollection);
            this.Length = source.Length;
        }

        /// <summary>
        /// Returns a copy of the object, which can be modified without affecting the original object.
        /// </summary>
        public BitStream Clone(){
            return new BitStream(this);
        }

        #endregion

        #region Mutation

        /// <summary>
        /// Returns the position of the next bit within the current <see cref="ulong"/> entry, adding a new entry if needed.
        /// </summary>
        private int PrepareOffsetForNextBit(){
            int offset = Length & BitEntryMask;

            if (offset == 0 && Length > 0){
                entryCollection.Add(0UL);
            }

            return offset;
        }

        /// <summary>
        /// Appends a bit to the end of the stream.
        /// </summary>
        /// <param name="bit">Input bit.</param>
        public void Add(bool bit){
            int offset = PrepareOffsetForNextBit();
            
            if (bit){
                entryCollection[LastEntryIndex] |= 1UL << offset;
            }

            ++Length;
        }
        
        /// <summary>
        /// Appends all bits from the provided <paramref name="stream"/> to the end of this stream.
        /// </summary>
        /// <param name="stream">Input stream.</param>
        public void AddAll(BitStream stream){
            int bitsLeft = stream.Length;

            foreach(ulong bitEntry in stream.entryCollection){
                for(int bitIndex = 0; bitIndex < BitEntrySize; bitIndex++){
                    if (--bitsLeft < 0){
                        return;
                    }

                    Add((bitEntry & (1UL << bitIndex)) != 0);
                }
            }
        }

        /// <summary>
        /// Appends a byte to the end of the stream. Intended to use in <see cref="IBitWriter"/> only after the stream is byte-aligned, otherwise the behavior is undefined.
        /// </summary>
        /// <param name="value">Input byte.</param>
        internal void AddByte(byte value){
            int offset = PrepareOffsetForNextBit();

            entryCollection[LastEntryIndex] |= (ulong)value << offset;
            Length += ByteSize;
        }
        
        /// <summary>
        /// Appends 8 bytes to the end of the stream. Intended to use in <see cref="IBitWriter"/> only after the stream is long-aligned, otherwise the behavior is undefined.
        /// </summary>
        /// <param name="value">Input bytes combined into an <see cref="ulong"/>.</param>
        internal void AddLong(ulong value){
            PrepareOffsetForNextBit();

            entryCollection[LastEntryIndex] = value;
            Length += BitEntrySize;
        }

        /// <summary>
        /// Returns an <see cref="IBitWriter"/> instance that appends to the end of the stream.
        /// </summary>
        public IBitWriter GetWriter(){
            return new BitWriter(this);
        }

        #endregion

        #region Enumeration

        /// <summary>
        /// Returns an enumerator that traverses the stream, converting 0s to false, and 1s to true.
        /// </summary>
        public IEnumerator<bool> GetEnumerator(){
            int bitsLeft = Length;

            foreach(ulong bitEntry in entryCollection){
                for(int bitIndex = 0; bitIndex < BitEntrySize; bitIndex++){
                    if (--bitsLeft < 0){
                        yield break;
                    }

                    yield return (bitEntry & (1UL << bitIndex)) != 0;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Returns an <see cref="IBitReader"/> instance that traverses the stream.
        /// </summary>
        public IBitReader GetReader(){
            return new BitReader(this);
        }

        #endregion

        #region Conversion

        /// <summary>
        /// Converts the stream into a byte array, with zero padding at the end if needed.
        /// </summary>
        public byte[] ToByteArray(){
            const int ByteMask = (1 << ByteSize) - 1;

            byte[] bytes = new byte[(Length + ByteSize - 1) / ByteSize];
            int index = -1;

            foreach(ulong bitEntry in entryCollection){
                if (++index >= bytes.Length){
                    break;
                }

                bytes[index] = (byte)(bitEntry & ByteMask);

                for(int byteOffset = 1; byteOffset < BytesPerEntry; byteOffset++){
                    if (++index >= bytes.Length){
                        break;
                    }

                    bytes[index] = (byte)((bitEntry >> (ByteSize * byteOffset)) & ByteMask);
                }
            }

            return bytes;
        }

        /// <summary>
        /// Converts the stream into its text representation. The returned string is empty if the stream is also empty, or contains a sequence 0s and 1s.
        /// </summary>
        public override string ToString(){
            StringBuilder build = new StringBuilder(Length);
            int bitsLeft = Length;

            foreach(ulong bitEntry in entryCollection){
                if (bitsLeft < BitEntrySize){
                    break;
                }

                bitsLeft -= BitEntrySize;

                for(int index = 0; index < BitEntrySize; index += CombinationBits){
                    build.Append(Combinations[(bitEntry >> index) & CombinationMask]);
                }
            }

            ulong lastValue = entryCollection[LastEntryIndex];

            while(bitsLeft > 0){
                build.Append((lastValue & 1) == 1 ? True : False);
                lastValue >>= 1;
                --bitsLeft;
            }

            return build.ToString();
        }

        #endregion

        #region Equality

        /// <summary>
        /// Returns true if and only if the <paramref name="obj"/> parameter is a <see cref="BitStream"/> with the same <see cref="Length"/> and contents.
        /// </summary>
        /// <param name="obj"></param>
        public override bool Equals(object obj){
            return obj is BitStream stream &&
                   Length == stream.Length &&
                   CollectionHelper.Equal(entryCollection, stream.entryCollection);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode(){
            return HashCode.Combine(Length, CollectionHelper.HashCode(entryCollection));
        }

        #endregion
    }
}
