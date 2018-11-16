using BrotliLib.Brotli.Components.Contents;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.IO;

namespace BrotliLib.Brotli.Components{
    /// <summary>
    /// Describes both the header and data of a Brotli meta-block.
    /// https://tools.ietf.org/html/rfc7932#section-9.2
    /// https://tools.ietf.org/html/rfc7932#section-9.3
    /// </summary>
    public abstract class MetaBlock{

        // Data

        public bool IsLast { get; set; }
        public DataLength DataLength { get; set; }
        
        protected MetaBlock(bool isLast, DataLength dataLength){
            this.IsLast = isLast;
            this.DataLength = dataLength;
        }

        // Types

        /// <inheritdoc />
        /// <summary>
        /// <code>ISLAST = 1, ISLASTEMPTY = 1</code>
        /// </summary>
        public class LastEmpty : MetaBlock{
            public LastEmpty() : base(true, DataLength.Empty){}
        }
        
        /// <inheritdoc />
        /// <summary>
        /// <code>ISLAST = 0, MLEN = 0</code>
        /// </summary>
        public class PaddedEmpty : MetaBlock{
            public PaddedEmptyMetaBlockContents Contents { get; set; }

            public PaddedEmpty() : base(false, DataLength.Empty){}
        }
        
        /// <inheritdoc />
        /// <summary>
        /// <code>ISLAST = 0, MLEN > 0, ISUNCOMPRESSED = 1</code>
        /// </summary>
        public class Uncompressed : MetaBlock{
            public UncompressedMetaBlockContents Contents { get; set; }

            public Uncompressed(DataLength dataLength) : base(false, dataLength){}
        }
        
        /// <inheritdoc />
        /// <summary>
        /// <code>ISLAST = ?, MLEN > 0, ISUNCOMPRESSED = 0</code>
        /// </summary>
        public class Compressed : MetaBlock{
            public CompressedMetaBlockContents Contents { get; set; }

            public Compressed(bool isLast, DataLength dataLength) : base(isLast, dataLength){}
        }
    }
}
