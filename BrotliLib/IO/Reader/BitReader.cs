using System;
using System.Collections.Generic;

namespace BrotliLib.IO.Reader{
    public class BitReader : IBitReader{
        private const int ByteSize = 8;
        private const int MaxChunkSize = ByteSize * sizeof(int);

        public int Index { get; private set; }
        
        private readonly IEnumerator<bool> enumerator;

        /// <summary>
        /// Initializes a new <see cref="BitReader"/> at the beginning of the specified <see cref="BitStream"/>.
        /// </summary>
        /// <param name="stream">Input bit stream.</param>
        public BitReader(BitStream stream){
            this.enumerator = stream.GetEnumerator();
            this.Index = 0;
        }

        // Implementation

        public bool NextBit(){
            if (!enumerator.MoveNext()){
                throw new IndexOutOfRangeException("No more bits left in the stream.");
            }
            
            ++Index;
            return enumerator.Current;
        }

        public int NextChunk(int bits){
            if (bits > MaxChunkSize){
                throw new ArgumentOutOfRangeException(nameof(bits), "Cannot retrieve a chunk larger than " + MaxChunkSize + " bits in one call.");
            }
            
            int value = 0;

            for(int bit = 0; bit < bits; bit++){
                if (NextBit()){
                    value |= 1 << bit;
                }
            }

            return value;
        }

        public void AlignToByteBoundary(){
            int relativeIndex = Index % ByteSize;

            if (relativeIndex > 0){
                for(int bitsLeft = ByteSize - relativeIndex; bitsLeft > 0; bitsLeft--){
                    NextBit();
                }
            }
        }

        public byte NextAlignedByte(){
            AlignToByteBoundary();
            return (byte)NextChunk(ByteSize);
        }
    }
}
