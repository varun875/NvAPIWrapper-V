using System;

namespace NvAPIWrapper.Native.GPU
{
    /// <summary>
    ///     Adapter model for a logical/physical GPU handle returned by modern system enumeration APIs.
    /// </summary>
    [Flags]
    public enum AdapterType : uint
    {
        /// <summary>
        ///     Adapter model information is not available.
        /// </summary>
        None = 0,

        /// <summary>
        ///     WDDM adapter (graphics/display path).
        /// </summary>
        WDDM = 1,

        /// <summary>
        ///     MCDM adapter (Microsoft Compute Driver Model).
        /// </summary>
        MCDM = 1 << 1,

        /// <summary>
        ///     TCC adapter (Tesla Compute Cluster).
        /// </summary>
        TCC = 1 << 2
    }
}
