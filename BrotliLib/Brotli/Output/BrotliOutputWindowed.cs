using System;
using BrotliLib.Brotli.Components;
using BrotliLib.Collections;

namespace BrotliLib.Brotli.Output{
    /// <summary>
    /// Stores only enough output to reference bytes inside the sliding window.
    /// </summary>
    public class BrotliOutputWindowed : IBrotliOutput{
        public int OutputSize { get; private set; }

        private readonly RingBuffer<byte> windowBuffer;

        public BrotliOutputWindowed(WindowSize windowSize){
            this.windowBuffer = new RingBuffer<byte>(new byte[windowSize.Bytes]);
        }

        private BrotliOutputWindowed(BrotliOutputWindowed original){
            this.windowBuffer = new RingBuffer<byte>(original.windowBuffer);
            this.OutputSize = original.OutputSize;
        }

        public void Write(byte value){
            windowBuffer.Push(value);
            ++OutputSize;
        }

        public void Write(byte[] bytes){
            foreach(byte value in bytes){
                windowBuffer.Push(value);
            }

            OutputSize += bytes.Length;
        }

        public byte GetByte(int distance){
            int bufferLength = windowBuffer.Length;
            int maxDistance = Math.Min(bufferLength, OutputSize);

            if (distance > maxDistance){
                throw new ArgumentOutOfRangeException(nameof(distance), "Distance is out of range: " + distance + " > " + maxDistance);
            }

            return windowBuffer[bufferLength - distance];
        }

        public IBrotliOutput Clone(){
            return new BrotliOutputWindowed(this);
        }
    }
}
