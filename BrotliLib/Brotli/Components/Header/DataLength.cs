using System;
using BrotliLib.Brotli.Markers;
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

        public static readonly DataLength Empty = new DataLength(0, 0);

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
        
        // Object

        public override bool Equals(object obj){
            return obj is DataLength length &&
                   ChunkNibbles == length.ChunkNibbles &&
                   UncompressedBytes == length.UncompressedBytes;
        }

        public override int GetHashCode(){
            unchecked{
                var hashCode = 1631702163;
                hashCode = hashCode * -1521134295 + ChunkNibbles.GetHashCode();
                hashCode = hashCode * -1521134295 + UncompressedBytes.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString(){
            return "ChunkNibbles = " + ChunkNibbles + ", UncompressedBytes = " + UncompressedBytes;
        }

        // Serialization

        public static readonly IBitSerializer<DataLength, NoContext> Serializer = new MarkedBitSerializer<DataLength, NoContext>(
            markerTitle: "Data Length",

            fromBits: (reader, context) => {
                int chunkNibbles = reader.NextChunk(2, "MNIBBLES", value => {
                    switch(value){
                        case 0b00: return 4;
                        case 0b01: return 5;
                        case 0b10: return 6;
                        case 0b11: return 0;
                        default: throw new InvalidOperationException("Reading two bits somehow returned a value outside [0, 3].");
                    }
                });
		        
                int uncompressedBytes = (chunkNibbles == 0) ? 0 : reader.NextChunk(4 * chunkNibbles, "MLEN", value => 1 + value);

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
