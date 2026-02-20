using System;
using System.Linq;
using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Attributes;
using NvAPIWrapper.Native.General.Structures;
using NvAPIWrapper.Native.Interfaces;

namespace NvAPIWrapper.Native.GPU.Structures
{
    /// <summary>
    ///     Physical GPU handle data returned by modern system-level GPU enumeration.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PhysicalGPUHandleDataV1 : IInitializable
    {
        internal readonly PhysicalGPUHandle _PhysicalGPUHandle;
        internal readonly AdapterType _AdapterType;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        internal readonly uint[] _Reserved;

        /// <summary>
        ///     Physical GPU handle.
        /// </summary>
        public PhysicalGPUHandle PhysicalGPUHandle
        {
            get => _PhysicalGPUHandle;
        }

        /// <summary>
        ///     Adapter model for this handle.
        /// </summary>
        public AdapterType AdapterType
        {
            get => _AdapterType;
        }
    }

    /// <summary>
    ///     Physical GPU enumeration payload returned by NvAPI_SYS_GetPhysicalGPUs.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [StructureVersion(1)]
    public struct PhysicalGPUsV1 : IInitializable
    {
        internal StructureVersion _Version;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PhysicalGPUHandle.MaxPhysicalGPUs)]
        internal PhysicalGPUHandleDataV1[] _GPUHandleData;

        internal uint _GPUHandleCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        internal uint[] _Reserved;

        /// <summary>
        ///     Number of physical GPU handles returned.
        /// </summary>
        public int GPUHandleCount
        {
            get => (int)_GPUHandleCount;
        }

        /// <summary>
        ///     Returned physical GPU handle metadata entries.
        /// </summary>
        public PhysicalGPUHandleDataV1[] GPUHandleData
        {
            get => _GPUHandleData?.Take((int)_GPUHandleCount).ToArray() ?? Array.Empty<PhysicalGPUHandleDataV1>();
        }
    }
}
