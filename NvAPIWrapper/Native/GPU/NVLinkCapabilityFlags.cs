using System;

namespace NvAPIWrapper.Native.GPU
{
    /// <summary>
    ///     NVLink capability flags returned by NVLINK_GET_CAPS.
    /// </summary>
    [Flags]
    public enum NVLinkCapabilityFlags : uint
    {
        /// <summary>
        ///     No capability flags are set.
        /// </summary>
        None = 0,

        /// <summary>
        ///     NVLink is present and supported on this GPU.
        /// </summary>
        Supported = 0x00000001,

        /// <summary>
        ///     Peer-to-peer over NVLink is supported.
        /// </summary>
        P2PSupported = 0x00000002,

        /// <summary>
        ///     System memory access over NVLink is supported.
        /// </summary>
        SysmemAccess = 0x00000004,

        /// <summary>
        ///     Peer-to-peer atomics over NVLink are supported.
        /// </summary>
        P2PAtomics = 0x00000008,

        /// <summary>
        ///     System memory atomics over NVLink are supported.
        /// </summary>
        SysmemAtomics = 0x00000010,

        /// <summary>
        ///     PEX tunneling over NVLink is supported.
        /// </summary>
        PexTunneling = 0x00000020,

        /// <summary>
        ///     SLI over NVLink is supported.
        /// </summary>
        SliBridge = 0x00000040,

        /// <summary>
        ///     SLI bridge sensing is supported.
        /// </summary>
        SliBridgeSensable = 0x00000080,

        /// <summary>
        ///     L0 power state is supported.
        /// </summary>
        PowerStateL0 = 0x00000100,

        /// <summary>
        ///     L1 power state is supported.
        /// </summary>
        PowerStateL1 = 0x00000200,

        /// <summary>
        ///     L2 power state is supported.
        /// </summary>
        PowerStateL2 = 0x00000400,

        /// <summary>
        ///     L3 power state is supported.
        /// </summary>
        PowerStateL3 = 0x00000800,

        /// <summary>
        ///     Per-link capability value is valid.
        /// </summary>
        Valid = 0x00001000,

        /// <summary>
        ///     Resetless recovery from uncontained packet errors is supported.
        /// </summary>
        UncontainedErrorRecovery = 0x00002000
    }
}
