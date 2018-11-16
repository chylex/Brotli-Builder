using BrotliLib.IO;

namespace BrotliLib.Brotli.Components.Contents{
    public sealed class CompressedMetaBlockContents{

        // Serialization

        internal static readonly IBitSerializer<CompressedMetaBlockContents, MetaBlock.Context> Serializer = new BitSerializer<CompressedMetaBlockContents, MetaBlock.Context>(
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
