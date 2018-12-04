using System.IO;

namespace BrotliLib.Brotli.Dictionary.Source{
    /// <summary>
    /// Reads dictionary words from a generic stream.
    /// </summary>
    public class StreamSource : IDictionarySource{
        private readonly Stream stream;

        public StreamSource(Stream stream){
            this.stream = stream;
        }

        public void Dispose(){
            stream.Dispose();
        }

        byte[] IDictionarySource.ReadBytes(int position, int count){
            byte[] bytes = new byte[count];
            stream.Seek(position, SeekOrigin.Begin);
            stream.Read(bytes, 0, count);
            return bytes;
        }
    }
}
