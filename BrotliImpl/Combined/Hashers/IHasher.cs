namespace BrotliImpl.Combined.Hashers{
    /// <summary>
    /// Adapted from https://github.com/google/brotli/blob/master/c/enc/hash.h
    /// </summary>
    interface IHasher{
        int StoreLookahead { get; }
        int HashTypeLength { get; }

        void Store(int ip);
        void StoreRange(int ipStart, int ipEnd);
        void StitchToPreviousBlock(int chunkLength, int ip);

        HasherSearchResult FindLongestMatch(int ip, int maxLength, int maxDistance, int dictionaryStart, int lastDistance, int bestLenIn);
    }
}
