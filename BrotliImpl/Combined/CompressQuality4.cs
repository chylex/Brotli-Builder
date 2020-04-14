using BrotliImpl.Combined.Hashers;
using BrotliImpl.Combined.Hashers.Types;
using BrotliImpl.Transformers;
using BrotliLib.Brotli.Parameters;

namespace BrotliImpl.Combined{
    public class CompressQuality4 : CompressQualityMedium{
        protected override Features SupportedFeatures => Features.BlockSplit;

        public CompressQuality4(){
            Transformers.Add(new TransformOfficialBlockSplitterLQ());
        }

        private protected override IHasher CreateHasher(byte[] bytes, BrotliFileParameters fileParameters){
            return new HashLongestMatchQuickly.Configure{
                BucketBits = 17,
                SweepBits = 2,
                Dictionary = fileParameters.Dictionary
            }.Build(bytes);
        }
    }
}
