using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Attributes;
using NvAPIWrapper.Native.General.Structures;
using NvAPIWrapper.Native.Interfaces;

namespace NvAPIWrapper.Native.GPU.Structures
{
    /// <summary>
    ///     Contains firmware information for GSP-enabled GPUs.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [StructureVersion(1)]
    public struct GSPInfoV1 : IInitializable
    {
        internal StructureVersion _Version;
        internal readonly ShortString _FirmwareVersion;
        internal readonly uint _Reserved;

        /// <summary>
        ///     GSP firmware version string.
        /// </summary>
        public string FirmwareVersion
        {
            get => (_FirmwareVersion.Value ?? string.Empty).TrimEnd('\0');
        }
    }
}
