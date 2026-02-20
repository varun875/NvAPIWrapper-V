using System.Runtime.InteropServices;
using NvAPIWrapper.Native.General.Structures;

namespace NvAPIWrapper.Native.General
{
    /// <summary>
    ///     Managed callback signature for RISE update callbacks.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RiseCallbackV1(ref RiseCallbackDataV1 data);
}
