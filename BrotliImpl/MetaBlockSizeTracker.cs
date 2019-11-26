using System;
using System.Diagnostics;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Encode.Build;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Serialization.Writer;

namespace BrotliImpl{
    class MetaBlockSizeTracker{
        public (MetaBlock, BrotliGlobalState)? Smallest { get; private set; } = null;
        public int SmallestSize { get; private set; } = int.MaxValue;

        private readonly BrotliGlobalState initialState;

        public MetaBlockSizeTracker(BrotliGlobalState initialState){
            this.initialState = initialState;
        }

        public void Test(MetaBlock tested, BrotliSerializationParameters? serializationParameters = null, string? debugText = null){
            var (testedSize, nextState) = CountBits(tested, initialState.Clone(), serializationParameters) ?? (int.MaxValue, null!);

            if (debugText != null){
                Debug.Write(debugText + " = " + testedSize + " bits");
            }

            if (testedSize < SmallestSize){
                if (debugText != null && SmallestSize != int.MaxValue){
                    Debug.Write(" < " + SmallestSize + " bits (new best)");
                }

                Smallest = (tested, nextState);
                SmallestSize = testedSize;
            }
            
            if (debugText != null){
                Debug.WriteLine("");
            }
        }

        public void Test(CompressedMetaBlockBuilder builder, BrotliCompressionParameters compressionParameters, BrotliSerializationParameters? serializationParameters = null, string? debugText = null){
            Test(builder.Build(compressionParameters).MetaBlock, serializationParameters, debugText);
        }

        public static (int, BrotliGlobalState)? CountBits(MetaBlock tested, BrotliGlobalState state, BrotliSerializationParameters? serializationParameters = null){
            var writer = new BitWriterNull();
            var nextState = state.Clone();

            try{
                MetaBlock.Serialize(writer, tested, nextState, serializationParameters ?? BrotliSerializationParameters.Default);
                return (writer.Length, nextState);
            }catch(Exception ex){
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }
    }
}
