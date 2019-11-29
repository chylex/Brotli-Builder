using BrotliLib.Collections.Huffman;

namespace BrotliLib.Serialization.Writer{
    public class BitWriterNull : IBitWriter{
        private const int ByteSize = 8;

        public int Length { get; private set; }

        public void WriteBit(bool bit){
            Length += 1;
        }

        public void WriteBits(BitStream bits){
            Length += bits.Length;
        }

        public void WriteBits(in BitPath bits){
            Length += bits.Length;
        }

        public void WriteChunk(int count, int value){
            Length += count;
        }

        public void AlignToByteBoundary(){
            int relativeIndex = Length % ByteSize;

            if (relativeIndex > 0){
                Length += ByteSize - relativeIndex;
            }
        }

        public void WriteAlignedBytes(byte[] bytes){
            AlignToByteBoundary();
            Length += bytes.Length * ByteSize;
        }
    }
}
