using System;

namespace NvAPIWrapper.Native.General
{
    /// <summary>
    ///     Package classification flags returned by NV_DISPLAY_DRIVER_INFO.
    /// </summary>
    [Flags]
    public enum DisplayDriverPackageFlags : uint
    {
        /// <summary>
        ///     No package metadata reported.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Driver is packaged as DCH.
        /// </summary>
        DCHDriver = 1 << 0,

        /// <summary>
        ///     Driver is from NVIDIA Studio package.
        /// </summary>
        NVIDIAStudioPackage = 1 << 1,

        /// <summary>
        ///     Driver is from NVIDIA Game Ready package.
        /// </summary>
        NVIDIAGameReadyPackage = 1 << 2,

        /// <summary>
        ///     Driver is from NVIDIA RTX Enterprise Production Branch package.
        /// </summary>
        NVIDIARTXProductionBranchPackage = 1 << 3,

        /// <summary>
        ///     Driver is from NVIDIA RTX New Feature Branch package.
        /// </summary>
        NVIDIARTXNewFeatureBranchPackage = 1 << 4
    }
}
