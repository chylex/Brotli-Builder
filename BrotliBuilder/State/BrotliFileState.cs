using System;
using System.Diagnostics;
using System.Linq;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Output;
using BrotliLib.Markers;
using BrotliLib.Serialization;

namespace BrotliBuilder.State{
    abstract class BrotliFileState{
        public abstract class TimedBrotliFileState : BrotliFileState{
            public Stopwatch? Stopwatch { get; }

            protected TimedBrotliFileState(Stopwatch? stopwatch){
                this.Stopwatch = stopwatch;
            }
        }

        // Concrete states

        public sealed class NoFile : BrotliFileState{}
        public sealed class Waiting : BrotliFileState{}
        public sealed class Starting : BrotliFileState{}

        public sealed class HasStructure : TimedBrotliFileState{
            public BrotliFileStructure File { get; }

            public HasStructure(BrotliFileStructure file, Stopwatch? stopwatch) : base(stopwatch){
                this.File = file;
            }
        }

        public sealed class HasBits : TimedBrotliFileState{
            public string Bits { get; }

            public HasBits(string bits, Stopwatch? stopwatch) : base(stopwatch){
                this.Bits = bits;
            }
        }

        public sealed class HasOutput : TimedBrotliFileState{
            public byte[]? PreviousBytes { get; }
            public byte[] OutputBytes { get; }

            public HasOutput(byte[]? previousBytes, byte[] outputBytes, Stopwatch? stopwatch) : base(stopwatch){
                this.PreviousBytes = previousBytes;
                this.OutputBytes = outputBytes;
            }
        }

        public sealed class Loaded : BrotliFileState{
            public BrotliFileStructure File { get; }
            public MarkerRoot MarkerRoot { get; }
            public MarkerNode[] Markers { get; }

            public int TotalCompressedBits { get; }
            public int TotalOutputBytes { get; }

            public Loaded(BrotliFileStructure file, BitStream bits, BrotliOutputStored output){
                this.File = file;
                this.MarkerRoot = output.MarkerRoot;
                this.Markers = output.MarkerRoot.ToArray();

                this.TotalCompressedBits = Markers.LastOrDefault()?.Marker?.IndexEnd ?? bits.Length; // use markers to account for padding whenever possible
                this.TotalOutputBytes = output.OutputSize;
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
