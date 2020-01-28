using System;
using BrotliLib.Markers.Serialization;
using BrotliLib.Numbers;
using BrotliLib.Serialization;

namespace BrotliLib.Brotli.Components{
    /// <summary>
    /// Describes Brotli window size specified in the stream header.
    /// https://tools.ietf.org/html/rfc7932#section-9.1
    /// </summary>
    public sealed class WindowSize{
        public const int MinBits = 10;
        public const int MaxBits = 24;

        public const int MarginBytes = 16;

        public static readonly IntRange BitsRange = new IntRange(MinBits, MaxBits);

        public static WindowSize Default => new WindowSize(16);

        // Data
        
        public int Bytes => (1 << Bits) - MarginBytes;
        public int Bits { get; }

        public WindowSize(int wbits){
            if (!BitsRange.Contains(wbits)){
                throw new ArgumentOutOfRangeException(nameof(wbits), wbits, "Window size bits must be in the range " + BitsRange + ".");
            }

            this.Bits = wbits;
        }

        // Object

        public override bool Equals(object obj){
            return obj is WindowSize size &&
                   Bits == size.Bits;
        }

        public override int GetHashCode(){
            return HashCode.Combine(Bits);
        }

        public override string ToString(){
            return "Bits = " + Bits + " (Bytes = " + Bytes + ")";
        }

        // Serialization

        public static readonly BitDeserializer<WindowSize, NoContext> Deserialize = MarkedBitDeserializer.Title<WindowSize, NoContext>(
            "Window Size",

            (reader, context) => {
                int wbits = reader.MarkValue("WBITS", () => {
                    if (!reader.NextBit()){ // [0]
                        return 16;
                    }
                    else{
                        int next = reader.NextChunk(3); // [1xxx]

                        if (next != 0){
                            return 17 + next;
                        }
                        else{
                            next = reader.NextChunk(3); // [1000xxx]

                            return next switch{
                                1 => throw new InvalidOperationException("Invalid window size, 1000001 is a reserved value."),
                                0 => 17,
                                _ => 8 + next,
                            };
                        }
                    }
                });
                
                return new WindowSize(wbits);
            }
        );

        public static readonly BitSerializer<WindowSize, NoContext> Serialize = (writer, obj, context) => {
            switch(obj.Bits){
                case 10: writer.WriteChunk(7, 0b_010_000_1); break;
                case 11: writer.WriteChunk(7, 0b_011_000_1); break;
                case 12: writer.WriteChunk(7, 0b_100_000_1); break;
                case 13: writer.WriteChunk(7, 0b_101_000_1); break;
                case 14: writer.WriteChunk(7, 0b_110_000_1); break;
                case 15: writer.WriteChunk(7, 0b_111_000_1); break;
                case 16: writer.WriteChunk(1, 0b_0); break;
                case 17: writer.WriteChunk(7, 0b_000_000_1); break;
                case 18: writer.WriteChunk(4, 0b_001_1); break;
                case 19: writer.WriteChunk(4, 0b_010_1); break;
                case 20: writer.WriteChunk(4, 0b_011_1); break;
                case 21: writer.WriteChunk(4, 0b_100_1); break;
                case 22: writer.WriteChunk(4, 0b_101_1); break;
                case 23: writer.WriteChunk(4, 0b_110_1); break;
                case 24: writer.WriteChunk(4, 0b_111_1); break;
                default: throw new InvalidOperationException("Window size object has an invalid window size parameter (WBITS): " + obj.Bits);
            }
        };
    }
}
