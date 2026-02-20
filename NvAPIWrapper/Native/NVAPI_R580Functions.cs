using System.ComponentModel;

#nullable enable

namespace NvAPIWrapper.Native
{
    /// <summary>
    ///     Verified modern NVAPI function IDs aligned to a safe R580-era baseline.
    /// </summary>
    public static class NVAPI_R580Functions
    {
        /// <summary>
        ///     Get GSP firmware features/version information (Release 550+).
        /// </summary>
        [Description("Get GSP firmware features/version")]
        public const uint NvAPI_GPU_GetGspFeatures = 0x581C4391;

        /// <summary>
        ///     Get NVLink capabilities (official NVLINK API).
        /// </summary>
        [Description("Get NVLink capabilities")]
        public const uint NvAPI_GPU_NVLINK_GetCaps = 0xBEF1119D;

        /// <summary>
        ///     Get display driver package information.
        /// </summary>
        [Description("Get display driver information")]
        public const uint NvAPI_SYS_GetDisplayDriverInfo = 0x721FACEB;

        /// <summary>
        ///     Enumerate physical GPU handles with adapter metadata (Release 530+).
        /// </summary>
        [Description("Get physical GPUs with adapter metadata")]
        public const uint NvAPI_SYS_GetPhysicalGPUs = 0xD3B24D2D;

        /// <summary>
        ///     Enumerate logical GPU handles with adapter metadata (Release 530+).
        /// </summary>
        [Description("Get logical GPUs with adapter metadata")]
        public const uint NvAPI_SYS_GetLogicalGPUs = 0xCCFFFC10;

        /// <summary>
        ///     Register RISE callback (Release r570+).
        /// </summary>
        [Description("Register RISE callback")]
        public const uint NvAPI_RegisterRiseCallback = 0x9CFE8F94;

        /// <summary>
        ///     Request RISE operation (Release r570+).
        /// </summary>
        [Description("Request RISE")]
        public const uint NvAPI_RequestRise = 0x5047DE98;

        /// <summary>
        ///     Uninstall RISE feature (Release r570+).
        /// </summary>
        [Description("Uninstall RISE")]
        public const uint NvAPI_UninstallRise = 0xAB8D09F6;
    }
}
