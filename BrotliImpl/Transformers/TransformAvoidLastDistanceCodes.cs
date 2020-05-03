using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.Encode.Build;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Brotli.Parameters.Heuristics;
using BrotliLib.Collections;

namespace BrotliImpl.Transformers{
    public class TransformAvoidLastDistanceCodes : BrotliTransformerCompressed{
        protected override (MetaBlock, BrotliGlobalState) Transform(MetaBlock.Compressed original, BrotliGlobalState state, BrotliCompressionParameters parameters){
            var builder = new CompressedMetaBlockBuilder(original, state);
            var tracker = new MetaBlockSizeTracker(state);
            tracker.Test(builder, parameters);

            var blocker = new Blocker(parameters.DistanceCodePicker);

            parameters = new BrotliCompressionParameters.Builder(parameters){
                DistanceCodePicker = blocker.Pick
            }.Build();

            foreach(var code in DistanceCode.Last.Codes.Except(new DistanceCode[]{ DistanceCode.Zero })){
                var prev = tracker.SmallestSize;
                
                blocker.BlockedCodes.Add(code);
                tracker.Test(builder, parameters);

                if (tracker.SmallestSize < prev){
                    Debug.WriteLine("Blocking code " + code + " reduced size (" + prev + " > " + tracker.SmallestSize + "), keeping it...");
                }
                else{
                    Debug.WriteLine("Blocking code " + code + " did not improve the size, continuing...");
                    blocker.BlockedCodes.Remove(code);
                }
            }

            Debug.WriteLine("Final blocked codes: " + string.Join(", ", blocker.BlockedCodes));
            
            return tracker.Smallest ?? throw new InvalidOperationException("Transformation did not generate any meta-blocks.");
        }

        private class Blocker{
            public HashSet<DistanceCode> BlockedCodes { get; } = new HashSet<DistanceCode>();

            private readonly PickCodeHeuristics<DistanceCode>.Picker originalPicker;

            public Blocker(PickCodeHeuristics<DistanceCode>.Picker originalPicker){
                this.originalPicker = originalPicker;
            }

            public DistanceCode Pick(List<DistanceCode> picks, FrequencyList<DistanceCode> previouslySeen){
                picks.RemoveAll(BlockedCodes.Contains);
                return originalPicker(picks, previouslySeen);
            }
        }
    }
}
