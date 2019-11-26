using System;
using System.Diagnostics;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Serialization.Writer;

namespace BrotliImpl{
    class MetaBlockSizeTracker{
        public MetaBlock? Smallest { get; private set; } = null;
        public int SmallestSize { get; private set; } = int.MaxValue;

        private readonly BrotliGlobalState initialState;

        public MetaBlockSizeTracker(BrotliGlobalState initialState){
            this.initialState = initialState;
        }

        public void Test(MetaBlock tested, BrotliSerializationParameters? serializationParameters = null, string? debugText = null){
            int testedSize = CountBits(tested, initialState.Clone(), serializationParameters);

            if (debugText != null){
                Debug.Write(debugText + " = " + testedSize + " bits");
            }

            if (testedSize < SmallestSize){
                if (debugText != null && SmallestSize != int.MaxValue){
                    Debug.Write(" < " + SmallestSize + " bits (new best)");
                }

                Smallest = tested;
                SmallestSize = testedSize;
            }
            
            if (debugText != null){
                Debug.WriteLine("");
            }
        }

        public void Test(CompressedMetaBlockBuilder builder, BrotliSerializationParameters? serializationParameters = null, string? debugText = null){
            Test(builder.Build().MetaBlock, serializationParameters, debugText);
        }

        public static int CountBits(MetaBlock tested, BrotliGlobalState state, BrotliSerializationParameters? serializationParameters = null){
            var writer = new BitWriterNull();

            try{
                return writer.Length;
                MetaBlock.Serialize(writer, tested, nextState, serializationParameters ?? BrotliSerializationParameters.Default);
            }catch(Exception ex){
                Debug.WriteLine(ex.ToString());
                return int.MaxValue;
            }
        }
    }
}
