using System;

namespace BrotliLib.Brotli.Dictionary.Source{
    /// <summary>
    /// Provides a way to read bytes from a representation of the dictionary.
    /// </summary>
    public interface IDictionarySource : IDisposable{
        /// <summary>
        /// Reads <paramref name="count"/> bytes from the source, starting at the provided <paramref name="position"/>.
        /// </summary>
        byte[] ReadBytes(int position, int count);
    }
}
