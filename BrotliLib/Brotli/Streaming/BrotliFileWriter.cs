using System;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Serialization;
using BrotliLib.Serialization.Writer;

namespace BrotliLib.Brotli.Streaming{
    public sealed class BrotliFileWriter{
        /// <summary>
        /// Provides a streaming meta-block serializer.
        /// </summary>
        public BrotliFileParameters FileParameters { get; }
        public BrotliSerializationParameters SerializationParameters { get; }

        private BitStream? bits;
        private IBitWriter? writer;
        private readonly BrotliGlobalState state;

        private MetaBlock? nextToWrite;

        public BrotliFileWriter(BrotliFileParameters fileParameters, BrotliSerializationParameters serializationParameters){
            this.bits = new BitStream();
            this.writer = bits.GetWriter();

            this.FileParameters = fileParameters;
            this.SerializationParameters = serializationParameters;

            this.state = new BrotliGlobalState(FileParameters);

            WriteHeader();
        }

        // Writer

        private void WriteHeader(){
            WindowSize.Serialize(writer!, FileParameters.WindowSize, NoContext.Value);
        }

        public void WriteMetaBlock(MetaBlock metaBlock){
            CheckCanWrite();

            if (nextToWrite != null){
                MetaBlock.Serialize(writer!, nextToWrite.Mark(isLast: false), state, SerializationParameters);
            }

            nextToWrite = metaBlock;
        }

        // Finalization

        private void CheckCanWrite(){
            if (bits == null){
                throw new InvalidOperationException("This writer has already been closed.");
            }
        }

        public BitStream Close(){
            CheckCanWrite();

            if (nextToWrite == null){ // no meta-blocks have been written
                MetaBlock.Serialize(writer!, MetaBlock.LastEmpty.Marked, state, SerializationParameters);
            }
            else if (nextToWrite is MetaBlock.Uncompressed){
                MetaBlock.Serialize(writer!, nextToWrite.Mark(isLast: false), state, SerializationParameters);
                MetaBlock.Serialize(writer!, MetaBlock.LastEmpty.Marked, state, SerializationParameters);
            }
            else{
                MetaBlock.Serialize(writer!, nextToWrite.Mark(isLast: true), state, SerializationParameters);
            }

            var finalBits = bits!;
            bits = null;
            writer = null;
            nextToWrite = null;
            return finalBits;
        }
    }
}
