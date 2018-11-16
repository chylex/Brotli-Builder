using BrotliLib.IO;

namespace BrotliLib.Brotli.Components.Contents{
    public sealed class UncompressedMetaBlockContents{

        // Serialization

        internal static readonly IBitSerializer<UncompressedMetaBlockContents, MetaBlock.Context> Serializer = new BitSerializer<UncompressedMetaBlockContents, MetaBlock.Context>(
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
