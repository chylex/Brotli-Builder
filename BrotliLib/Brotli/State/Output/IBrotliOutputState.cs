namespace BrotliLib.Brotli.State.Output{
    public interface IBrotliOutputState{
        int OutputSize { get; }

        void Write(byte value);
        void Write(byte[] bytes);
        byte GetByte(int distance);

        IBrotliOutputState Clone();
    }
}
