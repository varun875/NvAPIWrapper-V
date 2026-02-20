using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Attributes;
using NvAPIWrapper.Native.General;
using NvAPIWrapper.Native.Helpers;
using NvAPIWrapper.Native.Interfaces;

namespace NvAPIWrapper.Native.General.Structures
{
    /// <summary>
    ///     Request payload for NvAPI_RequestRise.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [StructureVersion(1)]
    public struct RequestRiseSettingsV1 : IInitializable
    {
        internal StructureVersion _Version;
        internal RiseContentType _ContentType;
        internal GenericString _Content;
        internal uint _Completed;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        internal byte[] _Reserved;

        /// <summary>
        ///     Creates RISE request settings.
        /// </summary>
        /// <param name="contentType">Content payload type.</param>
        /// <param name="content">Request content.</param>
        /// <param name="completed">True if request is complete in this payload.</param>
        public RequestRiseSettingsV1(RiseContentType contentType, string content, bool completed = true)
        {
            this = typeof(RequestRiseSettingsV1).Instantiate<RequestRiseSettingsV1>();
            _ContentType = contentType;
            _Content = new GenericString(content ?? string.Empty);
            _Completed = completed ? 1u : 0u;
        }

        /// <summary>
        ///     Request content type.
        /// </summary>
        public RiseContentType ContentType
        {
            get => _ContentType;
        }

        /// <summary>
        ///     Request content text.
        /// </summary>
        public string Content
        {
            get => _Content.Value ?? string.Empty;
        }

        /// <summary>
        ///     True when this payload is marked complete.
        /// </summary>
        public bool IsCompleted
        {
            get => _Completed != 0;
        }
    }
}
