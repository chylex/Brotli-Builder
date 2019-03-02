using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.IO;

namespace BrotliLib.Brotli.Components{
    /// <summary>
    /// Describes Brotli window size specified in the stream header.
    /// https://tools.ietf.org/html/rfc7932#section-9.1
    /// </summary>
    public sealed class WindowSize{
        public const int MinBits = 10;
        public const int MaxBits = 24;

        public static WindowSize Default => new WindowSize(16);
        public static IEnumerable<WindowSize> ValidValues = Enumerable.Range(MinBits, MaxBits - MinBits + 1).Select(bits => new WindowSize(bits)).ToList();

        // Data
        
        public int Bytes => (1 << Bits) - 16;
        public int Bits { get; }

        public WindowSize(int wbits){
            if (wbits < MinBits || wbits > MaxBits){
                throw new ArgumentOutOfRangeException(nameof(wbits), "Window size parameter (WBITS) must be between " + MinBits + " and " + MaxBits + ".");
            }

            this.Bits = wbits;
        }

        // Object

        public override bool Equals(object obj){
            return obj is WindowSize size &&
                   Bits == size.Bits;
        }

        public override int GetHashCode(){
            unchecked{
                return -943821695 + Bits.GetHashCode();
            }
        }

        public override string ToString(){
            return "Bits = " + Bits + " (Bytes = " + Bytes + ")";
        }

        // Serialization

        public static readonly IBitSerializer<WindowSize, NoContext> Serializer = new BitSerializer<WindowSize, NoContext>(
            fromBits: (reader, context) => {
                int wbits;

                if (!reader.NextBit()){ // [0]
                    wbits = 16;
                }
                else{
                    int next = reader.NextChunk(3); // [1xxx]

                    if (next != 0){
                        wbits = 17 + next;
                    }
                    else{
                        next = reader.NextChunk(3); // [1000xxx]

                        switch(next){
                            case 1: throw new InvalidOperationException("Invalid window size, 1000001 is a reserved value.");
                            case 0: wbits = 17; break;
                            default: wbits = 8 + next; break;
                        }
                    }
                }

                return new WindowSize(wbits);
            },

            toBits: (writer, obj, context) => {
                int value;
                int count;

                switch(obj.Bits){
                    case 10: value = 0b010_000_1; count = 7; break;
                    case 11: value = 0b011_000_1; count = 7; break;
                    case 12: value = 0b100_000_1; count = 7; break;
                    case 13: value = 0b101_000_1; count = 7; break;
                    case 14: value = 0b110_000_1; count = 7; break;
                    case 15: value = 0b111_000_1; count = 7; break;
                    case 16: value = 0b0; count = 1; break;
                    case 17: value = 0b000_000_1; count = 7; break;
                    case 18: value = 0b001_1; count = 4; break;
                    case 19: value = 0b010_1; count = 4; break;
                    case 20: value = 0b011_1; count = 4; break;
                    case 21: value = 0b100_1; count = 4; break;
                    case 22: value = 0b101_1; count = 4; break;
                    case 23: value = 0b110_1; count = 4; break;
                    case 24: value = 0b111_1; count = 4; break;
                    default: throw new InvalidOperationException("Window size object has an invalid window size parameter (WBITS): " + obj.Bits);
                }

                writer.WriteChunk(count, value);
            }
        );
    }
}
