using System;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Brotli.Dictionary;
using BrotliLib.Brotli.State.Output;
using BrotliLib.Collections;

namespace BrotliLib.Brotli.State{
    /// <summary>
    /// Global state used during compression/decompression.
    /// </summary>
    public class BrotliGlobalState{
        public int OutputSize => outputState.OutputSize;
        public int MaxDistance => Math.Min(WindowSize.Bytes, OutputSize);

        public BrotliDictionary Dictionary { get; }
        public WindowSize WindowSize { get; }
        
        public RingBuffer<byte> LiteralBuffer { get; }
        public RingBuffer<int> DistanceBuffer { get; }
        
        private readonly IBrotliOutputState outputState;
        
        public BrotliGlobalState(BrotliDictionary dictionary, WindowSize windowSize, IBrotliOutputState outputState){
            this.Dictionary = dictionary;
            this.WindowSize = windowSize;
            this.outputState = outputState;

            this.LiteralBuffer = new RingBuffer<byte>(0, 0);
            this.DistanceBuffer = new RingBuffer<int>(16, 15, 11, 4);
        }

        // State helpers
        
        public int NextLiteralContextID(LiteralContextMode mode){
            return mode.DetermineContextID(LiteralBuffer.Front, LiteralBuffer.Back);
        }

        // Output handling

        private void WriteByte(byte value){
            outputState.Write(value);
            LiteralBuffer.Push(value);
        }
        
        public void OutputBytes(byte[] bytes){
            outputState.Write(bytes);

            int length = bytes.Length;

            if (length >= 2){
                LiteralBuffer.Push(bytes[length - 2]);
            }

            if (length >= 1){
                LiteralBuffer.Push(bytes[length - 1]);
            }
        }

        public void OutputLiteral(in Literal literal){
            WriteByte(literal.Value);
        }
        
        public int OutputCopy(int length, DistanceInfo distance){
            int distanceValue = distance.GetValue(this);
            int maxDistance = MaxDistance;

            if (distanceValue <= maxDistance){
                if (distance.ShouldWriteToDistanceBuffer()){
                    DistanceBuffer.Push(distanceValue);
                }

                for(int index = 0; index < length; index++){
                    WriteByte(outputState.GetByte(distanceValue));
                }

                return length;
            }
            else{
                byte[] word = Dictionary.ReadTransformed(length, distanceValue - maxDistance - 1);

                OutputBytes(word);
                return word.Length;
            }
        }
    }
}
