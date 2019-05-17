﻿using System.Linq;
using BrotliLib.Brotli.State;
using BrotliLib.IO;

namespace BrotliLib.Brotli.Components.Data{
    public abstract partial class DistanceCode{
        /// <inheritdoc />
        /// <summary>
        /// Represents a distance code which uses the global ring buffer of last distances.
        /// </summary>
        private sealed class Last : DistanceCode{
            private static readonly byte[] BufferIndexes ={
                3, 2, 1, 0,
                3, 3, 3, 3, 3, 3,
                2, 2, 2, 2, 2, 2
            };

            private static readonly sbyte[] BufferValueOffsets ={
                0, 0, 0, 0,
                -1, 1, -2, 2, -3, 3,
                -1, 1, -2, 2, -3, 3
            };

            public static readonly DistanceCode[] Codes = Enumerable.Range(0, BufferIndexes.Length)
                                                                    .Select(code => new Last(code))
                                                                    .ToArray<DistanceCode>();

            private readonly byte index;
            private readonly sbyte offset;

            private Last(int code) : base(code){
                this.index = BufferIndexes[code];
                this.offset = BufferValueOffsets[code];
            }

            public override bool CanEncodeValue(BrotliGlobalState state, int value){
                return value == state.DistanceBuffer[index] + offset;
            }

            protected override int ReadValue(BrotliGlobalState state, BitReader reader){
                return state.DistanceBuffer[index] + offset;
            }

            protected override void WriteValue(BrotliGlobalState state, int value, BitWriter writer){
                // no extra bits
            }

            public override string ToString(){
                return base.ToString() + " | Value = buffer[" + index + "] " + (offset < 0 ? "- " + (-offset) : "+ " + offset);
            }
        }
    }
}