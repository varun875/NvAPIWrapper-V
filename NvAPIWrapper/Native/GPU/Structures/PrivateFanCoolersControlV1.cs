using System;
using System.Linq;
using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Attributes;
using NvAPIWrapper.Native.General.Structures;
using NvAPIWrapper.Native.Helpers;
using NvAPIWrapper.Native.Interfaces;

namespace NvAPIWrapper.Native.GPU.Structures
{
    /// <summary>
    ///     [PRIVATE] Client fan cooler control settings (v1).
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivateFanCoolersControlV1 : IInitializable
    {
        internal const int MaxNumberOfFanCoolerControlEntries = 32;
        internal StructureVersion _Version;
        internal readonly uint _UnknownUInt;
        internal readonly uint _FanCoolersControlCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8, ArraySubType = UnmanagedType.U4)]
        internal readonly uint[] _Reserved;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfFanCoolerControlEntries)]
        internal readonly FanCoolersControlEntry[] _FanCoolersControlEntries;

        /// <summary>
        ///     Gets the configured fan cooler control entries.
        /// </summary>
        public FanCoolersControlEntry[] FanCoolersControlEntries
        {
            get => _FanCoolersControlEntries.Take((int) _FanCoolersControlCount).ToArray();
        }

        /// <summary>
        ///     Gets the driver-reserved control value.
        /// </summary>
        public uint UnknownUInt
        {
            get => _UnknownUInt;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrivateFanCoolersControlV1" /> structure.
        /// </summary>
        /// <param name="entries">The fan cooler control entries to apply.</param>
        /// <param name="unknownUInt">The driver-reserved control value.</param>
        public PrivateFanCoolersControlV1(FanCoolersControlEntry[] entries, uint unknownUInt = 0)
        {
            if (entries?.Length > MaxNumberOfFanCoolerControlEntries)
            {
                throw new ArgumentException(
                    $"Maximum of {MaxNumberOfFanCoolerControlEntries} coolers are configurable.",
                    nameof(entries));
            }

            if (entries == null || entries.Length == 0)
            {
                throw new ArgumentException("Array is null or empty.", nameof(entries));
            }

            entries = entries.OrderBy(entry => entry.CoolerId).ToArray();

            this = typeof(PrivateFanCoolersControlV1).Instantiate<PrivateFanCoolersControlV1>();
            _UnknownUInt = unknownUInt;
            _FanCoolersControlCount = (uint) entries.Length;
            Array.Copy(entries, 0, _FanCoolersControlEntries, 0, entries.Length);
        }


        /// <summary>
        ///     A single fan cooler control entry.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct FanCoolersControlEntry
        {
            internal readonly uint _CoolerId;
            internal readonly uint _Level;
            internal readonly FanCoolersControlMode _ControlMode;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8, ArraySubType = UnmanagedType.U4)]
            internal readonly uint[] _Reserved;

            /// <summary>
            ///     Initializes a new instance of the <see cref="FanCoolersControlEntry" /> structure.
            /// </summary>
            /// <param name="coolerId">The cooler identifier.</param>
            /// <param name="controlMode">The control mode to apply.</param>
            /// <param name="level">The manual control level.</param>
            public FanCoolersControlEntry(uint coolerId, FanCoolersControlMode controlMode, uint level)
            {
                this = typeof(FanCoolersControlEntry).Instantiate<FanCoolersControlEntry>();
                _CoolerId = coolerId;
                _ControlMode = controlMode;
                _Level = level;
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="FanCoolersControlEntry" /> structure.
            /// </summary>
            /// <param name="coolerId">The cooler identifier.</param>
            /// <param name="controlMode">The control mode to apply.</param>
            public FanCoolersControlEntry(uint coolerId, FanCoolersControlMode controlMode) : this(coolerId,
                controlMode, 0)
            {
                if (controlMode == FanCoolersControlMode.Manual)
                {
                    throw new ArgumentException(
                        "Manual control mode is not valid when no level value is provided.",
                        nameof(controlMode)
                    );
                }
            }

            /// <summary>
            ///     Gets the cooler identifier.
            /// </summary>
            public uint CoolerId
            {
                get => _CoolerId;
            }

            /// <summary>
            ///     Gets the manual control level.
            /// </summary>
            public uint Level
            {
                get => _Level;
            }

            /// <summary>
            ///     Gets the control mode.
            /// </summary>
            public FanCoolersControlMode ControlMode
            {
                get => _ControlMode;
            }
        }
    }
}