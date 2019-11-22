using System;
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

        public MarkerRoot BitMarkerRoot { get; internal set; }

        private readonly MemoryStream decompressedStream = new MemoryStream();

        public BrotliOutputStored(){}

        private BrotliOutputStored(BrotliOutputStored original){
            original.decompressedStream.CopyTo(decompressedStream);
            // TODO not full clone as it's missing the bit marker
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
