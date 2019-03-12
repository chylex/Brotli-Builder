using System;
using System.IO;
using System.Text;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Brotli.Dictionary;
using BrotliLib.Collections;
using BrotliLib.Markers;

namespace BrotliLib.Brotli{
    /// <summary>
    /// Global state used during compression/decompression.
    /// </summary>
    public class BrotliGlobalState{
        public int OutputSize => (int)decompressedStream.Length;
        public int MaxDistance => Math.Min(WindowSize.Bytes, OutputSize);

        public byte[] OutputAsBytes => decompressedStream.ToArray();
        public string OutputAsUTF8 => Encoding.UTF8.GetString(decompressedStream.ToArray());

        public BrotliDictionary Dictionary { get; }
        public WindowSize WindowSize { get; }

        public RingBuffer<byte> LiteralBuffer { get; }
        public RingBuffer<int> DistanceBuffer { get; }

        public MarkerRoot BitMarkerRoot { get; internal set; }

        private readonly MemoryStream decompressedStream = new MemoryStream();

        public BrotliGlobalState(BrotliDictionary dictionary, WindowSize windowSize){
            this.Dictionary = dictionary;
            this.WindowSize = windowSize;
            this.LiteralBuffer = new RingBuffer<byte>(0, 0);
            this.DistanceBuffer = new RingBuffer<int>(16, 15, 11, 4);
        }
        
        public int NextLiteralContextID(LiteralContextMode mode){
            return mode.DetermineContextID(LiteralBuffer.Front, LiteralBuffer.Back);
        }

        public byte GetByteAt(int position){
            long prevPos = decompressedStream.Position;

            decompressedStream.Position = position;
            int readByte = decompressedStream.ReadByte();
            decompressedStream.Position = prevPos;

            return readByte >= 0 ? (byte)readByte : throw new ArgumentOutOfRangeException(nameof(position), "Position is out of range: " + position + " >= " + decompressedStream.Length);
        }

        public void Output(byte data){
            decompressedStream.WriteByte(data);
            LiteralBuffer.Push(data);
        }

        public void Output(byte[] data){
            int length = data.Length;
            decompressedStream.Write(data, 0, length);

            if (length >= 2){
                LiteralBuffer.Push(data[length - 2]);
            }

            if (length >= 1){
                LiteralBuffer.Push(data[length - 1]);
            }
        }
    }
}
