using System.Runtime.InteropServices;
using NvAPIWrapper.Native.General;
using NvAPIWrapper.Native.Interfaces;

namespace NvAPIWrapper.Native.General.Structures
{
    /// <summary>
    ///     RISE callback payload data.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RiseCallbackDataV1 : IInitializable
    {
        internal RiseCallbackSettingsSuperV1 _Super;
        internal RiseContentType _ContentType;
        internal GenericString _Content;
        internal uint _Completed;

        /// <summary>
        ///     Callback super settings.
        /// </summary>
        public RiseCallbackSettingsSuperV1 Super
        {
            get => _Super;
        }

        /// <summary>
        ///     Callback content type.
        /// </summary>
        public RiseContentType ContentType
        {
            get => _ContentType;
        }

        /// <summary>
        ///     Callback content payload.
        /// </summary>
        public string Content
        {
            get => _Content.Value ?? string.Empty;
        }

        /// <summary>
        ///     True when this is the last item in the callback batch.
        /// </summary>
        public bool IsCompleted
        {
            get => _Completed != 0;
        }
    }
}
