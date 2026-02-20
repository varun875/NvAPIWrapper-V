using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Attributes;
using NvAPIWrapper.Native.General.Structures;
using NvAPIWrapper.Native.Interfaces;

namespace NvAPIWrapper.Native.GPU.Structures
{
    /// <summary>
    ///     NVLink capability information returned by NvAPI_GPU_NVLINK_GetCaps.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [StructureVersion(1)]
    public struct NVLinkGetCapsV1 : IInitializable
    {
        internal StructureVersion _Version;
        internal uint _Capabilities;
        internal byte _LowestNVLinkVersion;
        internal byte _HighestNVLinkVersion;
        internal byte _LowestNCIVersion;
        internal byte _HighestNCIVersion;
        internal uint _LinkMask;

        /// <summary>
        ///     Raw NVLink capability flags.
        /// </summary>
        public NVLinkCapabilityFlags CapabilityFlags
        {
            get => (NVLinkCapabilityFlags) _Capabilities;
        }

        /// <summary>
        ///     Lowest supported NVLink protocol version.
        /// </summary>
        public byte LowestNVLinkVersion
        {
            get => _LowestNVLinkVersion;
        }

        /// <summary>
        ///     Highest supported NVLink protocol version.
        /// </summary>
        public byte HighestNVLinkVersion
        {
            get => _HighestNVLinkVersion;
        }

        /// <summary>
        ///     Lowest supported NCI protocol version.
        /// </summary>
        public byte LowestNCIVersion
        {
            get => _LowestNCIVersion;
        }

        /// <summary>
        ///     Highest supported NCI protocol version.
        /// </summary>
        public byte HighestNCIVersion
        {
            get => _HighestNCIVersion;
        }

        /// <summary>
        ///     Bit mask of enabled NVLink links.
        /// </summary>
        public uint LinkMask
        {
            get => _LinkMask;
        }

        /// <summary>
        ///     True when NVLink is present and supported.
        /// </summary>
        public bool IsSupported
        {
            get => CapabilityFlags.HasFlag(NVLinkCapabilityFlags.Supported);
        }
    }
}
