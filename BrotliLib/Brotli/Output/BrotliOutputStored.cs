﻿using System;
using System.IO;
using System.Text;
using BrotliLib.Markers;

namespace BrotliLib.Brotli.Output{
    /// <summary>
    /// Stores all generated output in memory.
    /// </summary>
    public class BrotliOutputStored : IBrotliOutput{
        public int OutputSize => (int)decompressedStream.Length;

        public byte[] AsBytes => decompressedStream.ToArray();
        public string AsUTF8 => Encoding.UTF8.GetString(decompressedStream.ToArray());

        public MarkerRoot MarkerRoot { get; }

        private readonly MemoryStream decompressedStream = new MemoryStream();

        public BrotliOutputStored(MarkerRoot markerRoot){
            this.MarkerRoot = markerRoot;
        }

        private BrotliOutputStored(BrotliOutputStored original) : this(new MarkerRoot()){ // TODO not full clone as it's missing the bit marker
            original.decompressedStream.CopyTo(decompressedStream);
        }

        public void Write(byte value){
            decompressedStream.WriteByte(value);
        }

        public void Write(byte[] bytes){
            decompressedStream.Write(bytes, 0, bytes.Length);
        }

        public byte GetByte(int distance){
            long prevPos = decompressedStream.Position;
            
            decompressedStream.Position -= distance;
            int readByte = decompressedStream.ReadByte();
            decompressedStream.Position = prevPos;

            return readByte >= 0 ? (byte)readByte : throw new ArgumentOutOfRangeException(nameof(distance), "Distance is out of range: " + distance + " > " + decompressedStream.Length);
        }

        public IBrotliOutput Clone(){
            return new BrotliOutputStored(this);
        }
    }
}
