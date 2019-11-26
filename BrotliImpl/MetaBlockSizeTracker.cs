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

        public void Test(MetaBlock tested, BrotliSerializationParameters? parameters = null, string? debugText = null){
            int testedSize = CountBits(tested, initialState.Clone(), parameters);

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

        public void Test(CompressedMetaBlockBuilder builder, BrotliSerializationParameters? parameters = null, string? debugText = null){
            Test(builder.Build().MetaBlock, parameters, debugText);
        }

        public static int CountBits(MetaBlock tested, BrotliGlobalState state, BrotliSerializationParameters? parameters = null){
            var writer = new BitWriterNull();

            try{
                MetaBlock.Serialize(writer, tested, state.Clone(), parameters ?? BrotliSerializationParameters.Default);
                return writer.Length;
            }catch(Exception ex){
                Debug.WriteLine(ex.ToString());
                return int.MaxValue;
            }
        }
    }
}
