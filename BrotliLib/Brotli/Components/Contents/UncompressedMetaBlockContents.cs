﻿using System;
using System.Collections.Generic;
using BrotliLib.Brotli.Markers;
using BrotliLib.Brotli.Markers.Data;
using BrotliLib.Collections;
using BrotliLib.IO;

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
                   EqualityComparer<byte[]>.Default.Equals(uncompressedData, contents.uncompressedData);
        }

        public override int GetHashCode(){
            unchecked{
                return 1470885995 + EqualityComparer<byte[]>.Default.GetHashCode(uncompressedData);
            }
        }
        
        // Serialization

        internal static readonly IBitSerializer<UncompressedMetaBlockContents, MetaBlock.Context> Serializer = new MarkedBitSerializer<UncompressedMetaBlockContents, MetaBlock.Context>(
            fromBits: (reader, context) => {
                byte[] bytes = new byte[context.MetaBlock.DataLength.UncompressedBytes];

                if (bytes.Length == 0){
                    throw new InvalidOperationException("Uncompressed meta-block must not be empty.");
                }

                reader.AlignToByteBoundary();
                reader.MarkStart();

                for(int index = 0; index < bytes.Length; index++){
                    bytes[index] = reader.NextAlignedByte("byte");
                }

                reader.MarkEnd(new TitleMarker("Uncompressed Bytes"));

                context.State.OutputBytes(bytes);
                return new UncompressedMetaBlockContents(bytes);
            },

            toBits: (writer, obj, context) => {
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
            }
        );
    }
}
