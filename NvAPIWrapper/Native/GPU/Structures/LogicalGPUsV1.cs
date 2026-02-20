using System;
using System.Linq;
using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Attributes;
using NvAPIWrapper.Native.General.Structures;
using NvAPIWrapper.Native.Interfaces;

namespace NvAPIWrapper.Native.GPU.Structures
{
    /// <summary>
    ///     Logical GPU handle data returned by modern system-level GPU enumeration.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LogicalGPUHandleDataV1 : IInitializable
    {
        internal readonly LogicalGPUHandle _LogicalGPUHandle;
        internal readonly AdapterType _AdapterType;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        internal readonly uint[] _Reserved;

        /// <summary>
        ///     Logical GPU handle.
        /// </summary>
        public LogicalGPUHandle LogicalGPUHandle
        {
            get => _LogicalGPUHandle;
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
    ///     Logical GPU enumeration payload returned by NvAPI_SYS_GetLogicalGPUs.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [StructureVersion(1)]
    public struct LogicalGPUsV1 : IInitializable
    {
        internal StructureVersion _Version;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = LogicalGPUHandle.MaxLogicalGPUs)]
        internal LogicalGPUHandleDataV1[] _GPUHandleData;

        internal uint _GPUHandleCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        internal uint[] _Reserved;

        /// <summary>
        ///     Number of logical GPU handles returned.
        /// </summary>
        public int GPUHandleCount
        {
            get => (int)_GPUHandleCount;
        }

        /// <summary>
        ///     Returned logical GPU handle metadata entries.
        /// </summary>
        public LogicalGPUHandleDataV1[] GPUHandleData
        {
            get => _GPUHandleData?.Take((int)_GPUHandleCount).ToArray() ?? Array.Empty<LogicalGPUHandleDataV1>();
        }
    }
}
