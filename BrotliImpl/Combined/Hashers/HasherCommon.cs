namespace BrotliImpl.Combined.Hashers{
    abstract class HasherCommon : IHasher{
        /// <summary>
        /// Adapted from https://github.com/google/brotli/blob/master/c/enc/platform.h (BROTLI_UNALIGNED_LOAD64LE).
        /// </summary>
        public static ulong Load64LE(byte[] bytes, int start){
            ulong n = 0L;

            for(int offset = 0; offset < sizeof(ulong); offset++){
                n |= (ulong)bytes[start + offset] << (8 * offset);
            }

            return n;
        }

        // Abstract

        public abstract int StoreLookahead { get; }
        public abstract int HashTypeLength { get; }

        public abstract void Store(int ip);
        public abstract void StoreRange(int ipStart, int ipEnd);
        public abstract void StitchToPreviousBlock(int chunkLength, int ip);

        public abstract HasherSearchResult FindLongestMatch(int ip, int maxLength, int maxDistance, int dictionaryStart, int lastDistance, int bestLenIn);
    }
}
