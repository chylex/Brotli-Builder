namespace BrotliLib.IO{
    /// <summary>
    /// Provides a generic way of deserializing a <see cref="BitStream"/> into an object.
    /// </summary>
    /// <typeparam name="T">Type to convert.</typeparam>
    /// <typeparam name="C">Type used to provide context.</typeparam>
    public delegate T BitDeserializer<T, C>(BitReader reader, C context);

    /// <summary>
    /// Provides a generic way of serializing an object into a <see cref="BitStream"/>.
    /// </summary>
    /// <typeparam name="T">Type to convert.</typeparam>
    /// <typeparam name="C">Type used to provide context.</typeparam>
    public delegate void BitSerializer<T, C>(BitWriter writer, T obj, C context);
}
