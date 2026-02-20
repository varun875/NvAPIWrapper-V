using System;
using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Attributes;
using NvAPIWrapper.Native.Helpers;
using NvAPIWrapper.Native.Interfaces;

namespace NvAPIWrapper.Native.General.Structures
{
    /// <summary>
    ///     Callback registration settings for NvAPI_RegisterRiseCallback.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [StructureVersion(1)]
    public struct RiseCallbackSettingsV1 : IInitializable
    {
        internal StructureVersion _Version;
        internal RiseCallbackSettingsSuperV1 _Super;
        internal IntPtr _Callback;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        internal byte[] _Reserved;

        /// <summary>
        ///     Creates callback registration settings.
        /// </summary>
        /// <param name="callback">Unmanaged function pointer for callback (or zero to unregister).</param>
        /// <param name="callbackParameter">User callback parameter pointer.</param>
        public RiseCallbackSettingsV1(IntPtr callback, IntPtr callbackParameter = default)
        {
            this = typeof(RiseCallbackSettingsV1).Instantiate<RiseCallbackSettingsV1>();
            _Super = new RiseCallbackSettingsSuperV1(callbackParameter);
            _Callback = callback;
        }

        /// <summary>
        ///     Callback settings super payload.
        /// </summary>
        public RiseCallbackSettingsSuperV1 Super
        {
            get => _Super;
        }

        /// <summary>
        ///     Unmanaged callback function pointer.
        /// </summary>
        public IntPtr Callback
        {
            get => _Callback;
        }
    }
}
