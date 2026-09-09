using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Attributes;
using NvAPIWrapper.Native.General.Structures;
using NvAPIWrapper.Native.Interfaces;

namespace NvAPIWrapper.Native.GPU.Structures
{
    /// <summary>
    ///     [PRIVATE] An active application entry reported by the driver (v2).
    /// </summary>
    [StructureVersion(2)]
    [StructLayout(LayoutKind.Sequential)]
    public struct PrivateActiveApplicationV2 : IInitializable
    {
        internal const int MaximumNumberOfApplications = 128;

        internal StructureVersion _Version;
        internal uint _ProcessId;
        internal LongString _ProcessName;

        /// <summary>
        ///     Gets the process identifier of the active application.
        /// </summary>
        public int ProcessId
        {
            get => (int) _ProcessId;
        }

        /// <summary>
        ///     Gets the process name of the active application.
        /// </summary>
        public string ProcessName
        {
            get => _ProcessName.Value;
        }
    }
}