using System;
using NvAPIWrapper.Native.GPU;
using NvAPIWrapper.Native.GPU.Structures;

namespace NvAPIWrapper.GPU
{
    /// <summary>
    ///     Contains information about a power domain usage
    /// </summary>
    public class GPUPowerTopologyStatus
    {
        internal GPUPowerTopologyStatus(
            PrivatePowerTopologiesStatusV1.PowerTopologiesStatusEntry powerTopologiesStatusEntry)
        {
            Domain = powerTopologiesStatusEntry.Domain;
            PowerUsageInPCM = powerTopologiesStatusEntry.PowerUsageInPCM;
        }

        /// <summary>
        ///     Gets the power usage domain
        /// </summary>
        public PowerTopologyDomain Domain { get; }

        /// <summary>
        ///     Gets the current power usage in per cent mille (PCM).
        ///     A value of 100000 PCM = 100% of the current power limit.
        /// </summary>
        public uint PowerUsageInPCM { get; }

        /// <summary>
        ///     Gets the current power usage in percentage (0-100+).
        ///     100% means the GPU is at its current power limit.
        ///     Values above 100% indicate transient power excursion.
        /// </summary>
        public float PowerUsageInPercent
        {
            get => PowerUsageInPCM / 1000f;
        }

        /// <summary>
        ///     Estimates current power usage in watts from a known reference power limit.
        /// </summary>
        /// <param name="powerLimitInWatts">
        ///     Reference power limit in watts (for example board TGP for board domain).
        /// </param>
        /// <returns>Estimated current power usage in watts.</returns>
        public double EstimatePowerUsageInWatts(double powerLimitInWatts)
        {
            if (powerLimitInWatts <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(powerLimitInWatts));
            }

            return powerLimitInWatts * PowerUsageInPercent / 100d;
        }

        /// <summary>
        ///     Estimates current power usage in watts using the built-in GPU TDP database.
        ///     Returns null if the GPU is not found in the database.
        /// </summary>
        /// <param name="gpuFullName">The GPU full name from NVAPI (e.g. "NVIDIA GeForce RTX 4090").</param>
        /// <returns>Estimated power in watts, or null if the GPU TDP is unknown.</returns>
        public double? EstimatePowerUsageInWattsAuto(string gpuFullName)
        {
            var tdp = GPUPowerSpecDatabase.GetDefaultTDP(gpuFullName);

            if (!tdp.HasValue)
            {
                return null;
            }

            return tdp.Value * PowerUsageInPercent / 100d;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{Domain}] {PowerUsageInPercent:F1}%";
        }
    }
}
