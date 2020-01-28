using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Dictionary;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Markers;
using BrotliLib.Markers.Serialization.Reader;
using BrotliLib.Serialization;

namespace BrotliLib.Brotli{
    /// <summary>
    /// Provides a streaming reader as an alternative to <see cref="BrotliFileStructure"/>.
    /// </summary>
    public sealed class BrotliFileReader{
        public static BrotliFileReader FromBytes(byte[] bytes, MarkerLevel markerLevel, BrotliDictionary? dictionary = null){
            return FromBytes(new BitStream(bytes), markerLevel, dictionary);
        }

        public static BrotliFileReader FromBytes(BitStream bits, MarkerLevel markerLevel, BrotliDictionary? dictionary = null){
            return new BrotliFileReader(bits, markerLevel, dictionary ?? BrotliFileParameters.Default.Dictionary);
        }

        // Instance

        public BrotliFileParameters Parameters { get; }
        public BrotliGlobalState State { get; }

        public MarkerRoot MarkerRoot => reader.MarkerRoot;

        private readonly IMarkedBitReader reader;
        private bool isAtEnd = false;

        private BrotliFileReader(BitStream bits, MarkerLevel markerLevel, BrotliDictionary dictionary){
            this.reader = markerLevel.CreateBitReader(bits);

            this.Parameters = new BrotliFileParameters.Builder{
                WindowSize = ReadHeader(),
                Dictionary = dictionary
            }.Build();

            this.State = new BrotliGlobalState(Parameters);
        }

        // Reader

        private WindowSize ReadHeader(){
            return WindowSize.Deserialize(reader, NoContext.Value);
        }

        public MetaBlock? NextMetaBlock(){
            if (isAtEnd){
                return null;
            }

            MetaBlock metaBlock = MetaBlock.Deserialize(reader, State);
            isAtEnd = metaBlock.IsLast;
            return metaBlock;
        }
    }
}
