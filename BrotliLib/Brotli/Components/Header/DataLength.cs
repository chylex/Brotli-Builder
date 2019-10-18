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

        public const int MinUncompressedBytes = 0;
        public const int MaxUncompressedBytes = 1 << (4 * MaxNibbles);

        public static readonly DataLength Empty = new DataLength(0);

        // Data

        public int ChunkNibbles{
            get{
                if (UncompressedBytes == 0){
                    return 0;
                }

                for(int nibbles = MinNibbles; nibbles <= MaxNibbles; nibbles++){
                    int maxValue = 1 << (4 * nibbles);

                    if (UncompressedBytes <= maxValue){
                        return nibbles;
                    }
                }

                throw new InvalidOperationException("The amount of bytes (" + UncompressedBytes + ") cannot be expressed with at most " + MaxNibbles + " nibbles.");
            }
        }

        public int UncompressedBytes { get; }

        public DataLength(int uncompressedBytes){
            if (uncompressedBytes < MinUncompressedBytes || uncompressedBytes > MaxUncompressedBytes){
                throw new ArgumentOutOfRangeException(nameof(uncompressedBytes), "The amount of uncompressed bytes must be in the range [" + MinUncompressedBytes + "; " + MaxUncompressedBytes + "].");
            }

            this.UncompressedBytes = uncompressedBytes;
        }
        
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

        public static readonly BitDeserializer<DataLength, NoContext> Deserialize = MarkedBitDeserializer.Title<DataLength, NoContext>(
            "Data Length",

            (reader, context) => {
                int chunkNibbles = reader.NextChunk(2, "MNIBBLES", value => value switch{
                   0b00 => 4,
                   0b01 => 5,
                   0b10 => 6,
                   0b11 => 0,
                   _ => throw new InvalidOperationException("Reading two bits somehow returned a value outside [0, 3]."),
                });
		        
                int uncompressedBytes = (chunkNibbles == 0) ? 0 : reader.NextChunk(4 * chunkNibbles, "MLEN", value => 1 + value);

                return new DataLength(uncompressedBytes);
            }
        );

        public static readonly BitSerializer<DataLength, NoContext> Serialize = (writer, obj, context) => {
            switch(obj.ChunkNibbles){
                case 4: writer.WriteChunk(2, 0b00); break;
                case 5: writer.WriteChunk(2, 0b01); break;
                case 6: writer.WriteChunk(2, 0b10); break;
                case 0: writer.WriteChunk(2, 0b11); break;
                default: throw new InvalidOperationException("Data length object has an invalid amount of nibbles: " + obj.ChunkNibbles);
            }

            writer.WriteChunk(4 * obj.ChunkNibbles, obj.UncompressedBytes - 1);
        };
    }
}
