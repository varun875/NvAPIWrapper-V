using System;
using System.Collections.Generic;
using System.Linq;
using NvAPIWrapper.Native;
using NvAPIWrapper.Native.Exceptions;
using NvAPIWrapper.Native.General;
using NvAPIWrapper.Native.GPU;

namespace NvAPIWrapper.GPU
{
    /// <summary>
    ///     Holds information about the GPU utilization domains
    /// </summary>
    public class GPUUsageInformation
    {
        internal GPUUsageInformation(PhysicalGPU physicalGPU)
        {
            PhysicalGPU = physicalGPU;
        }

        /// <summary>
        ///     Gets the Bus interface (BUS) utilization
        /// </summary>
        public GPUUsageDomainStatus BusInterface
        {
            get => UtilizationDomainsStatus.FirstOrDefault(status => status.Domain == UtilizationDomain.BusInterface);
        }

        /// <summary>
        ///     Gets the frame buffer (FB) utilization
        /// </summary>
        public GPUUsageDomainStatus FrameBuffer
        {
            get => UtilizationDomainsStatus.FirstOrDefault(status => status.Domain == UtilizationDomain.FrameBuffer);
        }

        /// <summary>
        ///     Gets the graphic engine (GPU) utilization
        /// </summary>
        public GPUUsageDomainStatus GPU
        {
            get => UtilizationDomainsStatus.FirstOrDefault(status => status.Domain == UtilizationDomain.GPU);
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
        ///     Gets all valid utilization domains and information
        /// </summary>
        public IEnumerable<GPUUsageDomainStatus> UtilizationDomainsStatus
        {
            get
            {
                try
                {
                    var dynamicPerformanceStates = GPUApi.GetDynamicPerformanceStatesInfoEx(PhysicalGPU.Handle);

                    if (dynamicPerformanceStates.IsDynamicPerformanceStatesEnabled)
                    {
                        return dynamicPerformanceStates.Domains
                            .Select(pair => new GPUUsageDomainStatus(pair.Key, pair.Value));
                    }
                }
                catch
                {
                    // ignored
                }

                return GPUApi.GetUsages(PhysicalGPU.Handle).Domains
                    .Select(pair => new GPUUsageDomainStatus(pair.Key, pair.Value));
            }
        }


        /// <summary>
        ///     Gets the Video engine (VID) utilization
        /// </summary>
        public GPUUsageDomainStatus VideoEngine
        {
            get => UtilizationDomainsStatus.FirstOrDefault(status => status.Domain == UtilizationDomain.VideoEngine);
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
                domainsStatus = UtilizationDomainsStatus.ToArray();
                return true;
            }
            catch (NVIDIANotSupportedException)
            {
                domainsStatus = Array.Empty<GPUUsageDomainStatus>();
                return false;
            }
            catch (NVIDIAApiException ex) when (IsCapabilityUnavailableStatus(ex.Status))
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
            catch (NVIDIAApiException ex) when (IsCapabilityUnavailableStatus(ex.Status))
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

        private static bool IsCapabilityUnavailableStatus(Status status)
        {
            return status == Status.NotSupported ||
                   status == Status.NoImplementation ||
                   status == Status.FunctionNotFound;
        }
    }
}

