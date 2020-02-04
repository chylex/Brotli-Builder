using BrotliImpl.Combined.Hashers;
using BrotliImpl.Combined.Hashers.Types;
using BrotliLib.Brotli.Parameters;

namespace BrotliImpl.Combined{
    public class CompressQuality3 : CompressQualityMedium{
        private protected override IHasher CreateHasher(byte[] bytes, BrotliFileParameters fileParameters){
            return new HashLongestMatchQuickly.Configure{
                BucketBits = 16,
                SweepBits = 1
            }.Build(bytes);
        }
    }
}
