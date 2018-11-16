using BrotliLib.IO;

namespace BrotliLib.Brotli.Components.Contents{
    public sealed class UncompressedMetaBlockContents{

        // Serialization

        public static readonly IBitSerializer<UncompressedMetaBlockContents, MetaBlock> Serializer = new BitSerializer<UncompressedMetaBlockContents, MetaBlock>(
            fromBits: (reader, context) => {
                // TODO
                return new UncompressedMetaBlockContents();
            },

            toBits: (writer, obj, context) => {
                // TODO
            }
        );
    }
}
