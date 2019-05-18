using System;
using System.Diagnostics;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.State;
using BrotliLib.IO;

namespace BrotliImpl{
    class MetaBlockSizeTracker{
        public MetaBlock Smallest { get; private set; } = null;
        public int SmallestSize { get; private set; } = int.MaxValue;

        private readonly BrotliGlobalState initialState;

        public MetaBlockSizeTracker(BrotliGlobalState initialState){
            this.initialState = initialState;
        }

        public void Test(MetaBlock tested, string debugText = null){
            int testedSize = CountBits(tested, initialState.Clone());

            if (debugText != null){
                Debug.Write(debugText + " = " + testedSize + " bits");
            }

            if (testedSize < SmallestSize){
                if (debugText != null){
                    Debug.Write(" < " + SmallestSize + " bits (new best)");
                }

                Smallest = tested;
                SmallestSize = testedSize;
            }
            
            if (debugText != null){
                Debug.WriteLine("");
            }
        }

        public void Test(CompressedMetaBlockBuilder builder, string debugText = null){
            Test(builder.Build().MetaBlock, debugText);
        }

        public static int CountBits(MetaBlock tested, BrotliGlobalState state){
            var stream = new BitStream();

            try{
                MetaBlock.Serializer.ToBits(stream.GetWriter(), tested, state.Clone());
                return stream.Length;
            }catch(Exception ex){
                Debug.WriteLine(ex.ToString());
                return int.MaxValue;
            }
        }
    }
}
