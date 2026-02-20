using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Attributes;
using NvAPIWrapper.Native.General;
using NvAPIWrapper.Native.Helpers;
using NvAPIWrapper.Native.Interfaces;

namespace NvAPIWrapper.Native.General.Structures
{
    /// <summary>
    ///     Information about the installed NVIDIA display driver package.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [StructureVersion(2)]
    public struct DisplayDriverInfoV2 : IInitializable
    {
        internal StructureVersion _Version;
        internal uint _DriverVersion;
        internal ShortString _BuildBranch;
        internal uint _PackageFlags;
        internal ShortString _BuildBaseBranch;
        internal uint _ReservedEx;

        /// <summary>
        ///     NVIDIA display driver version.
        /// </summary>
        public uint DriverVersion
        {
            get => _DriverVersion;
        }

        /// <summary>
        ///     Driver branch string.
        /// </summary>
        public string BuildBranch
        {
            get => (_BuildBranch.Value ?? string.Empty).TrimEnd('\0');
        }

        /// <summary>
        ///     Driver base branch string.
        /// </summary>
        public string BuildBaseBranch
        {
            get => (_BuildBaseBranch.Value ?? string.Empty).TrimEnd('\0');
        }

        /// <summary>
        ///     Raw package flags.
        /// </summary>
        public DisplayDriverPackageFlags PackageFlags
        {
            get => (DisplayDriverPackageFlags) _PackageFlags;
        }

        /// <summary>
        ///     True when installed package is DCH.
        /// </summary>
        public bool IsDCHDriver
        {
            get => _PackageFlags.GetBit(0);
        }

        /// <summary>
        ///     True when installed package is NVIDIA Studio.
        /// </summary>
        public bool IsNVIDIAStudioPackage
        {
            get => _PackageFlags.GetBit(1);
        }

        /// <summary>
        ///     True when installed package is NVIDIA Game Ready.
        /// </summary>
        public bool IsNVIDIAGameReadyPackage
        {
            get => _PackageFlags.GetBit(2);
        }

        /// <summary>
        ///     True when installed package is NVIDIA RTX Enterprise Production Branch.
        /// </summary>
        public bool IsNVIDIARTXProductionBranchPackage
        {
            get => _PackageFlags.GetBit(3);
        }

        /// <summary>
        ///     True when installed package is NVIDIA RTX New Feature Branch.
        /// </summary>
        public bool IsNVIDIARTXNewFeatureBranchPackage
        {
            get => _PackageFlags.GetBit(4);
        }
    }
}
