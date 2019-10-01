using System;
using System.Diagnostics;
using System.Linq;
using BrotliLib.Brotli;
using BrotliLib.Brotli.State.Output;
using BrotliLib.IO;
using BrotliLib.Markers;

namespace BrotliBuilder.State{
    abstract class BrotliFileState{
        public abstract class TimedBrotliFileState : BrotliFileState{
            public Stopwatch Stopwatch { get; }

            protected TimedBrotliFileState(Stopwatch stopwatch){
                this.Stopwatch = stopwatch;
            }
        }

        // Concrete states

        public sealed class NoFile : BrotliFileState{}
        public sealed class Waiting : BrotliFileState{}
        public sealed class Starting : BrotliFileState{}

        public sealed class HasStructure : TimedBrotliFileState{
            public BrotliFileStructure File { get; }

            public HasStructure(BrotliFileStructure file, Stopwatch stopwatch) : base(stopwatch){
                this.File = file;
            }
        }

        public sealed class HasBits : TimedBrotliFileState{
            public string Bits { get; }

            public HasBits(string bits, Stopwatch stopwatch) : base(stopwatch){
                this.Bits = bits;
            }
        }

        public sealed class Loaded : TimedBrotliFileState{
            public BrotliFileStructure File { get; }
            public MarkerNode[] Markers { get; }
            public string OutputText { get; }
            public int TotalBits { get; }

            public Loaded(BrotliFileStructure file, BitStream bits, BrotliOutputStored output, Stopwatch stopwatch) : base(stopwatch){
                this.File = file;
                this.Markers = output.BitMarkerRoot.ToArray();
                this.OutputText = output.AsUTF8;
                this.TotalBits = Markers.LastOrDefault()?.Marker?.IndexEnd ?? bits.Length; // use markers to account for padding whenever possible
            }
        }

        public sealed class Error : BrotliFileState{
            public ErrorType Type { get; }
            public Exception Exception { get; }

            public Error(ErrorType type, Exception exception){
                this.Type = type;
                this.Exception = exception;
            }
        }
    }
}
