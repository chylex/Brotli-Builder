using System.IO;

namespace BrotliLib.Brotli.Dictionary.Source{
    /// <summary>
    /// Reads dictionary words from bytes stored in memory.
    /// </summary>
    public sealed class MemorySource : IDictionarySource{
        private readonly MemoryStream stream;
        
        /// <summary>
        /// Initializes the source from a file, which is read once and stored in memory.
        /// </summary>
        public MemorySource(string filePath){
            this.stream = new MemoryStream(File.ReadAllBytes(filePath));
        }
        
        public void Dispose(){
            stream.Dispose();
        }

        byte[] IDictionarySource.ReadBytes(int position, int count){
            byte[] bytes = new byte[count];
            stream.Position = position;
            stream.Read(bytes, 0, count);
            return bytes;
        }
    }
}
