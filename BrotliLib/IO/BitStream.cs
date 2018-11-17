using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrotliLib.IO{
    public class BitStream : IEnumerable<bool>{
        private const char False = '0';
        private const char True = '1';

        private const int ByteSize = 8;
        internal const int BytesPerEntry = sizeof(ulong);
        internal const int BitEntrySize = ByteSize * BytesPerEntry;

        // Instance

        public int Length { get; private set; }
        
        private readonly LinkedList<ulong> bitCollection = new LinkedList<ulong>();
        private LinkedListNode<ulong> lastNode;
        private int lastNodeIndex;
        
        #region Construction

        /// <summary>
        /// Initializes an empty <see cref="BitStream"/>.
        /// </summary>
        public BitStream(){
            this.lastNode = this.bitCollection.AddLast(0L);
            this.lastNodeIndex = 0;
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
                    default: throw new ArgumentOutOfRangeException(nameof(bits), "Invalid character found in input string: "+chr);
                }
            }
        }

        /// <summary>
        /// Initializes a <see cref="BitStream"/> from a byte array.
        /// </summary>
        /// <param name="bytes">Input byte array segment.</param>
        public BitStream(byte[] bytes) : this(){
            int index = 0;

            foreach(byte value in bytes){
                int offset = index % BytesPerEntry;

                if (offset == 0 && index > 0){
                    this.lastNode = this.bitCollection.AddLast(0L);
                    this.lastNodeIndex += BitEntrySize;
                }
                
                this.lastNode.Value |= (ulong)value << (ByteSize * offset);
                ++index;
            }

            this.Length = ByteSize * index;
        }

        /// <summary>
        /// Initializes a new <see cref="BitStream"/> as a clone of <paramref name="source"/>.
        /// </summary>
        /// <param name="source">Source stream.</param>
        private BitStream(BitStream source){
            foreach(ulong bitEntry in source.bitCollection){
                this.bitCollection.AddLast(bitEntry);
            }

            this.lastNode = this.bitCollection.Last;
            this.lastNodeIndex = source.lastNodeIndex;
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
        /// Appends a bit to the end of the stream.
        /// </summary>
        /// <param name="bit">Input bit.</param>
        public void Add(bool bit){
            int offset = Length - lastNodeIndex;

            if (offset >= BitEntrySize){
                lastNode = bitCollection.AddLast(0L);
                lastNodeIndex += BitEntrySize;
                offset -= BitEntrySize;
            }
            
            if (bit){
                lastNode.Value |= 1UL << offset;
            }
            else{
                lastNode.Value &= lastNode.Value & ~(1UL << offset);
            }

            ++Length;
        }
        
        /// <summary>
        /// Appends all bits from the provided <paramref name="stream"/> to the end of this stream.
        /// </summary>
        /// <param name="stream">Input stream.</param>
        public void AddAll(BitStream stream){
            foreach(bool bit in stream){
                Add(bit);
            }
        }

        /// <summary>
        /// Appends a byte to the end of the stream. Intended to use in <see cref="BitWriter"/> only after the stream is byte-aligned, otherwise the behavior is undefined.
        /// </summary>
        /// <param name="value">Input byte.</param>
        internal void AddByte(byte value){
            int offset = Length - lastNodeIndex;

            lastNode.Value |= (ulong)value << offset;
            Length += ByteSize;

            if (offset + ByteSize >= BitEntrySize){
                lastNode = bitCollection.AddLast(0L);
                lastNodeIndex += BitEntrySize;
            }
        }
        
        /// <summary>
        /// Appends 4 bytes to the end of the stream. Intended to use in <see cref="BitWriter"/> only after the stream is long-aligned, otherwise the behavior is undefined.
        /// </summary>
        /// <param name="value">Input bytes combined into an <see cref="ulong"/>.</param>
        internal void AddLong(ulong value){
            lastNode.Value = value;
            lastNode = bitCollection.AddLast(0L);

            lastNodeIndex += BitEntrySize;
            Length += BitEntrySize;
        }

        /// <summary>
        /// Returns a <see cref="BitWriter"/> instance that appends to the end of the stream.
        /// </summary>
        public BitWriter GetWriter(){
            return new BitWriter(this);
        }

        #endregion

        #region Enumeration

        /// <summary>
        /// Returns an enumerator that traverses the stream, converting 0s to false, and 1s to true.
        /// </summary>
        public IEnumerator<bool> GetEnumerator(){
            int bitsLeft = Length;

            foreach(ulong bitEntry in bitCollection){
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
        /// Returns a <see cref="BitReader"/> instance that traverses the stream.
        /// </summary>
        public BitReader GetReader(){
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

            foreach(ulong bitEntry in bitCollection){
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

            foreach(bool bit in this){
                build.Append(bit ? True : False);
            }

            return build.ToString();
        }

        #endregion

        #region Equality

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode(){
            int hash = Length * 17;

            foreach(ulong bitEntry in bitCollection){
                hash = unchecked((hash * 31) + (bitEntry.GetHashCode()));
            }

            return hash;
        }

        /// <summary>
        /// Returns true if and only if the <paramref name="obj"/> parameter is a <see cref="BitStream"/> with the same <see cref="Length"/> and contents.
        /// </summary>
        /// <param name="obj"></param>
        public override bool Equals(object obj){
            return obj is BitStream other && Length == other.Length && bitCollection.SequenceEqual(other.bitCollection);
        }

        #endregion
    }
}
