using System;
using System.IO;
using System.Runtime.InteropServices;

namespace NvAPIWrapper.Native.Power
{
    // ABI references: NVIDIA NVML API, nvmlPciInfo_t and the unversioned power getters.
    // https://docs.nvidia.com/deploy/nvml-api/group__nvmlDeviceQueries.html
    // https://github.com/NVIDIA/go-nvml/blob/main/pkg/nvml/nvml.h
    // Only read-only functions are bound. All uint power outputs are milliwatts.
    internal enum NvmlReturn
    {
        Success = 0,
        Uninitialized = 1,
        InvalidArgument = 2,
        NotSupported = 3,
        NoPermission = 4,
        NotFound = 6,
        DriverNotLoaded = 9,
        FunctionNotFound = 13,
        GpuLost = 15,
        Unknown = 999
    }

    internal enum NvmlPowerQuery
    {
        Usage,
        DefaultLimit,
        CurrentLimit,
        EnforcedLimit
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
    internal struct NvmlPciInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        internal string BusIdLegacy;
        internal uint Domain;
        internal uint Bus;
        internal uint Device;
        internal uint DeviceId;
        internal uint SubSystemId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        internal string BusId;
    }

    internal interface INvmlApi : IDisposable
    {
        NvmlReturn Initialize();
        NvmlReturn Shutdown();
        NvmlReturn GetCount(out uint count);
        NvmlReturn GetDevice(uint index, out IntPtr device);
        NvmlReturn GetPciInfo(IntPtr device, out NvmlPciInfo info);
        NvmlReturn GetPowerValue(IntPtr device, NvmlPowerQuery query, out uint milliwatts);
        NvmlReturn GetPowerConstraints(IntPtr device, out uint minimum, out uint maximum);
    }

    internal sealed class NvmlNativeApi : INvmlApi
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate NvmlReturn LifecycleDelegate();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate NvmlReturn CountDelegate(out uint count);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate NvmlReturn DeviceDelegate(uint index, out IntPtr device);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate NvmlReturn PciDelegate(IntPtr device, out NvmlPciInfo info);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate NvmlReturn PowerDelegate(IntPtr device, out uint value);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate NvmlReturn ConstraintsDelegate(IntPtr device, out uint minimum, out uint maximum);

        private IntPtr _library;
        private readonly LifecycleDelegate? _initialize;
        private readonly LifecycleDelegate? _shutdown;
        private readonly CountDelegate? _count;
        private readonly DeviceDelegate? _device;
        private readonly PciDelegate? _pci;
        private readonly PowerDelegate? _usage;
        private readonly PowerDelegate? _defaultLimit;
        private readonly PowerDelegate? _currentLimit;
        private readonly PowerDelegate? _enforcedLimit;
        private readonly ConstraintsDelegate? _constraints;

        private NvmlNativeApi(IntPtr library)
        {
            _library = library;
            _initialize = GetExport<LifecycleDelegate>("nvmlInit_v2");
            _shutdown = GetExport<LifecycleDelegate>("nvmlShutdown");
            _count = GetExport<CountDelegate>("nvmlDeviceGetCount_v2");
            _device = GetExport<DeviceDelegate>("nvmlDeviceGetHandleByIndex_v2");
            _pci = GetExport<PciDelegate>("nvmlDeviceGetPciInfo_v3");
            _usage = GetExport<PowerDelegate>("nvmlDeviceGetPowerUsage");
            _defaultLimit = GetExport<PowerDelegate>("nvmlDeviceGetPowerManagementDefaultLimit");
            _currentLimit = GetExport<PowerDelegate>("nvmlDeviceGetPowerManagementLimit");
            _enforcedLimit = GetExport<PowerDelegate>("nvmlDeviceGetEnforcedPowerLimit");
            _constraints = GetExport<ConstraintsDelegate>("nvmlDeviceGetPowerManagementLimitConstraints");
        }

        internal static NvmlNativeApi? TryCreate()
        {
            // This backend augments a Windows NVAPI handle, rather than pretending
            // to make PhysicalGPU or the rest of NVAPI portable to Linux/ARM.
            if (!OperatingSystem.IsWindows() || !Environment.Is64BitProcess)
            {
                return null;
            }

            var candidates = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "nvml.dll"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "NVIDIA Corporation", "NVSMI", "nvml.dll")
            };

            foreach (var path in candidates)
            {
                // Resolve only known driver installation locations, not a working-directory DLL.
                if (!NativeLibrary.TryLoad(path, out var library))
                {
                    continue;
                }

                var api = new NvmlNativeApi(library);
                if (api._initialize != null && api._shutdown != null && api._count != null &&
                    api._device != null && api._pci != null)
                {
                    return api;
                }

                api.Dispose();
            }

            return null;
        }

        private T? GetExport<T>(string name) where T : Delegate =>
            NativeLibrary.TryGetExport(_library, name, out var pointer)
                ? Marshal.GetDelegateForFunctionPointer<T>(pointer)
                : null;

        public NvmlReturn Initialize() => _initialize!();
        public NvmlReturn Shutdown() => _shutdown!();
        public NvmlReturn GetCount(out uint count) => _count!(out count);
        public NvmlReturn GetDevice(uint index, out IntPtr device) => _device!(index, out device);
        public NvmlReturn GetPciInfo(IntPtr device, out NvmlPciInfo info) => _pci!(device, out info);

        public NvmlReturn GetPowerValue(IntPtr device, NvmlPowerQuery query, out uint milliwatts)
        {
            var function = query switch
            {
                NvmlPowerQuery.Usage => _usage,
                NvmlPowerQuery.DefaultLimit => _defaultLimit,
                NvmlPowerQuery.CurrentLimit => _currentLimit,
                NvmlPowerQuery.EnforcedLimit => _enforcedLimit,
                _ => null
            };
            milliwatts = 0;
            return function == null ? NvmlReturn.FunctionNotFound : function(device, out milliwatts);
        }

        public NvmlReturn GetPowerConstraints(IntPtr device, out uint minimum, out uint maximum)
        {
            minimum = maximum = 0;
            return _constraints == null ? NvmlReturn.FunctionNotFound : _constraints(device, out minimum, out maximum);
        }

        public void Dispose()
        {
            if (_library != IntPtr.Zero)
            {
                NativeLibrary.Free(_library);
                _library = IntPtr.Zero;
            }
        }
    }
}
