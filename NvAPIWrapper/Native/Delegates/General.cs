using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Attributes;
using NvAPIWrapper.Native.General;
using NvAPIWrapper.Native.General.Structures;
using NvAPIWrapper.Native.Helpers;
using NvAPIWrapper.Native.Helpers.Structures;

// ReSharper disable InconsistentNaming

namespace NvAPIWrapper.Native.Delegates
{
    internal static class General
    {
        [FunctionId(FunctionId.NvAPI_GetErrorMessage)]
        public delegate Status NvAPI_GetErrorMessage([In] Status status, out ShortString message);

        [FunctionId(FunctionId.NvAPI_GetInterfaceVersionString)]
        public delegate Status NvAPI_GetInterfaceVersionString(out ShortString version);

        [FunctionId(FunctionId.NvAPI_Initialize)]
        public delegate Status NvAPI_Initialize();


        [FunctionId(FunctionId.NvAPI_RestartDisplayDriver)]
        public delegate Status NvAPI_RestartDisplayDriver();

        [FunctionId(FunctionId.NvAPI_SYS_GetChipSetInfo)]
        public delegate Status NvAPI_SYS_GetChipSetInfo(
            [In] [Accepts(typeof(ChipsetInfoV4), typeof(ChipsetInfoV3), typeof(ChipsetInfoV2), typeof(ChipsetInfoV1))]
            ValueTypeReference chipsetInfo);

        [FunctionId(FunctionId.NvAPI_SYS_GetDriverAndBranchVersion)]
        public delegate Status NvAPI_SYS_GetDriverAndBranchVersion(
            out uint driverVersion,
            out ShortString buildBranchString);

        [FunctionId(FunctionId.NvAPI_SYS_GetDisplayDriverInfo)]
        public delegate Status NvAPI_SYS_GetDisplayDriverInfo(
            [Accepts(typeof(DisplayDriverInfoV2))] [In]
            ValueTypeReference driverInfo);

        [FunctionId(FunctionId.NvAPI_SYS_GetLidAndDockInfo)]
        public delegate Status NvAPI_SYS_GetLidAndDockInfo([In] [Out] ref LidDockParameters lidAndDock);

        [FunctionId(FunctionId.NvAPI_RegisterRiseCallback)]
        public delegate Status NvAPI_RegisterRiseCallback(
            [Accepts(typeof(RiseCallbackSettingsV1))] [In]
            ValueTypeReference callbackSettings);

        [FunctionId(FunctionId.NvAPI_RequestRise)]
        public delegate Status NvAPI_RequestRise(
            [Accepts(typeof(RequestRiseSettingsV1))] [In]
            ValueTypeReference requestContent);

        [FunctionId(FunctionId.NvAPI_UninstallRise)]
        public delegate Status NvAPI_UninstallRise(
            [Accepts(typeof(UninstallRiseSettingsV1))] [In]
            ValueTypeReference requestContent);

        [FunctionId(FunctionId.NvAPI_Unload)]
        public delegate Status NvAPI_Unload();
    }
}
