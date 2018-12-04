using System.IO;

namespace BrotliLib.Brotli.Dictionary.Source{
    /// <summary>
    /// Reads dictionary words from a file, which will stay open and block write access until disposed.
    /// </summary>
    public sealed class OpenFileSource : StreamSource{
        public OpenFileSource(string filePath) : base(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)){}
    }
}
