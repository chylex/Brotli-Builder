using System;
using System.Collections.Generic;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Serialization;
using BrotliLib.Serialization.Reader;
using BrotliLib.Serialization.Writer;
using DistanceTree = BrotliLib.Brotli.Components.Header.HuffmanTree<BrotliLib.Brotli.Components.Data.DistanceCode>;

namespace BrotliLib.Brotli.Components.Data{
    /// <summary>
    /// Describes a <see cref="HuffmanTree{T}"/> symbol used to calculate the distance in an insert&amp;copy command.
    /// https://tools.ietf.org/html/rfc7932#section-4
    /// </summary>
    public abstract partial class DistanceCode : IComparable<DistanceCode>{
        public static DistanceTree.Context GetTreeContext(DistanceParameters parameters){
            return new DistanceTree.Context(parameters.AlphabetSize, value => Create(parameters, value), symbol => symbol.Code);
        }

        public static DistanceCode Zero => Last.Codes[0];
        private const int DirectCodeOffset = Last.CodeCount - 1;

        // Data

        public int Code { get; }
        
        protected DistanceCode(int code){
            this.Code = code;
        }

        internal DistanceContext MakeContext(BrotliGlobalState state){
            return new DistanceContext(this, state);
        }

        public abstract int ExtraBits { get; }
        public abstract bool CanEncodeValue(BrotliGlobalState state, int value);
        
        protected abstract int ReadValue(BrotliGlobalState state, IBitReader reader);
        protected abstract void WriteValue(BrotliGlobalState state, int value, IBitWriter writer);

        public int CompareTo(DistanceCode other){
            return Code.CompareTo(other.Code);
        }

        // Object

        public bool Equals(DistanceCode code){
            return Code == code.Code;
        }

        public override bool Equals(object obj){
            return obj is DistanceCode code &&
                   Code == code.Code;
        }

        public override int GetHashCode(){
            return HashCode.Combine(Code);
        }

        public override string ToString(){
            return "Code = " + Code + " | " + GetType().Name;
        }

        // Types

        internal static void ForValueExcludingCodeZero(in DistanceParameters parameters, BrotliGlobalState state, int value, List<DistanceCode> outputCodes){
            outputCodes.Clear();

            for(int code = 1; code < Last.CodeCount; code++){
                if (Last.Codes[code].CanEncodeValue(state, value)){
                    outputCodes.Add(Last.Codes[code]);
                }
            }

            if (value <= parameters.DirectCodeCount){
                outputCodes.Add(new Direct(value + DirectCodeOffset));
            }
            else{
                outputCodes.Add(Complex.ForValue(parameters, value));
            }
        }

        public static DistanceCode Create(in DistanceParameters parameters, int code){
            if (code < Last.CodeCount){
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

        internal readonly struct DistanceContext{
            public DistanceCode Code { get; }
            public BrotliGlobalState State { get; }

            public DistanceContext(DistanceCode code, BrotliGlobalState state){
                this.Code = code;
                this.State = state;
            }
        }

        // Serialization

        internal static readonly BitDeserializer<DistanceInfo, DistanceContext> Deserialize = (reader, context) => {
            var code = context.Code;

            if (code.Equals(Zero)){
                return DistanceInfo.ExplicitCodeZero;
            }
            else{
                return (DistanceInfo)code.ReadValue(context.State, reader);
            }
        };

        internal static readonly BitSerializer<DistanceInfo, DistanceContext> Serialize = (writer, obj, context) => {
            if (obj >= DistanceInfo.FirstExactValue){
                var state = context.State;
                context.Code.WriteValue(state, obj.GetValue(state), writer);
            }
        };
    }
}
