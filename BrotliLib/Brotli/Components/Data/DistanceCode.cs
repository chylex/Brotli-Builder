using System;
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

        internal static DistanceCode ForValue(DistanceParameters parameters, BrotliGlobalState state, int value){
            var lastDistance = LastDistances.FirstOrDefault(code => code.CanEncodeValue(state, value));

            if (lastDistance != null){
                return lastDistance;
            }
            
            if (value <= parameters.DirectCodeCount){
                return new Direct(value + DirectCodeOffset);
            }
            
            // TODO deuglify this later
            return Enumerable.Range(Last.Codes.Length + parameters.DirectCodeCount, 100000 /* TODO random */)
                             .Select(code => new Complex(parameters, code))
                             .First(code => code.CanEncodeValue(state, value));
        }

        private static DistanceCode Create(DistanceParameters parameters, int code){
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
