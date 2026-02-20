using System;
using NvAPIWrapper.Native.Exceptions;
using NvAPIWrapper.Native.General;
using NvAPIWrapper.Native.General.Structures;
using NvAPIWrapper.Native.Helpers;
using NvAPIWrapper.Native.Helpers.Structures;

namespace NvAPIWrapper.Native
{
    /// <summary>
    ///     Modern system APIs (R396+ / r570+) with compatibility fallbacks.
    /// </summary>
    public static partial class GeneralApi
    {
        /// <summary>
        ///     Returns information about the installed NVIDIA display driver package.
        /// </summary>
        /// <returns>Display driver package information.</returns>
        /// <exception cref="NVIDIAApiException">Throws for NVAPI error statuses except capability unavailability.</exception>
        public static DisplayDriverInfoV2 GetDisplayDriverInfo()
        {
            var instance = typeof(DisplayDriverInfoV2).Instantiate<DisplayDriverInfoV2>();

            using (var driverInfoReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<Delegates.General.NvAPI_SYS_GetDisplayDriverInfo>()(
                    driverInfoReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return driverInfoReference.ToValueType<DisplayDriverInfoV2>(typeof(DisplayDriverInfoV2));
            }
        }

        /// <summary>
        ///     Tries to get display driver package information without throwing for unsupported drivers.
        /// </summary>
        /// <param name="driverInfo">Returned package information when successful.</param>
        /// <returns>True when information is available; otherwise false.</returns>
        public static bool TryGetDisplayDriverInfo(out DisplayDriverInfoV2 driverInfo)
        {
            try
            {
                driverInfo = GetDisplayDriverInfo();
                return true;
            }
            catch (NVIDIANotSupportedException)
            {
                driverInfo = default;
                return false;
            }
            catch (NVIDIAApiException ex) when (IsCapabilityUnavailableStatus(ex.Status))
            {
                driverInfo = default;
                return false;
            }
        }

        /// <summary>
        ///     Registers (or unregisters when callback is null) a RISE callback.
        /// </summary>
        /// <param name="callbackSettings">RISE callback settings payload.</param>
        public static void RegisterRiseCallback(RiseCallbackSettingsV1 callbackSettings)
        {
            using (var callbackSettingsReference = ValueTypeReference.FromValueType(callbackSettings))
            {
                var status = DelegateFactory.GetDelegate<Delegates.General.NvAPI_RegisterRiseCallback>()(
                    callbackSettingsReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }

        /// <summary>
        ///     Tries to register/unregister a RISE callback without throwing for unsupported drivers.
        /// </summary>
        /// <param name="callbackSettings">RISE callback settings payload.</param>
        /// <returns>True when operation is available and successful; otherwise false.</returns>
        public static bool TryRegisterRiseCallback(RiseCallbackSettingsV1 callbackSettings)
        {
            try
            {
                RegisterRiseCallback(callbackSettings);
                return true;
            }
            catch (NVIDIANotSupportedException)
            {
                return false;
            }
            catch (NVIDIAApiException ex) when (IsCapabilityUnavailableStatus(ex.Status))
            {
                return false;
            }
        }

        /// <summary>
        ///     Requests a RISE operation with the provided payload.
        /// </summary>
        /// <param name="requestContent">RISE request payload.</param>
        public static void RequestRise(RequestRiseSettingsV1 requestContent)
        {
            using (var requestContentReference = ValueTypeReference.FromValueType(requestContent))
            {
                var status = DelegateFactory.GetDelegate<Delegates.General.NvAPI_RequestRise>()(
                    requestContentReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }

        /// <summary>
        ///     Tries to request a RISE operation without throwing for unsupported drivers.
        /// </summary>
        /// <param name="requestContent">RISE request payload.</param>
        /// <returns>True when operation is available and successful; otherwise false.</returns>
        public static bool TryRequestRise(RequestRiseSettingsV1 requestContent)
        {
            try
            {
                RequestRise(requestContent);
                return true;
            }
            catch (NVIDIANotSupportedException)
            {
                return false;
            }
            catch (NVIDIAApiException ex) when (IsCapabilityUnavailableStatus(ex.Status))
            {
                return false;
            }
        }

        /// <summary>
        ///     Uninstalls RISE for the current system context.
        /// </summary>
        /// <param name="requestContent">RISE uninstall request payload.</param>
        public static void UninstallRise(UninstallRiseSettingsV1 requestContent)
        {
            using (var requestContentReference = ValueTypeReference.FromValueType(requestContent))
            {
                var status = DelegateFactory.GetDelegate<Delegates.General.NvAPI_UninstallRise>()(
                    requestContentReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }

        /// <summary>
        ///     Tries to uninstall RISE without throwing for unsupported drivers.
        /// </summary>
        /// <param name="requestContent">RISE uninstall request payload.</param>
        /// <returns>True when operation is available and successful; otherwise false.</returns>
        public static bool TryUninstallRise(UninstallRiseSettingsV1 requestContent)
        {
            try
            {
                UninstallRise(requestContent);
                return true;
            }
            catch (NVIDIANotSupportedException)
            {
                return false;
            }
            catch (NVIDIAApiException ex) when (IsCapabilityUnavailableStatus(ex.Status))
            {
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
