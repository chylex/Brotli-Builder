using System;
using BrotliLib.IO;
using BrotliLib.Numbers;

namespace BrotliLib.Brotli.Components.Header{
    /// <summary>
    /// Describes a value in the range [1; 256], which can be encoded with a variable 1-11 bit code.
    /// https://tools.ietf.org/html/rfc7932#section-9.2
    /// </summary>
    public sealed class VariableLength11Code{
        public const int MinValue = 1;
        public const int MaxValue = 256;

        public static readonly IntRange Range = new IntRange(MinValue, MaxValue);

        // Data

        public int Value { get; }

        public VariableLength11Code(int value){
            if (!Range.Contains(value)){
                throw new ArgumentOutOfRangeException(nameof(value), "The value must be in the range " + Range + ".");
            }

            this.Value = value;
        }
        
        // Object

        public override bool Equals(object obj){
            return obj is VariableLength11Code code &&
                   Value == code.Value;
        }

        public override int GetHashCode(){
            return HashCode.Combine(Value);
        }

        public override string ToString(){
            return "Value = " + Value;
        }

        // Serialization

        public static readonly BitDeserializer<VariableLength11Code, NoContext> Deserialize = (reader, context) => {
            if (!reader.NextBit()){
                return new VariableLength11Code(MinValue);
            }
            else{
                int chunkBits = reader.NextChunk(3);
                return new VariableLength11Code(MinValue + (1 << chunkBits) + reader.NextChunk(chunkBits));
            }
        };

        public static readonly BitSerializer<VariableLength11Code, NoContext> Serialize = (writer, obj, context) => {
            if (obj.Value == MinValue){
                writer.WriteBit(false);
            }
            else{
                writer.WriteBit(true);

                int offsetValue = obj.Value - MinValue;
                int chunkBits = offsetValue == 0 ? 0 : (int)Math.Floor(Math.Log(offsetValue, 2));

                writer.WriteChunk(3, chunkBits);
                writer.WriteChunk(chunkBits, offsetValue - (1 << chunkBits));
            }
        };
    }
}
