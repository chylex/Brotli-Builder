using System;
using BrotliLib.Brotli.Components.Data;

namespace BrotliLib.Brotli.Utils{
    /// <summary>
    /// Determines how to pick <see cref="DistanceInfo"/> for an insert&amp;copy command whose distance can be encoded using <see cref="DistanceCode.Zero"/>.
    /// </summary>
    public enum DistanceCodeZeroStrategy{
        /// <summary>
        /// If possible, uses an implicit code zero encoded in <see cref="InsertCopyLengthCode.UseDistanceCodeZero"/>.
        /// Otherwise, throws <see cref="InvalidOperationException"/>.
        /// </summary>
        ForceImplicit,

        /// <summary>
        /// If possible, uses an implicit code zero encoded in <see cref="InsertCopyLengthCode.UseDistanceCodeZero"/>.
        /// Otherwise, uses an explicit <see cref="DistanceCode.Zero"/>.
        /// </summary>
        PreferImplicit,

        /// <summary>
        /// Uses an explicit <see cref="DistanceCode.Zero"/>.
        /// </summary>
        Explicit,

        /// <summary>
        /// Uses a non-zero <see cref="DistanceCode"/>.
        /// </summary>
        Avoid
    }
    
    internal static class DistanceCodeZeroStrategyExtensions{
        public static DistanceInfo Decide(this DistanceCodeZeroStrategy strategy, int insertLength, int copyLength, int copyDistance){
            return strategy switch{
                DistanceCodeZeroStrategy.ForceImplicit  => InsertCopyLengths.CanUseImplicitDCZ(insertLength, copyLength) ? DistanceInfo.ImplicitCodeZero : throw new InvalidOperationException("Cannot use implicit distance code zero (insert length " + insertLength + ", copy length " + copyLength + ")."),
                DistanceCodeZeroStrategy.PreferImplicit => InsertCopyLengths.CanUseImplicitDCZ(insertLength, copyLength) ? DistanceInfo.ImplicitCodeZero : DistanceInfo.ExplicitCodeZero,
                DistanceCodeZeroStrategy.Explicit       => DistanceInfo.ExplicitCodeZero,
                DistanceCodeZeroStrategy.Avoid          => (DistanceInfo)copyDistance,
                _                                       => throw new InvalidOperationException("Invalid distance code zero strategy: " + strategy),
            };
        }
    }
}
