using System.IO;

namespace BrotliLib.Brotli.Dictionary.Source{
    /// <summary>
    /// Reads dictionary words from a file, which will stay open and block write access until disposed.
    /// </summary>
    public sealed class OpenFileSource : IDictionarySource{
        private readonly FileStream file;
        
        public OpenFileSource(string filePath){
            this.file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        
        public void Dispose(){
            file.Dispose();
        }

        byte[] IDictionarySource.ReadBytes(int position, int count){
            byte[] bytes = new byte[count];
            file.Seek(position, SeekOrigin.Begin);
            file.Read(bytes, 0, count);
            return bytes;
        }
    }
}
