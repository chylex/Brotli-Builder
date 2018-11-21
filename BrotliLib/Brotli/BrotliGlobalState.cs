using System.IO;
using System.Text;
using BrotliLib.Brotli.Components;

namespace BrotliLib.Brotli{
    /// <summary>
    /// Global state used during compression/decompression.
    /// </summary>
    public class BrotliGlobalState{
        public string OutputAsUTF8 => Encoding.UTF8.GetString(decompressedStream.ToArray());

        public WindowSize WindowSize { get; }

        private readonly MemoryStream decompressedStream = new MemoryStream();

        public BrotliGlobalState(WindowSize windowSize){
            this.WindowSize = windowSize;
        }

        public void Output(byte[] data){
            decompressedStream.Write(data, 0, data.Length);
        }
    }
}
