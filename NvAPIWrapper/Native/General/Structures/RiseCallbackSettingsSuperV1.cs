using System;
using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Helpers;
using NvAPIWrapper.Native.Interfaces;

namespace NvAPIWrapper.Native.General.Structures
{
    /// <summary>
    ///     Common callback settings for RISE callback payloads.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RiseCallbackSettingsSuperV1 : IInitializable
    {
        internal IntPtr _CallbackParameter;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        internal byte[] _Reserved;

        /// <summary>
        ///     Creates callback super settings.
        /// </summary>
        /// <param name="callbackParameter">User callback parameter pointer.</param>
        public RiseCallbackSettingsSuperV1(IntPtr callbackParameter)
        {
            this = typeof(RiseCallbackSettingsSuperV1).Instantiate<RiseCallbackSettingsSuperV1>();
            _CallbackParameter = callbackParameter;
        }

        /// <summary>
        ///     User callback parameter pointer.
        /// </summary>
        public IntPtr CallbackParameter
        {
            get => _CallbackParameter;
        }
    }
}
