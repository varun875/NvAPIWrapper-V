using NvAPIWrapper.Native.General;

namespace NvAPIWrapper.Native.Helpers
{
    /// <summary>
    ///     Shared helper for evaluating NVAPI status codes related to capability availability.
    /// </summary>
    internal static class NvApiStatusHelper
    {
        /// <summary>
        ///     Returns true when the status indicates that the requested capability is unavailable
        ///     on the current GPU/driver combination (as opposed to a real error).
        /// </summary>
        internal static bool IsCapabilityUnavailable(Status status)
        {
            return status == Status.NotSupported ||
                   status == Status.NoImplementation ||
                   status == Status.FunctionNotFound;
        }
    }
}
