using BrotliImpl.Combined.Hashers;
using BrotliImpl.Combined.Hashers.Types;
using BrotliLib.Brotli.Parameters;

namespace BrotliImpl.Combined{
    public class CompressQuality2 : CompressQualityMedium{
        private protected override IHasher CreateHasher(byte[] bytes, BrotliFileParameters fileParameters){
            return new HashLongestMatchQuickly.Configure{
                BucketBits = 16,
                // TODO the dictionary lookup is too good and causes a LOT more successful matches than the official compressor
                // Dictionary = fileParameters.Dictionary
            }.Build(bytes);
        }
    }
}
