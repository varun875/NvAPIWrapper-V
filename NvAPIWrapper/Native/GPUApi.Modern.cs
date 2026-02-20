using System;
using NvAPIWrapper.Native.Exceptions;
using NvAPIWrapper.Native.General;
using NvAPIWrapper.Native.GPU;
using NvAPIWrapper.Native.GPU.Structures;
using NvAPIWrapper.Native.Helpers;
using NvAPIWrapper.Native.Helpers.Structures;

namespace NvAPIWrapper.Native
{
    /// <summary>
    ///     Modern GPU/system APIs (R530+ / R550+) with compatibility fallbacks.
    /// </summary>
    public static partial class GPUApi
    {
        /// <summary>
        ///     Returns GSP firmware information for GPUs/drivers that support it.
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle.</param>
        /// <returns>GSP firmware information.</returns>
        /// <exception cref="NVIDIAApiException">Throws for NVAPI error statuses except capability unavailability.</exception>
        public static GSPInfoV1 GetGSPInfo(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(GSPInfoV1).Instantiate<GSPInfoV1>();

            using (var gspInfoReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<Delegates.GPU.NvAPI_GPU_GetGspFeatures>()(
                    gpuHandle,
                    gspInfoReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return gspInfoReference.ToValueType<GSPInfoV1>(typeof(GSPInfoV1));
            }
        }

        /// <summary>
        ///     Tries to get GSP firmware information without throwing for unsupported drivers/GPUs.
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle.</param>
        /// <param name="gspInfo">Returned GSP firmware information when successful.</param>
        /// <returns>True when information is available; otherwise false.</returns>
        public static bool TryGetGSPInfo(PhysicalGPUHandle gpuHandle, out GSPInfoV1 gspInfo)
        {
            try
            {
                gspInfo = GetGSPInfo(gpuHandle);
                return true;
            }
            catch (NVIDIANotSupportedException)
            {
                gspInfo = default;
                return false;
            }
            catch (NVIDIAApiException ex) when (IsCapabilityUnavailableStatus(ex.Status))
            {
                gspInfo = default;
                return false;
            }
        }

        /// <summary>
        ///     Returns NVLink capability information for the selected GPU.
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle.</param>
        /// <returns>NVLink capability information.</returns>
        /// <exception cref="NVIDIAApiException">Throws for NVAPI error statuses except capability unavailability.</exception>
        public static NVLinkGetCapsV1 GetNVLinkCaps(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(NVLinkGetCapsV1).Instantiate<NVLinkGetCapsV1>();

            using (var capsReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<Delegates.GPU.NvAPI_GPU_NVLINK_GetCaps>()(
                    gpuHandle,
                    capsReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return capsReference.ToValueType<NVLinkGetCapsV1>(typeof(NVLinkGetCapsV1));
            }
        }

        /// <summary>
        ///     Tries to get NVLink capability information without throwing for unsupported drivers/GPUs.
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle.</param>
        /// <param name="caps">Returned NVLink capability information when successful.</param>
        /// <returns>True when information is available; otherwise false.</returns>
        public static bool TryGetNVLinkCaps(PhysicalGPUHandle gpuHandle, out NVLinkGetCapsV1 caps)
        {
            try
            {
                caps = GetNVLinkCaps(gpuHandle);
                return true;
            }
            catch (NVIDIANotSupportedException)
            {
                caps = default;
                return false;
            }
            catch (NVIDIAApiException ex) when (IsCapabilityUnavailableStatus(ex.Status))
            {
                caps = default;
                return false;
            }
        }

        /// <summary>
        ///     Returns physical GPU handle metadata from NvAPI_SYS_GetPhysicalGPUs (Release 530+).
        /// </summary>
        /// <returns>Physical GPU handle metadata entries.</returns>
        public static PhysicalGPUHandleDataV1[] GetPhysicalGPUHandleData()
        {
            var instance = typeof(PhysicalGPUsV1).Instantiate<PhysicalGPUsV1>();

            using (var physicalGpusReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<Delegates.GPU.NvAPI_SYS_GetPhysicalGPUs>()(
                    physicalGpusReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return physicalGpusReference.ToValueType<PhysicalGPUsV1>(typeof(PhysicalGPUsV1)).GPUHandleData;
            }
        }

        /// <summary>
        ///     Tries to get physical GPU handle metadata without throwing for unsupported drivers.
        /// </summary>
        /// <param name="handleData">Returned metadata entries when successful.</param>
        /// <returns>True when data is available; otherwise false.</returns>
        public static bool TryGetPhysicalGPUHandleData(out PhysicalGPUHandleDataV1[] handleData)
        {
            try
            {
                handleData = GetPhysicalGPUHandleData();
                return true;
            }
            catch (NVIDIANotSupportedException)
            {
                handleData = Array.Empty<PhysicalGPUHandleDataV1>();
                return false;
            }
            catch (NVIDIAApiException ex) when (IsCapabilityUnavailableStatus(ex.Status))
            {
                handleData = Array.Empty<PhysicalGPUHandleDataV1>();
                return false;
            }
        }

        /// <summary>
        ///     Returns logical GPU handle metadata from NvAPI_SYS_GetLogicalGPUs (Release 530+).
        /// </summary>
        /// <returns>Logical GPU handle metadata entries.</returns>
        public static LogicalGPUHandleDataV1[] GetLogicalGPUHandleData()
        {
            var instance = typeof(LogicalGPUsV1).Instantiate<LogicalGPUsV1>();

            using (var logicalGpusReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<Delegates.GPU.NvAPI_SYS_GetLogicalGPUs>()(
                    logicalGpusReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return logicalGpusReference.ToValueType<LogicalGPUsV1>(typeof(LogicalGPUsV1)).GPUHandleData;
            }
        }

        /// <summary>
        ///     Tries to get logical GPU handle metadata without throwing for unsupported drivers.
        /// </summary>
        /// <param name="handleData">Returned metadata entries when successful.</param>
        /// <returns>True when data is available; otherwise false.</returns>
        public static bool TryGetLogicalGPUHandleData(out LogicalGPUHandleDataV1[] handleData)
        {
            try
            {
                handleData = GetLogicalGPUHandleData();
                return true;
            }
            catch (NVIDIANotSupportedException)
            {
                handleData = Array.Empty<LogicalGPUHandleDataV1>();
                return false;
            }
            catch (NVIDIAApiException ex) when (IsCapabilityUnavailableStatus(ex.Status))
            {
                handleData = Array.Empty<LogicalGPUHandleDataV1>();
                return false;
            }
        }

        /// <summary>
        ///     Tries to get private power topology status without throwing for unsupported drivers/GPUs.
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle.</param>
        /// <param name="topologyStatus">Returned topology status when successful.</param>
        /// <returns>True when data is available; otherwise false.</returns>
        public static bool TryClientPowerTopologyGetStatus(
            PhysicalGPUHandle gpuHandle,
            out PrivatePowerTopologiesStatusV1 topologyStatus)
        {
            try
            {
                topologyStatus = ClientPowerTopologyGetStatus(gpuHandle);
                return true;
            }
            catch (NVIDIANotSupportedException)
            {
                topologyStatus = default;
                return false;
            }
            catch (NVIDIAApiException ex) when (IsCapabilityUnavailableStatus(ex.Status))
            {
                topologyStatus = default;
                return false;
            }
        }

        /// <summary>
        ///     Tries to get private power policy status without throwing for unsupported drivers/GPUs.
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle.</param>
        /// <param name="policyStatus">Returned power policy status when successful.</param>
        /// <returns>True when data is available; otherwise false.</returns>
        public static bool TryClientPowerPoliciesGetStatus(
            PhysicalGPUHandle gpuHandle,
            out PrivatePowerPoliciesStatusV1 policyStatus)
        {
            try
            {
                policyStatus = ClientPowerPoliciesGetStatus(gpuHandle);
                return true;
            }
            catch (NVIDIANotSupportedException)
            {
                policyStatus = default;
                return false;
            }
            catch (NVIDIAApiException ex) when (IsCapabilityUnavailableStatus(ex.Status))
            {
                policyStatus = default;
                return false;
            }
        }

        /// <summary>
        ///     Tries to get private power policy info without throwing for unsupported drivers/GPUs.
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle.</param>
        /// <param name="policyInfo">Returned power policy info when successful.</param>
        /// <returns>True when data is available; otherwise false.</returns>
        public static bool TryClientPowerPoliciesGetInfo(
            PhysicalGPUHandle gpuHandle,
            out PrivatePowerPoliciesInfoV1 policyInfo)
        {
            try
            {
                policyInfo = ClientPowerPoliciesGetInfo(gpuHandle);
                return true;
            }
            catch (NVIDIANotSupportedException)
            {
                policyInfo = default;
                return false;
            }
            catch (NVIDIAApiException ex) when (IsCapabilityUnavailableStatus(ex.Status))
            {
                policyInfo = default;
                return false;
            }
        }

        /// <summary>
        ///     Tries to get the current performance policy status without throwing for unsupported drivers/GPUs.
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle.</param>
        /// <param name="performanceStatus">Returned performance status when successful.</param>
        /// <returns>True when data is available; otherwise false.</returns>
        public static bool TryGetPerformancePoliciesStatus(
            PhysicalGPUHandle gpuHandle,
            out PrivatePerformanceStatusV1 performanceStatus)
        {
            try
            {
                performanceStatus = PerformancePoliciesGetStatus(gpuHandle);
                return true;
            }
            catch (NVIDIANotSupportedException)
            {
                performanceStatus = default;
                return false;
            }
            catch (NVIDIAApiException ex) when (IsCapabilityUnavailableStatus(ex.Status))
            {
                performanceStatus = default;
                return false;
            }
        }

        /// <summary>
        ///     Tries to get the current performance decrease reason without throwing for unsupported drivers/GPUs.
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle.</param>
        /// <param name="decreaseReason">Returned reason when successful.</param>
        /// <returns>True when data is available; otherwise false.</returns>
        public static bool TryGetPerformanceDecreaseInfo(
            PhysicalGPUHandle gpuHandle,
            out PerformanceDecreaseReason decreaseReason)
        {
            try
            {
                decreaseReason = GetPerformanceDecreaseInfo(gpuHandle);
                return true;
            }
            catch (NVIDIANotSupportedException)
            {
                decreaseReason = default;
                return false;
            }
            catch (NVIDIAApiException ex) when (IsCapabilityUnavailableStatus(ex.Status))
            {
                decreaseReason = default;
                return false;
            }
        }

        /// <summary>
        ///     Tries to get current P-State without throwing for unsupported drivers/GPUs.
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle.</param>
        /// <param name="performanceState">Returned P-State when successful.</param>
        /// <returns>True when data is available; otherwise false.</returns>
        public static bool TryGetCurrentPerformanceState(
            PhysicalGPUHandle gpuHandle,
            out PerformanceStateId performanceState)
        {
            try
            {
                performanceState = GetCurrentPerformanceState(gpuHandle);
                return true;
            }
            catch (NVIDIANotSupportedException)
            {
                performanceState = default;
                return false;
            }
            catch (NVIDIAApiException ex) when (IsCapabilityUnavailableStatus(ex.Status))
            {
                performanceState = default;
                return false;
            }
        }

        private static bool IsCapabilityUnavailableStatus(Status status)
        {
            return status == Status.NotSupported ||
                   status == Status.NoImplementation ||
                   status == Status.FunctionNotFound;
        }
    }
}
