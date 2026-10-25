using System;
using System.Collections.Generic;
using System.Linq;
using NvAPIWrapper.Native;
using NvAPIWrapper.Native.GPU;

namespace NvAPIWrapper.GPU
{
    /// <summary>
    ///     Holds information regarding current power topology and their current power usage.
    ///     Provides both percentage-based and automatic watt-based power readings.
    /// </summary>
    public class GPUPowerTopologyInformation
    {
        internal GPUPowerTopologyInformation(PhysicalGPU physicalGPU)
        {
            PhysicalGPU = physicalGPU;
        }

        /// <summary>
        ///     Gets the physical GPU that this instance describes
        /// </summary>
        public PhysicalGPU PhysicalGPU { get; }

        /// <summary>
        ///     Gets the current power topology entries
        /// </summary>
        public IEnumerable<GPUPowerTopologyStatus> PowerTopologyEntries
        {
            get
            {
                return GPUApi.ClientPowerTopologyGetStatus(PhysicalGPU.Handle).PowerPolicyStatusEntries
                    .Select(entry => new GPUPowerTopologyStatus(entry));
            }
        }

        /// <summary>
        ///     Tries to get current power topology entries without throwing for unsupported GPUs/drivers.
        /// </summary>
        /// <param name="entries">Topology entries when available.</param>
        /// <returns>True when telemetry is available; otherwise false.</returns>
        public bool TryGetPowerTopologyEntries(out GPUPowerTopologyStatus[] entries)
        {
            if (!GPUApi.TryClientPowerTopologyGetStatus(PhysicalGPU.Handle, out var topologyStatus))
            {
                entries = Array.Empty<GPUPowerTopologyStatus>();
                return false;
            }

            entries = topologyStatus.PowerPolicyStatusEntries
                .Select(entry => new GPUPowerTopologyStatus(entry))
                .ToArray();
            return true;
        }

        /// <summary>
        ///     Tries to get current power usage percentage for a specific topology domain.
        /// </summary>
        /// <param name="domain">Power topology domain (e.g. GPU/Board).</param>
        /// <param name="powerUsageInPercent">Current usage in percent when available.</param>
        /// <returns>True when telemetry is available; otherwise false.</returns>
        public bool TryGetPowerUsageInPercent(PowerTopologyDomain domain, out float powerUsageInPercent)
        {
            powerUsageInPercent = 0;

            if (!TryGetPowerTopologyEntries(out var entries))
            {
                return false;
            }

            var entry = entries.FirstOrDefault(status => status.Domain == domain);

            if (entry == null)
            {
                return false;
            }

            powerUsageInPercent = entry.PowerUsageInPercent;
            return true;
        }

        /// <summary>
        ///     Tries to estimate power usage in watts for a specific domain from NVAPI percentage telemetry.
        /// </summary>
        /// <param name="domain">Power topology domain to estimate.</param>
        /// <param name="powerLimitInWatts">
        ///     Reference power limit in watts (for example board TGP for board domain).
        /// </param>
        /// <param name="estimatedPowerUsageInWatts">Estimated power usage in watts when available.</param>
        /// <returns>True when telemetry is available; otherwise false.</returns>
        public bool TryGetEstimatedPowerUsageInWatts(
            PowerTopologyDomain domain,
            double powerLimitInWatts,
            out double estimatedPowerUsageInWatts)
        {
            GPUPowerMath.ValidateReference(powerLimitInWatts, nameof(powerLimitInWatts));

            estimatedPowerUsageInWatts = 0;

            if (!TryGetPowerUsageInPercent(domain, out var powerUsageInPercent))
            {
                return false;
            }

            return GPUPowerMath.TryEstimate(powerUsageInPercent, powerLimitInWatts, out estimatedPowerUsageInWatts);
        }

        /// <summary>
        ///     Tries to get board power in watts using the same driver-first rules as a power snapshot.
        ///     Use the snapshot's BoardPower property when source and estimation metadata are needed.
        /// </summary>
        /// <param name="estimatedPowerUsageInWatts">Estimated board power usage in watts when available.</param>
        /// <returns>True when a direct or driver-reference-based watt reading is available.</returns>
        public bool TryGetBoardPowerUsageInWatts(out double estimatedPowerUsageInWatts)
        {
            return TryGetBoardPowerUsageInWatts(GPUPowerTelemetryOptions.Default, out estimatedPowerUsageInWatts);
        }

        /// <summary>Gets board watts using explicitly configured power-reporting options.</summary>
        /// <param name="options">Controls power reference and optional database estimates.</param>
        /// <param name="powerUsageInWatts">Available board power, or zero when the method returns false.</param>
        /// <returns>True when a direct or estimated watt reading is available.</returns>
        public bool TryGetBoardPowerUsageInWatts(GPUPowerTelemetryOptions options, out double powerUsageInWatts)
        {
            powerUsageInWatts = 0;

            if (!PhysicalGPU.TryGetPowerTelemetrySnapshot(options, out var snapshot) || !snapshot!.BoardPowerDrawInWatts.HasValue)
            {
                return false;
            }

            powerUsageInWatts = snapshot.BoardPowerDrawInWatts.Value;
            return true;
        }

        /// <summary>
        ///     Tries to get GPU-domain watts. A board TDP cannot establish a GPU-domain reference,
        ///     so use TryGetEstimatedPowerUsageInWatts with an explicit reference, or snapshot options.
        /// </summary>
        /// <param name="estimatedPowerUsageInWatts">Estimated GPU power usage in watts when available.</param>
        /// <returns>True when both telemetry and TDP are available; otherwise false.</returns>
        public bool TryGetGPUPowerUsageInWatts(out double estimatedPowerUsageInWatts)
        {
            return TryGetGPUPowerUsageInWatts(GPUPowerTelemetryOptions.Default, out estimatedPowerUsageInWatts);
        }

        /// <summary>Gets GPU-domain watts using an explicit domain reference in the reporting options.</summary>
        /// <param name="options">Must supply a GPU-domain reference for watt estimation.</param>
        /// <param name="powerUsageInWatts">Estimated power, or zero when the method returns false.</param>
        /// <returns>True when both GPU-domain telemetry and a domain reference are available.</returns>
        public bool TryGetGPUPowerUsageInWatts(GPUPowerTelemetryOptions options, out double powerUsageInWatts)
        {
            powerUsageInWatts = 0;
            if (!PhysicalGPU.TryGetPowerTelemetrySnapshot(options, out var snapshot) || !snapshot!.GPUPowerDrawInWatts.HasValue)
            {
                return false;
            }

            powerUsageInWatts = snapshot.GPUPowerDrawInWatts.Value;
            return true;
        }
    }
}
