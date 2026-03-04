using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NvAPIWrapper.Native;
using NvAPIWrapper.Native.Exceptions;
using NvAPIWrapper.Native.General;
using NvAPIWrapper.Native.GPU;
using NvAPIWrapper.Native.Helpers;

namespace NvAPIWrapper.GPU
{
    /// <summary>
    ///     Holds information about the GPU utilization domains.
    ///     Property accesses are cached for a short window (100ms) to avoid redundant P/Invoke calls
    ///     when multiple domain properties are read in sequence.
    /// </summary>
    public class GPUUsageInformation
    {
        private const long CacheTTLMs = 100;
        private GPUUsageDomainStatus[]? _cachedDomains;
        private long _cacheTimestamp;

        internal GPUUsageInformation(PhysicalGPU physicalGPU)
        {
            PhysicalGPU = physicalGPU;
        }

        /// <summary>
        ///     Gets the Bus interface (BUS) utilization
        /// </summary>
        public GPUUsageDomainStatus? BusInterface
        {
            get => GetCachedDomains().FirstOrDefault(status => status.Domain == UtilizationDomain.BusInterface);
        }

        /// <summary>
        ///     Gets the frame buffer (FB) utilization
        /// </summary>
        public GPUUsageDomainStatus? FrameBuffer
        {
            get => GetCachedDomains().FirstOrDefault(status => status.Domain == UtilizationDomain.FrameBuffer);
        }

        /// <summary>
        ///     Gets the graphic engine (GPU) utilization
        /// </summary>
        public GPUUsageDomainStatus? GPU
        {
            get => GetCachedDomains().FirstOrDefault(status => status.Domain == UtilizationDomain.GPU);
        }

        /// <summary>
        ///     Gets a boolean value indicating if the dynamic performance states is enabled
        /// </summary>
        public bool IsDynamicPerformanceStatesEnabled
        {
            get => GPUApi.GetDynamicPerformanceStatesInfoEx(PhysicalGPU.Handle).IsDynamicPerformanceStatesEnabled;
        }

        /// <summary>
        ///     Gets the physical GPU that this instance describes
        /// </summary>
        public PhysicalGPU PhysicalGPU { get; }

        /// <summary>
        ///     Gets all valid utilization domains and information.
        ///     Results are cached for 100ms to avoid redundant NVAPI calls.
        /// </summary>
        public IEnumerable<GPUUsageDomainStatus> UtilizationDomainsStatus
        {
            get => GetCachedDomains();
        }

        /// <summary>
        ///     Gets the Video engine (VID) utilization
        /// </summary>
        public GPUUsageDomainStatus? VideoEngine
        {
            get => GetCachedDomains().FirstOrDefault(status => status.Domain == UtilizationDomain.VideoEngine);
        }

        /// <summary>
        ///     Tries to get utilization domains without throwing for unsupported GPUs/drivers.
        /// </summary>
        /// <param name="domainsStatus">Utilization domains when available.</param>
        /// <returns>True when telemetry is available; otherwise false.</returns>
        public bool TryGetUtilizationDomainsStatus(out GPUUsageDomainStatus[] domainsStatus)
        {
            try
            {
                domainsStatus = GetCachedDomains();
                return true;
            }
            catch (NVIDIANotSupportedException)
            {
                domainsStatus = Array.Empty<GPUUsageDomainStatus>();
                return false;
            }
            catch (NVIDIAApiException ex) when (NvApiStatusHelper.IsCapabilityUnavailable(ex.Status))
            {
                domainsStatus = Array.Empty<GPUUsageDomainStatus>();
                return false;
            }
        }

        /// <summary>
        ///     Tries to get dynamic performance state enablement without throwing for unsupported GPUs/drivers.
        /// </summary>
        /// <param name="isEnabled">True if dynamic performance states are enabled.</param>
        /// <returns>True when the capability is available; otherwise false.</returns>
        public bool TryGetDynamicPerformanceStatesEnabled(out bool isEnabled)
        {
            try
            {
                isEnabled = IsDynamicPerformanceStatesEnabled;
                return true;
            }
            catch (NVIDIANotSupportedException)
            {
                isEnabled = false;
                return false;
            }
            catch (NVIDIAApiException ex) when (NvApiStatusHelper.IsCapabilityUnavailable(ex.Status))
            {
                isEnabled = false;
                return false;
            }
        }

        /// <summary>
        ///     Enables dynamic performance states
        /// </summary>
        public void EnableDynamicPerformanceStates()
        {
            GPUApi.EnableDynamicPStates(PhysicalGPU.Handle);
        }

        /// <summary>
        ///     Invalidates the internal utilization cache, forcing the next access to query NVAPI.
        /// </summary>
        public void InvalidateCache()
        {
            _cachedDomains = null;
            _cacheTimestamp = 0;
        }

        private GPUUsageDomainStatus[] GetCachedDomains()
        {
            var now = Stopwatch.GetTimestamp();
            var elapsedMs = (now - _cacheTimestamp) * 1000L / Stopwatch.Frequency;

            if (_cachedDomains != null && elapsedMs < CacheTTLMs)
            {
                return _cachedDomains;
            }

            _cachedDomains = FetchDomains();
            _cacheTimestamp = now;
            return _cachedDomains;
        }

        private GPUUsageDomainStatus[] FetchDomains()
        {
            try
            {
                var dynamicPerformanceStates = GPUApi.GetDynamicPerformanceStatesInfoEx(PhysicalGPU.Handle);

                if (dynamicPerformanceStates.IsDynamicPerformanceStatesEnabled)
                {
                    return dynamicPerformanceStates.Domains
                        .Select(pair => new GPUUsageDomainStatus(pair.Key, pair.Value))
                        .ToArray();
                }
            }
            catch
            {
                // ignored
            }

            return GPUApi.GetUsages(PhysicalGPU.Handle).Domains
                .Select(pair => new GPUUsageDomainStatus(pair.Key, pair.Value))
                .ToArray();
        }
    }
}
