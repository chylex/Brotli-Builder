using System;
using System.Collections.Generic;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Brotli.Output;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Collections;

namespace BrotliLib.Brotli{
    /// <summary>
    /// Global state used during compression/decompression.
    /// </summary>
    public class BrotliGlobalState{
        public int OutputSize => outputState.OutputSize;
        public int MaxDistance => Math.Min(Parameters.WindowSize.Bytes, OutputSize);

        public BrotliFileParameters Parameters { get; }
        
        public RingBuffer<byte> LiteralBuffer { get; }
        public RingBuffer<int> DistanceBuffer { get; }
        
        private readonly IBrotliOutput outputState;
        
        // Construction
        
        public BrotliGlobalState(BrotliFileParameters parameters, IBrotliOutput outputState){
            this.Parameters = parameters;
            this.outputState = outputState;

            this.LiteralBuffer = new RingBuffer<byte>(0, 0);
            this.DistanceBuffer = new RingBuffer<int>(16, 15, 11, 4);
        }

        public BrotliGlobalState(BrotliFileParameters parameters) : this(parameters, new BrotliOutputWindowed(parameters.WindowSize)){}

        private BrotliGlobalState(BrotliGlobalState original){
            this.Parameters = original.Parameters;
            this.outputState = original.outputState.Clone();

            this.LiteralBuffer = new RingBuffer<byte>(original.LiteralBuffer);
            this.DistanceBuffer = new RingBuffer<int>(original.DistanceBuffer);
        }

        public BrotliGlobalState Clone(){
            return new BrotliGlobalState(this);
        }

        // State helpers
        
        public int NextLiteralContextID(LiteralContextMode mode){
            return mode.DetermineContextID(LiteralBuffer.Front, LiteralBuffer.Back);
        }

        private void UpdateLiteralBuffer(){
            int length = OutputSize;

            if (length >= 1){
                if (length >= 2){
                    LiteralBuffer.Push(outputState.GetByte(2));
                }

                LiteralBuffer.Push(outputState.GetByte(1));
            }
        }

        // Output handling

        internal byte GetOutput(int distance){
            return outputState.GetByte(distance);
        }

        public void OutputLiteral(in Literal literal){
            var value = literal.Value;
            outputState.Write(value);
            LiteralBuffer.Push(value);
        }

        public void OutputLiterals(IReadOnlyList<Literal> literals){
            for(int index = 0; index < literals.Count; index++){
                outputState.Write(literals[index].Value);
            }

            UpdateLiteralBuffer();
        }
        
        public void OutputBytes(byte[] bytes){
            outputState.Write(bytes);
            UpdateLiteralBuffer();
        }
        
        public CopyOutputInfo OutputCopy(int length, DistanceInfo distance){
            int distanceValue = distance.GetValue(this);
            int maxDistance = MaxDistance;

            if (distanceValue <= maxDistance){
                if (distance.ShouldWriteToDistanceBuffer()){
                    DistanceBuffer.Push(distanceValue);
                }

                for(int index = 0; index < length; index++){
                    outputState.Write(outputState.GetByte(distanceValue));
                }

                UpdateLiteralBuffer();
                return new CopyOutputInfo(length, isBackReference: true);
            }
            else{
                byte[] word = Parameters.Dictionary.ReadTransformed(length, distanceValue - maxDistance - 1);

                OutputBytes(word);
                return new CopyOutputInfo(word.Length, isBackReference: false);
            }
        }
    }
}
