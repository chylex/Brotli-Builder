﻿using System;
using System.Collections.Generic;
using BrotliLib.Brotli.Components.Header;

namespace BrotliLib.Brotli.Components.Data{
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

        public static bool HasExplicitDistanceCode(this DistanceInfo info){
            return info != DistanceInfo.EndsAfterLiterals && info != DistanceInfo.ImplicitCodeZero;
        }

        public static bool CanEncodeUsing(this DistanceInfo info, DistanceCode code, BrotliGlobalState state){
            switch(info){
                case DistanceInfo.EndsAfterLiterals:
                case DistanceInfo.ImplicitCodeZero:
                    return false;
                    
                case DistanceInfo.ExplicitCodeZero:
                    return code.Equals(DistanceCode.Zero);

                default:
                    return code.CanEncodeValue(state, (int)info) && !code.Equals(DistanceCode.Zero);
            }
        }

        public static int GetValue(this DistanceInfo info, BrotliGlobalState state){
            switch(info){
                case DistanceInfo.EndsAfterLiterals:
                    throw new InvalidOperationException("The command is missing a copy distance.");

                case DistanceInfo.ImplicitCodeZero:
                case DistanceInfo.ExplicitCodeZero:
                    return state.DistanceBuffer.Front;

                default:
                    return info >= DistanceInfo.FirstExactValue ? (int)info : throw new InvalidOperationException("Copy distance must be >= " + (int)DistanceInfo.FirstExactValue + ".");
            }
        }

        public static bool FindCodes(this DistanceInfo info, in DistanceParameters parameters, BrotliGlobalState state, List<DistanceCode> outputCodes){
            switch(info){
                case DistanceInfo.EndsAfterLiterals:
                case DistanceInfo.ImplicitCodeZero:
                    return false;
                    
                case DistanceInfo.ExplicitCodeZero:
                    outputCodes.Clear();
                    outputCodes.Add(DistanceCode.Zero);
                    return true;

                default:
                    DistanceCode.ForValueExcludingCodeZero(in parameters, state, (int)info, outputCodes);
                    return true;
            }
        }
    }
}
