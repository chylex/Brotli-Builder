using BrotliLib.IO;

namespace BrotliLib.Brotli.Components.Contents{
    public sealed class CompressedMetaBlockContents{

        // Serialization

        public static readonly IBitSerializer<CompressedMetaBlockContents, MetaBlock> Serializer = new BitSerializer<CompressedMetaBlockContents, MetaBlock>(
            fromBits: (reader, context) => {
                // TODO
                return new CompressedMetaBlockContents();
            },

            toBits: (writer, obj, context) => {
                // TODO
            }
        );
    }
}
