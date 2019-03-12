﻿using System;
using BrotliLib.Brotli.Components.Data;

namespace BrotliLib.Brotli.Components.Utils{
    /// <summary>
    /// Represents either an exact copy distance value, or a value with special meaning used to determine the distance code.
    /// </summary>
    public enum DistanceInfo{
        /// <summary>
        /// The command contains no copy distance.
        /// </summary>
        EndsAfterLiterals = -3,

        /// <summary>
        /// Represents distance code zero, which is encoded into the <see cref="InsertCopyLengthCode"/> and must not be written into the stream.
        /// </summary>
        ImplicitCodeZero = -2,

        /// <summary>
        /// Represents distance code zero, which must be written into the stream using a <see cref="DistanceCode"/> where <see cref="DistanceCode.Code"/> equals 0.
        /// </summary>
        ExplicitCodeZero = -1,

        /// <summary>
        /// All positive values of the enum (including zero) represent the actual values of the copy distance.
        /// </summary>
        FirstExactValue = 0
    }

    public static class DistanceInfos{
        public static bool ShouldWriteToDistanceBuffer(this DistanceInfo info){
            return info >= DistanceInfo.FirstExactValue;
        }

        public static bool CanEncodeUsing(this DistanceInfo info, DistanceCode code, BrotliGlobalState state){
            switch(info){
                case DistanceInfo.EndsAfterLiterals:
                case DistanceInfo.ImplicitCodeZero:
                    return false;
                    
                case DistanceInfo.ExplicitCodeZero:
                    return code.Code == 0;

                default:
                    return code.CanEncodeValue(state, (int)info);
            }
        }

        public static int GetValue(this DistanceInfo info, BrotliGlobalState state){
            switch(info){
                case DistanceInfo.EndsAfterLiterals:
                    throw new InvalidOperationException("The command is missing a copy distance.");

                case DistanceInfo.ExplicitCodeZero:
                case DistanceInfo.ImplicitCodeZero:
                    return state.DistanceBuffer.Front;

                default:
                    return info >= DistanceInfo.FirstExactValue ? (int)info : throw new InvalidOperationException("Copy distance must be >= " + (int)DistanceInfo.FirstExactValue + ".");
            }
        }
    }
}