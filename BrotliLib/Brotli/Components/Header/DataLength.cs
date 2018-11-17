using System;
using BrotliLib.IO;

namespace BrotliLib.Brotli.Components.Header{
    /// <summary>
    /// Describes size of uncompressed data stored in a meta-block.
    /// https://tools.ietf.org/html/rfc7932#section-9.2
    /// </summary>
    public sealed class DataLength{
        private const int MinNibbles = 4;
        private const int MaxNibbles = 6;

        public const int MaxUncompressedBytes = 1 << (4 * MaxNibbles);

        public static DataLength Empty = new DataLength(0, 0);

        private static int CalculateNibblesRequired(int uncompressedBytes){
            if (uncompressedBytes < 0){
                throw new ArgumentOutOfRangeException(nameof(uncompressedBytes), "The amount of bytes must be at least 0.");
            }
            else if (uncompressedBytes == 0){
                return 0;
            }

            for(int nibbles = MinNibbles; nibbles <= MaxNibbles; nibbles++){
                int maxValue = 1 << (4 * nibbles);

                if (uncompressedBytes <= maxValue){
                    return nibbles;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(uncompressedBytes), "The amount of bytes (" + uncompressedBytes + ") cannot be expressed with at most " + MaxNibbles + " nibbles.");
        }

        // Data

        public int ChunkNibbles { get; }
        public int UncompressedBytes { get; }

        private DataLength(int chunkNibbles, int uncompressedBytes){
            this.ChunkNibbles = chunkNibbles;
            this.UncompressedBytes = uncompressedBytes;
        }

        public DataLength(int uncompressedBytes) : this(CalculateNibblesRequired(uncompressedBytes), uncompressedBytes){}

        public override int GetHashCode(){
            return UncompressedBytes;
        }

        public override bool Equals(object obj){
            return obj is DataLength other && other.ChunkNibbles == ChunkNibbles && other.UncompressedBytes == UncompressedBytes;
        }

        // Serialization

        public static readonly IBitSerializer<DataLength, object> Serializer = new BitSerializer<DataLength, object>(
            fromBits: (reader, context) => {
                int chunkNibbles;
		        
                switch(reader.NextChunk(2)){
                    case 0b00: chunkNibbles = 4; break;
                    case 0b01: chunkNibbles = 5; break;
                    case 0b10: chunkNibbles = 6; break;
                    case 0b11: chunkNibbles = 0; break;
                    default: throw new InvalidOperationException("Reading two bits somehow returned a value outside [0, 3].");
                }

                int uncompressedBytes = (chunkNibbles == 0) ? 0 : 1 + reader.NextChunk(4 * chunkNibbles);

                return new DataLength(chunkNibbles, uncompressedBytes);
            },

            toBits: (writer, obj, context) => {
                switch(obj.ChunkNibbles){
                    case 4: writer.WriteChunk(2, 0b00); break;
                    case 5: writer.WriteChunk(2, 0b01); break;
                    case 6: writer.WriteChunk(2, 0b10); break;
                    case 0: writer.WriteChunk(2, 0b11); break;
                    default: throw new InvalidOperationException("Data length object has an invalid amount of nibbles: " + obj.ChunkNibbles);
                }

                writer.WriteChunk(4 * obj.ChunkNibbles, obj.UncompressedBytes - 1);
            }
        );
    }
}
