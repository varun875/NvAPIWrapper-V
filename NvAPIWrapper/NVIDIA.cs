using NvAPIWrapper.Native;
using NvAPIWrapper.Native.General.Structures;
using NvAPIWrapper.Native.Interfaces.General;

namespace NvAPIWrapper
{
    /// <summary>
    ///     .Net friendly version of system and general functions of NVAPI library
    /// </summary>
    public static class NVIDIA
    {
        /// <summary>
        ///     Gets information about the system's chipset.
        /// </summary>
        public static IChipsetInfo ChipsetInfo
        {
            get => GeneralApi.GetChipsetInfo();
        }

        /// <summary>
        ///     Gets NVIDIA driver branch version as string
        /// </summary>
        public static string DriverBranchVersion
        {
            get
            {
                GeneralApi.GetDriverAndBranchVersion(out var branchVersion);

                return branchVersion;
            }
        }

        /// <summary>
        ///     Gets NVIDIA driver version
        /// </summary>
        public static uint DriverVersion
        {
            get => GeneralApi.GetDriverAndBranchVersion(out _);
        }

        /// <summary>
        ///     Gets detailed NVIDIA display driver package information.
        /// </summary>
        public static DisplayDriverInfoV2 DisplayDriverInfo
        {
            get => GeneralApi.GetDisplayDriverInfo();
        }

        /// <summary>
        ///     Gets NVAPI interface version as string
        /// </summary>
        public static string InterfaceVersionString
        {
            get => GeneralApi.GetInterfaceVersionString();
        }

        /// <summary>
        ///     Gets the current lid and dock information.
        /// </summary>
        public static LidDockParameters LidAndDockParameters
        {
            get => GeneralApi.GetLidAndDockInfo();
        }

        /// <summary>
        ///     Initializes the NvAPI library (if not already initialized) but always increments the ref-counter.
        /// </summary>
        public static void Initialize()
        {
            GeneralApi.Initialize();
        }

        /// <summary>
        ///     Tries to get detailed NVIDIA display driver package information.
        /// </summary>
        /// <param name="driverInfo">Returned driver package information when available.</param>
        /// <returns>True when this information is supported and available.</returns>
        public static bool TryGetDisplayDriverInfo(out DisplayDriverInfoV2 driverInfo)
        {
            return GeneralApi.TryGetDisplayDriverInfo(out driverInfo);
        }

        /// <summary>
        ///     PRIVATE - Requests to restart the display driver
        /// </summary>
        public static void RestartDisplayDriver()
        {
            GeneralApi.RestartDisplayDriver();
        }

        /// <summary>
        ///     Decrements the ref-counter and when it reaches ZERO, unloads NVAPI library.
        /// </summary>
        public static void Unload()
        {
            GeneralApi.Unload();
        }
    }
}
