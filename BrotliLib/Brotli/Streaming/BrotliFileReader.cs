using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Dictionary;
using BrotliLib.Brotli.Output;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Markers;
using BrotliLib.Markers.Serialization.Reader;
using BrotliLib.Serialization;

namespace BrotliLib.Brotli.Streaming{
    /// <summary>
    /// Provides a streaming meta-block deserializer as an alternative to <see cref="BrotliFileStructure"/>.
    /// </summary>
    public sealed class BrotliFileReader : IBrotliFileStream{
        public static BrotliFileReader FromBytes(byte[] bytes, BrotliDictionary? dictionary = null, in MarkerSettings markerSettings = default){
            return FromBytes(new BitStream(bytes), dictionary, markerSettings);
        }

        public static BrotliFileReader FromBytes(BitStream bits, BrotliDictionary? dictionary = null, in MarkerSettings markerSettings = default){
            return new BrotliFileReader(bits, markerSettings, dictionary ?? BrotliFileParameters.Default.Dictionary);
        }

        // Instance

        public BrotliFileParameters Parameters { get; }
        public BrotliGlobalState State => state.Clone();

        private readonly IMarkedBitReader reader;
        private readonly BrotliGlobalState state;
        private bool isAtEnd = false;

        private BrotliFileReader(BitStream bits, in MarkerSettings markerSettings, BrotliDictionary dictionary){
            this.reader = markerSettings.CreateBitReader(bits);

            this.Parameters = new BrotliFileParameters.Builder{
                WindowSize = ReadHeader(),
                Dictionary = dictionary
            }.Build();

            this.state = new BrotliGlobalState(Parameters);
        }

        // Output

        public void AddOutputCallback(IBrotliOutput callback){
            state.AddOutputCallback(callback);
        }

        public void RemoveOutputCallback(IBrotliOutput callback){
            state.RemoveOutputCallback(callback);
        }

        // Reader

        private WindowSize ReadHeader(){
            return WindowSize.Deserialize(reader, NoContext.Value);
        }

        public MetaBlock? NextMetaBlock(){
            if (isAtEnd){
                return null;
            }

            var (metaBlock, isLast) = MetaBlock.Deserialize(reader, state);

            isAtEnd = isLast;
            return metaBlock;
        }
    }
}
