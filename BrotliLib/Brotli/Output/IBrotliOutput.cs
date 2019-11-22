namespace BrotliLib.Brotli.Output{
    public interface IBrotliOutput{
        int OutputSize { get; }

        void Write(byte value);
        void Write(byte[] bytes);
        byte GetByte(int distance);

        IBrotliOutput Clone();
    }
}
