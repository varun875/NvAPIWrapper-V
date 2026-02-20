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
            if (powerLimitInWatts <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(powerLimitInWatts));
            }

            estimatedPowerUsageInWatts = 0;

            if (!TryGetPowerUsageInPercent(domain, out var powerUsageInPercent))
            {
                return false;
            }

            estimatedPowerUsageInWatts = powerLimitInWatts * powerUsageInPercent / 100d;
            return true;
        }

        /// <summary>
        ///     Tries to automatically estimate board power usage in watts using the built-in GPU TDP database.
        ///     No manual TDP value required — the GPU's power specification is looked up automatically.
        /// </summary>
        /// <param name="estimatedPowerUsageInWatts">Estimated board power usage in watts when available.</param>
        /// <returns>True when both telemetry and TDP are available; otherwise false.</returns>
        public bool TryGetBoardPowerUsageInWatts(out double estimatedPowerUsageInWatts)
        {
            estimatedPowerUsageInWatts = 0;

            var tdp = GPUPowerSpecDatabase.GetDefaultTDP(PhysicalGPU.FullName);

            if (!tdp.HasValue)
            {
                return false;
            }

            return TryGetEstimatedPowerUsageInWatts(PowerTopologyDomain.Board, tdp.Value, out estimatedPowerUsageInWatts);
        }

        /// <summary>
        ///     Tries to automatically estimate GPU-domain power usage in watts using the built-in GPU TDP database.
        /// </summary>
        /// <param name="estimatedPowerUsageInWatts">Estimated GPU power usage in watts when available.</param>
        /// <returns>True when both telemetry and TDP are available; otherwise false.</returns>
        public bool TryGetGPUPowerUsageInWatts(out double estimatedPowerUsageInWatts)
        {
            estimatedPowerUsageInWatts = 0;

            var tdp = GPUPowerSpecDatabase.GetDefaultTDP(PhysicalGPU.FullName);

            if (!tdp.HasValue)
            {
                return false;
            }

            return TryGetEstimatedPowerUsageInWatts(PowerTopologyDomain.GPU, tdp.Value, out estimatedPowerUsageInWatts);
        }
    }
}
