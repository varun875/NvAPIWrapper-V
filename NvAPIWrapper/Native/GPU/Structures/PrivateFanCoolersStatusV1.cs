using System.Linq;
using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Attributes;
using NvAPIWrapper.Native.General.Structures;
using NvAPIWrapper.Native.Interfaces;

namespace NvAPIWrapper.Native.GPU.Structures
{
    /// <summary>
    ///     [PRIVATE] Client fan cooler live status (v1).
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivateFanCoolersStatusV1 : IInitializable
    {
        internal const int MaxNumberOfFanCoolerStatusEntries = 32;

        internal StructureVersion _Version;
        internal readonly uint _FanCoolersStatusCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8, ArraySubType = UnmanagedType.U4)]
        internal readonly uint[] _Reserved;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfFanCoolerStatusEntries)]
        internal readonly FanCoolersStatusEntry[] _FanCoolersStatusEntries;

        /// <summary>
        ///     Gets the fan cooler live status entries.
        /// </summary>
        public FanCoolersStatusEntry[] FanCoolersStatusEntries
        {
            get => _FanCoolersStatusEntries.Take((int) _FanCoolersStatusCount).ToArray();
        }

        /// <summary>
        ///     A single fan cooler live status entry.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct FanCoolersStatusEntry
        {
            internal readonly uint _CoolerId;
            internal readonly uint _CurrentRPM;
            internal readonly uint _CurrentMinimumLevel;
            internal readonly uint _CurrentMaximumLevel;
            internal readonly uint _CurrentLevel;

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
            ///     Gets the current cooler speed in revolutions per minute.
            /// </summary>
            public uint CurrentRPM
            {
                get => _CurrentRPM;
            }

            /// <summary>
            ///     Gets the current minimum control level.
            /// </summary>
            public uint CurrentMinimumLevel
            {
                get => _CurrentMinimumLevel;
            }

            /// <summary>
            ///     Gets the current maximum control level.
            /// </summary>
            public uint CurrentMaximumLevel
            {
                get => _CurrentMaximumLevel;
            }

            /// <summary>
            ///     Gets the current control level.
            /// </summary>
            public uint CurrentLevel
            {
                get => _CurrentLevel;
            }
        }
    }
}