using System;
using System.Collections.Generic;

namespace NvAPIWrapper.Native.Power
{
    internal readonly struct PowerDeviceIdentity
    {
        internal PowerDeviceIdentity(uint bus, uint deviceId, uint subSystemId)
        {
            Bus = bus;
            DeviceId = deviceId;
            SubSystemId = subSystemId;
        }

        internal uint Bus { get; }
        internal uint DeviceId { get; }
        internal uint SubSystemId { get; }

        internal bool Matches(NvmlPciInfo pci) => pci.Bus == Bus && pci.DeviceId == DeviceId &&
            (SubSystemId == 0 || pci.SubSystemId == 0 || pci.SubSystemId == SubSystemId);
    }

    internal sealed class NvmlPowerData
    {
        internal double? UsageWatts { get; init; }
        internal double? DefaultLimitWatts { get; init; }
        internal double? CurrentLimitWatts { get; init; }
        internal double? EnforcedLimitWatts { get; init; }
        internal double? MinimumLimitWatts { get; init; }
        internal double? MaximumLimitWatts { get; init; }
        internal string? PciAddress { get; init; }
        internal IReadOnlyList<string> Diagnostics { get; init; } = Array.Empty<string>();

        internal bool HasAnyData => UsageWatts.HasValue || DefaultLimitWatts.HasValue ||
            CurrentLimitWatts.HasValue || EnforcedLimitWatts.HasValue || MinimumLimitWatts.HasValue || MaximumLimitWatts.HasValue;

        internal static NvmlPowerData Unavailable(string reason) => new NvmlPowerData
        {
            Diagnostics = Array.AsReadOnly(new[] { reason })
        };
    }

    internal sealed class NvmlPowerProvider : IDisposable
    {
        private static readonly Lazy<NvmlPowerProvider> Shared = new Lazy<NvmlPowerProvider>(CreateDefault);
        private readonly object _sync = new object();
        private readonly INvmlApi? _api;
        private bool _initialized;
        private bool _disposed;

        internal static NvmlPowerProvider Default => Shared.Value;

        internal NvmlPowerProvider(INvmlApi? api)
        {
            _api = api;
        }

        private static NvmlPowerProvider CreateDefault()
        {
            var provider = new NvmlPowerProvider(NvmlNativeApi.TryCreate());
            // Own one NVML initialization reference for the process lifetime. Other
            // libraries' NVML references are unaffected by the matching shutdown.
            AppDomain.CurrentDomain.ProcessExit += (_, _) => provider.Dispose();
            return provider;
        }

        internal NvmlPowerData Read(PowerDeviceIdentity identity)
        {
            lock (_sync)
            {
                if (_disposed || _api == null)
                {
                    return NvmlPowerData.Unavailable("NVML is unavailable (requires the NVIDIA NVML driver library on Windows x64).");
                }

                if (!_initialized)
                {
                    var initializeStatus = _api.Initialize();
                    if (initializeStatus != NvmlReturn.Success)
                    {
                        return NvmlPowerData.Unavailable($"NVML initialization: {initializeStatus}.");
                    }

                    _initialized = true;
                }

                // Re-enumerate to avoid stale handles after a driver reset. NVAPI
                // and NVML enumeration indices are never assumed to correspond.
                var countStatus = _api.GetCount(out var count);
                if (countStatus != NvmlReturn.Success)
                {
                    if (countStatus == NvmlReturn.Uninitialized) _initialized = false;
                    return NvmlPowerData.Unavailable($"NVML device enumeration: {countStatus}.");
                }

                var matchedDevice = IntPtr.Zero;
                string? pciAddress = null;
                for (uint i = 0; i < count; i++)
                {
                    var status = _api.GetDevice(i, out var device);
                    if (status != NvmlReturn.Success)
                    {
                        return NvmlPowerData.Unavailable($"NVML device {i}: {status}; cannot establish an unambiguous device match.");
                    }

                    status = _api.GetPciInfo(device, out var pci);
                    if (status != NvmlReturn.Success)
                    {
                        return NvmlPowerData.Unavailable($"NVML PCI identity {i}: {status}; cannot establish an unambiguous device match.");
                    }

                    if (!identity.Matches(pci)) continue;
                    if (matchedDevice != IntPtr.Zero)
                    {
                        // NVAPI does not expose a PCI domain here. Reject two otherwise
                        // identical matches rather than return another board's readings.
                        return NvmlPowerData.Unavailable("NVML PCI identity is ambiguous across devices; power data was not attributed.");
                    }

                    matchedDevice = device;
                    pciAddress = pci.BusId;
                }

                if (matchedDevice == IntPtr.Zero)
                {
                    return NvmlPowerData.Unavailable("No NVML device matches the NVAPI PCI identity.");
                }

                var diagnostics = new List<string>();
                var deviceLost = false;
                double? ReadValue(NvmlPowerQuery query)
                {
                    var status = _api.GetPowerValue(matchedDevice, query, out var milliwatts);
                    if (status == NvmlReturn.Uninitialized) _initialized = false;
                    if (status == NvmlReturn.GpuLost || status == NvmlReturn.Uninitialized) deviceLost = true;
                    if (status != NvmlReturn.Success)
                    {
                        diagnostics.Add($"NVML {query}: {status}.");
                        return null;
                    }

                    if (milliwatts == uint.MaxValue || (query != NvmlPowerQuery.Usage && milliwatts == 0))
                    {
                        diagnostics.Add($"NVML {query}: invalid watt value.");
                        return null;
                    }

                    return milliwatts / 1000d;
                }

                var usage = ReadValue(NvmlPowerQuery.Usage);
                var defaultLimit = ReadValue(NvmlPowerQuery.DefaultLimit);
                var currentLimit = ReadValue(NvmlPowerQuery.CurrentLimit);
                var enforcedLimit = ReadValue(NvmlPowerQuery.EnforcedLimit);
                double? minimum = null, maximum = null;
                var constraintsStatus = _api.GetPowerConstraints(matchedDevice, out var min, out var max);
                if (constraintsStatus == NvmlReturn.Uninitialized) _initialized = false;
                if (constraintsStatus == NvmlReturn.GpuLost || constraintsStatus == NvmlReturn.Uninitialized) deviceLost = true;
                if (constraintsStatus == NvmlReturn.Success && min > 0 && min <= max && max != uint.MaxValue)
                {
                    minimum = min / 1000d;
                    maximum = max / 1000d;
                }
                else
                {
                    diagnostics.Add($"NVML power constraints: {(constraintsStatus == NvmlReturn.Success ? "invalid range" : constraintsStatus.ToString())}.");
                }

                if (deviceLost)
                {
                    return NvmlPowerData.Unavailable("NVML device became unavailable during capture; partial readings were discarded.");
                }

                return new NvmlPowerData
                {
                    UsageWatts = usage,
                    DefaultLimitWatts = defaultLimit,
                    CurrentLimitWatts = currentLimit,
                    EnforcedLimitWatts = enforcedLimit,
                    MinimumLimitWatts = minimum,
                    MaximumLimitWatts = maximum,
                    PciAddress = pciAddress,
                    Diagnostics = diagnostics.AsReadOnly()
                };
            }
        }

        public void Dispose()
        {
            lock (_sync)
            {
                if (_disposed) return;
                if (_initialized) _api!.Shutdown();
                _api?.Dispose();
                _initialized = false;
                _disposed = true;
            }
        }
    }
}
