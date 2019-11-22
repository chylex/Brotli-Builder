using System;
using BrotliLib.Collections;
using BrotliLib.Markers.Serialization;
using BrotliLib.Markers.Types;
using BrotliLib.Serialization;

namespace BrotliLib.Brotli.Components.Contents{
    public sealed class UncompressedMetaBlockContents{
        public byte[] UncompressedData => CollectionHelper.Clone(uncompressedData);

        private readonly byte[] uncompressedData;

        public UncompressedMetaBlockContents(byte[] uncompressedData){
            this.uncompressedData = uncompressedData;
        }

        // Object

        public override bool Equals(object obj){
            return obj is UncompressedMetaBlockContents contents &&
                   CollectionHelper.Equal(uncompressedData, contents.uncompressedData);
        }

        public override int GetHashCode(){
            return CollectionHelper.HashCode(uncompressedData);
        }
        
        // Serialization
        
        internal static readonly BitDeserializer<UncompressedMetaBlockContents, MetaBlock.Context> Deserialize = MarkedBitDeserializer.Wrap<UncompressedMetaBlockContents, MetaBlock.Context>(
            (reader, context) => {
                byte[] bytes = new byte[context.MetaBlock.DataLength.UncompressedBytes];

                if (bytes.Length == 0){
                    throw new InvalidOperationException("Uncompressed meta-block must not be empty.");
                }

                reader.AlignToByteBoundary();
                reader.MarkStart();

                for(int index = 0; index < bytes.Length; index++){
                    bytes[index] = reader.NextAlignedByte("byte");
                }

                reader.MarkEnd(() => new TitleMarker("Uncompressed Bytes"));

                context.State.OutputBytes(bytes);
                return new UncompressedMetaBlockContents(bytes);
            }
        );

        internal static readonly BitSerializer<UncompressedMetaBlockContents, MetaBlock.Context> Serialize = (writer, obj, context) => {
            byte[] bytes = obj.uncompressedData;

            if (bytes.Length == 0){
                throw new InvalidOperationException("Uncompressed meta-block must not be empty.");
            }
            else if (bytes.Length != context.MetaBlock.DataLength.UncompressedBytes){
                throw new InvalidOperationException("Uncompressed meta-block has invalid data length (" + bytes.Length + " != " + context.MetaBlock.DataLength.UncompressedBytes + ")");
            }
            
            writer.AlignToByteBoundary();
            writer.WriteAlignedBytes(bytes);

            context.State.OutputBytes(bytes);
        };
    }
}
