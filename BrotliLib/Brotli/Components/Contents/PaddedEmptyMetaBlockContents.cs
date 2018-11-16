using BrotliLib.IO;

namespace BrotliLib.Brotli.Components.Contents{
    public sealed class PaddedEmptyMetaBlockContents{

        // Serialization

        internal static readonly IBitSerializer<PaddedEmptyMetaBlockContents, MetaBlock.Context> Serializer = new BitSerializer<PaddedEmptyMetaBlockContents, MetaBlock.Context>(
            fromBits: (reader, context) => {
                // TODO
                return new PaddedEmptyMetaBlockContents();
            },

            toBits: (writer, obj, context) => {
                // TODO
            }
        );
    }
}
