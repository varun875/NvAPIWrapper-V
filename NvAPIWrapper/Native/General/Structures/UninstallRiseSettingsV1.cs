using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Attributes;
using NvAPIWrapper.Native.Interfaces;

namespace NvAPIWrapper.Native.General.Structures
{
    /// <summary>
    ///     Request payload for NvAPI_UninstallRise.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [StructureVersion(1)]
    public struct UninstallRiseSettingsV1 : IInitializable
    {
        internal StructureVersion _Version;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        internal byte[] _Reserved;
    }
}
