using System;
using BrotliLib.Brotli.Components.Data;

namespace BrotliLib.Brotli.Components.Utils{
    /// <summary>
    /// Determines how to set <see cref="InsertCopyLengthCode.UseDistanceCodeZero"/> during construction.
    /// </summary>
    public enum DistanceCodeZeroStrategy{
        /// <summary>
        /// Always sets <see cref="InsertCopyLengthCode.UseDistanceCodeZero"/> to <code>false</code>.
        /// </summary>
        Disable,

        /// <summary>
        /// Sets <see cref="InsertCopyLengthCode.UseDistanceCodeZero"/> to <code>true</code> if possible with the provided insert & copy codes, <code>false</code> otherwise.
        /// </summary>
        PreferEnabled,

        /// <summary>
        /// Sets <see cref="InsertCopyLengthCode.UseDistanceCodeZero"/> to <code>true</code> if possible with the provided insert & copy codes, throws <see cref="ArgumentOutOfRangeException"/> otherwise.
        /// </summary>
        ForceEnabled
    }

    internal static class DistanceCodeZeroStrategies{
        public static bool Determine(this DistanceCodeZeroStrategy strategy, int insertCode, int copyCode){
            return strategy switch{
                DistanceCodeZeroStrategy.Disable
                => false,

                DistanceCodeZeroStrategy.PreferEnabled
                => insertCode <= 7 && copyCode <= 15,

                DistanceCodeZeroStrategy.ForceEnabled when insertCode > 7
                => throw new ArgumentOutOfRangeException(nameof(insertCode), "Insert code must be in the range [0; 7] when using implied distance code zero."),

                DistanceCodeZeroStrategy.ForceEnabled when copyCode > 15
                => throw new ArgumentOutOfRangeException(nameof(copyCode), "Copy code must be in the range [0; 15] when using implied distance code zero."),

                DistanceCodeZeroStrategy.ForceEnabled
                => true,

                _ => throw new InvalidOperationException("Invalid distance code zero strategy: " + strategy),
            };
        }
    }
}
