using System.Linq;
using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Attributes;
using NvAPIWrapper.Native.General.Structures;
using NvAPIWrapper.Native.Interfaces;

namespace NvAPIWrapper.Native.GPU.Structures
{
    /// <summary>
    ///     [PRIVATE] Client fan cooler capability information (v1).
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivateFanCoolersInfoV1 : IInitializable
    {
        internal const int MaxNumberOfFanCoolerInfoEntries = 32;

        internal StructureVersion _Version;
        internal readonly uint _UnknownUInt1;
        internal readonly uint _FanCoolersInfoCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8, ArraySubType = UnmanagedType.U4)]
        internal readonly uint[] _Reserved;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfFanCoolerInfoEntries)]
        internal readonly FanCoolersInfoEntry[] _FanCoolersInfoEntries;

        /// <summary>
        ///     Gets the fan cooler capability entries.
        /// </summary>
        public FanCoolersInfoEntry[] FanCoolersInfoEntries
        {
            get => _FanCoolersInfoEntries.Take((int) _FanCoolersInfoCount).ToArray();
        }

        /// <summary>
        ///     A single fan cooler capability entry.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct FanCoolersInfoEntry
        {
            internal readonly uint _CoolerId;
            internal readonly uint _UnknownUInt3;
            internal readonly uint _UnknownUInt4;
            internal readonly uint _MaximumRPM;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8, ArraySubType = UnmanagedType.U4)]
            internal readonly uint[] _Reserved;

            /// <summary>
            ///     Gets the cooler identifier.
            /// </summary>
            public uint CoolerId
            {
                get => _CoolerId;
            }

            /// <summary>
            ///     Gets the maximum cooler speed in revolutions per minute.
            /// </summary>
            public uint MaximumRPM
            {
                get => _MaximumRPM;
            }
        }
    }
}