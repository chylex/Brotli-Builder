namespace BrotliLib.Serialization{
    /// <summary>
    /// Utility to use as the context type parameter in <see cref="BitSerializer{T, C}"/> and <see cref="BitDeserializer{T, C}"/>, to disallow passing null as a "no context" parameter.
    /// </summary>
    public readonly struct NoContext{
        public static readonly NoContext Value = new NoContext();
    }
}
