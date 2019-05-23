using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Brotli.State;
using BrotliLib.IO;
using DistanceTree = BrotliLib.Brotli.Components.Header.HuffmanTree<BrotliLib.Brotli.Components.Data.DistanceCode>;

namespace BrotliLib.Brotli.Components.Data{
    /// <summary>
    /// Describes a <see cref="HuffmanTree{T}"/> entry used to calculate the distance in an insert&amp;copy command.
    /// https://tools.ietf.org/html/rfc7932#section-4
    /// </summary>
    public abstract partial class DistanceCode : IComparable<DistanceCode>{
        public static DistanceTree.Context GenerateTreeContext(DistanceParameters parameters){
            return new DistanceTree.Context(parameters.AlphabetSize, value => Create(parameters, value), symbol => symbol.Code);
        }

        public static DistanceCode Zero => Last.Codes[0];
        private static readonly int DirectCodeOffset = Last.Codes.Length - 1;

        // Data

        public int Code { get; }
        
        protected DistanceCode(int code){
            this.Code = code;
        }

        internal DistanceContext MakeContext(BrotliGlobalState state){
            return new DistanceContext(this, state);
        }

        public abstract bool CanEncodeValue(BrotliGlobalState state, int value);
        
        protected abstract int ReadValue(BrotliGlobalState state, BitReader reader);
        protected abstract void WriteValue(BrotliGlobalState state, int value, BitWriter writer);

        public int CompareTo(DistanceCode other){
            return Code.CompareTo(other.Code);
        }

        // Object

        public override bool Equals(object obj){
            return obj is DistanceCode code &&
                   Code == code.Code;
        }

        public override int GetHashCode(){
            unchecked{
                return -434485196 + Code.GetHashCode();
            }
        }

        public override string ToString(){
            return "Code = " + Code + " | " + GetType().Name;
        }

        // Types

        internal static IReadOnlyList<DistanceCode> ForValue(in DistanceParameters parameters, BrotliGlobalState state, int value){
            List<DistanceCode> valid = new List<DistanceCode>();

            valid.AddRange(Last.Codes.Where(code => code.CanEncodeValue(state, value)));

            if (value <= parameters.DirectCodeCount){
                valid.Add(new Direct(value + DirectCodeOffset));
            }
            else{
                for(int complex = Last.Codes.Length + parameters.DirectCodeCount; /*true*/; complex++){
                    var code = new Complex(parameters, complex);

                    if (code.CanEncodeValue(state, value)){
                        valid.Add(code);
                        break;
                    }
                }
            }

            return valid;
        }

        private static DistanceCode Create(in DistanceParameters parameters, int code){
            if (code < Last.Codes.Length){
                return Last.Codes[code];
            }
            
            int normalized = code - DirectCodeOffset;

            if (normalized <= parameters.DirectCodeCount){
                return new Direct(code);
            }
            else{
                return new Complex(parameters, code);
            }
        }

        // Context

        internal class DistanceContext{
            private readonly DistanceCode code;
            private readonly BrotliGlobalState state;

            public DistanceContext(DistanceCode code, BrotliGlobalState state){
                this.code = code;
                this.state = state;
            }

            internal DistanceInfo Read(BitReader reader){
                if (code.Code == 0){
                    return DistanceInfo.ExplicitCodeZero;
                }
                else{
                    return (DistanceInfo)code.ReadValue(state, reader);
                }
            }

            internal void Write(BitWriter writer, DistanceInfo info){
                if (info != DistanceInfo.ExplicitCodeZero){
                    code.WriteValue(state, info.GetValue(state), writer);
                }
            }
        }

        // Serialization

        internal static readonly IBitSerializer<DistanceInfo, DistanceContext> Serializer = new BitSerializer<DistanceInfo, DistanceContext>(
            fromBits: (reader, context) => context.Read(reader),
            toBits: (writer, obj, context) => context.Write(writer, obj)
        );
    }
}
