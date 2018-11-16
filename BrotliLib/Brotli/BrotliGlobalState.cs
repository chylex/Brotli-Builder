using System.IO;
using System.Text;

namespace BrotliLib.Brotli{
    /// <summary>
    /// Global state used during compression/decompression.
    /// </summary>
    public class BrotliGlobalState{
        public string OutputAsUTF8 => Encoding.UTF8.GetString(decompressedStream.ToArray());

        private readonly MemoryStream decompressedStream = new MemoryStream();

        public void Output(byte[] data){
            decompressedStream.Write(data, 0, data.Length);
        }
    }
}
