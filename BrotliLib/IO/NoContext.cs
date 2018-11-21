namespace BrotliLib.IO{
    /// <summary>
    /// Utility to use as the context type parameter in <see cref="IBitSerializer{T, C}"/>, to disallow passing null as a "no context" parameter.
    /// </summary>
    public struct NoContext{
        public static readonly NoContext Value = new NoContext();
    }
}
