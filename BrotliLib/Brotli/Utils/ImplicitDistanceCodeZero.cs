using System;
using BrotliLib.Brotli.Components.Data;

namespace BrotliLib.Brotli.Utils{
    /// <summary>
    /// Determines how to set <see cref="InsertCopyLengthCode.UseDistanceCodeZero"/> during construction.
    /// </summary>
    public enum ImplicitDistanceCodeZero{
        /// <summary>
        /// Always sets <see cref="InsertCopyLengthCode.UseDistanceCodeZero"/> to <c>false</c>.
        /// </summary>
        Disable,

        /// <summary>
        /// Sets <see cref="InsertCopyLengthCode.UseDistanceCodeZero"/> to <c>true</c> if possible with the provided insert & copy codes, <c>false</c> otherwise.
        /// </summary>
        PreferEnabled,

        /// <summary>
        /// Sets <see cref="InsertCopyLengthCode.UseDistanceCodeZero"/> to <c>true</c> if possible with the provided insert & copy codes, throws <see cref="ArgumentOutOfRangeException"/> otherwise.
        /// </summary>
        ForceEnabled
    }

    internal static class ImplicitDistanceCodeZeroExtensions{
        public static bool Decide(this ImplicitDistanceCodeZero strategy, int insertCode, int copyCode){
            return strategy switch{
                ImplicitDistanceCodeZero.Disable
                => false,

                ImplicitDistanceCodeZero.PreferEnabled
                => insertCode <= 7 && copyCode <= 15,

                ImplicitDistanceCodeZero.ForceEnabled when insertCode > 7
                => throw new ArgumentOutOfRangeException(nameof(insertCode), "Insert code must be in the range [0; 7] when using implied distance code zero."),

                ImplicitDistanceCodeZero.ForceEnabled when copyCode > 15
                => throw new ArgumentOutOfRangeException(nameof(copyCode), "Copy code must be in the range [0; 15] when using implied distance code zero."),

                ImplicitDistanceCodeZero.ForceEnabled
                => true,

                _ => throw new InvalidOperationException("Invalid implicit distance code zero strategy: " + strategy),
            };
        }
    }
}
