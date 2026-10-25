using System;
using System.Globalization;

namespace NvAPIWrapper.GPU
{
    /// <summary>Identifies the source of a watt value, or of the reference used to estimate it.</summary>
    public enum GPUPowerDataSource
    {
        /// <summary>No value is available.</summary>
        Unavailable,
        /// <summary>Read from NVIDIA Management Library for the matched physical device.</summary>
        NVML,
        /// <summary>Explicitly supplied by the application.</summary>
        UserProvided,
        /// <summary>Unverified model-level reference data; never a direct measurement.</summary>
        ReferenceDatabase
    }

    /// <summary>
    ///     An immutable watt value with provenance. For estimates, Source identifies the reference baseline;
    ///     the percentage comes from NVAPI. NVML power usage has device-dependent sampling/averaging semantics.
    /// </summary>
    public sealed class GPUPowerValue
    {
        internal static GPUPowerValue Unavailable { get; } = new GPUPowerValue(null, GPUPowerDataSource.Unavailable, false);

        internal GPUPowerValue(double? watts, GPUPowerDataSource source, bool isEstimated)
        {
            if (watts.HasValue && (!double.IsFinite(watts.Value) || watts.Value < 0))
            {
                throw new ArgumentOutOfRangeException(nameof(watts));
            }

            Watts = watts;
            Source = watts.HasValue ? source : GPUPowerDataSource.Unavailable;
            IsEstimated = watts.HasValue && isEstimated;
        }

        /// <summary>Gets watts, or null if this reading is unavailable. Zero is a valid reading.</summary>
        public double? Watts { get; }

        /// <summary>Gets whether a watt value is available.</summary>
        public bool IsAvailable => Watts.HasValue;

        /// <summary>Gets the data source, or the reference source for an estimate.</summary>
        public GPUPowerDataSource Source { get; }

        /// <summary>Gets whether watts are estimated rather than read directly from the driver.</summary>
        public bool IsEstimated { get; }

        /// <inheritdoc />
        public override string ToString() => Watts.HasValue
            ? $"{(IsEstimated ? "~" : "")}{Watts.Value.ToString("F2", CultureInfo.InvariantCulture)} W ({Source})"
            : "N/A";
    }
}
