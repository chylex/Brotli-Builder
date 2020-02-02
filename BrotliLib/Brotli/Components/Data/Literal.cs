using System;
using System.Collections.Generic;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Numbers;
using LiteralTree = BrotliLib.Brotli.Components.Header.HuffmanTree<BrotliLib.Brotli.Components.Data.Literal>;

namespace BrotliLib.Brotli.Components.Data{
    /// <summary>
    /// Describes a <see cref="HuffmanTree{T}"/> entry that represents a single byte.
    /// https://tools.ietf.org/html/rfc7932#section-5
    /// </summary>
    public readonly struct Literal : IComparable<Literal>{
        public static readonly AlphabetSize AlphabetSize = new AlphabetSize(256);
        public static readonly LiteralTree.Context TreeContext = new LiteralTree.Context(AlphabetSize, value => new Literal((byte)value), symbol => symbol.Value);
        
        // Conversions

        public static IList<Literal> FromBytes(byte[] bytes){
            return Array.ConvertAll(bytes, b => new Literal(b));
        }

        public static IList<Literal> FromBytes(byte[] bytes, int start, int count){
            Literal[] literals = new Literal[count];

            for(int offset = 0; offset < count; offset++){
                literals[offset] = new Literal(bytes[start + offset]);
            }

            return literals;
        }

        public static IList<Literal> FromBytes(ArraySegment<byte> segment){
            return FromBytes(segment.Array, segment.Offset, segment.Count);
        }

        // Data

        public byte Value { get; }

        public Literal(byte value){
            this.Value = value;
        }

        public int CompareTo(Literal other){
            return Value.CompareTo(other.Value);
        }

        // Object

        public override bool Equals(object obj){
            return obj is Literal literal &&
                   Value == literal.Value;
        }

        public override int GetHashCode(){
            return HashCode.Combine(Value);
        }

        public override string ToString(){
            if (Value == '\n'){
                return "\\n";
            }
            else if (Value == '\r'){
                return "\\r";
            }
            else if (Value >= 32 && Value < 127){
                return "'" + (char)Value + "'";
            }
            else{
                return "\\" + Value;
            }
        }
    }
}
