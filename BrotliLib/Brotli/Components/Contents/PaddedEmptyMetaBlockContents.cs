using BrotliLib.IO;

namespace BrotliLib.Brotli.Components.Contents{
    public sealed class PaddedEmptyMetaBlockContents{

        // Serialization

        public static readonly IBitSerializer<PaddedEmptyMetaBlockContents, MetaBlock> Serializer = new BitSerializer<PaddedEmptyMetaBlockContents, MetaBlock>(
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
