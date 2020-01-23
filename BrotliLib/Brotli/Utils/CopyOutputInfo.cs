namespace BrotliLib.Brotli.Utils{
    public readonly struct CopyOutputInfo{
        public int BytesWritten { get; }
        public bool IsBackReference { get; }

        public CopyOutputInfo(int bytesWritten, bool isBackReference){
            this.BytesWritten = bytesWritten;
            this.IsBackReference = isBackReference;
        }
    }
}
