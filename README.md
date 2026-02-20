## <img src="NvAPIWrapper/Icon.png" width="24" alt="NvAPIWrapper"> NvAPIWrapper-V (Modernized NVIDIA NVAPI Wrapper)

Fork of [falahati/NvAPIWrapper](https://github.com/falahati/NvAPIWrapper) with safe modernization work focused on newer NVIDIA drivers/GPUs and better fallback behavior on older systems.

- Repository: `https://github.com/varun875/NvAPIWrapper-V`
- Package ID: `Varun.NvAPIWrapper.Net`
- Current package version in this repo: `8.9.0`
- Library targets: `netstandard2.1`, `net461`
- Library language version: C# `9.0`

## What Changed In This Fork

### 1. R580-era modern function mappings added
Modern IDs and delegates were wired for:

- `NvAPI_GPU_GetGspFeatures`
- `NvAPI_GPU_NVLINK_GetCaps`
- `NvAPI_SYS_GetDisplayDriverInfo`
- `NvAPI_SYS_GetPhysicalGPUs`
- `NvAPI_SYS_GetLogicalGPUs`
- `NvAPI_RegisterRiseCallback`
- `NvAPI_RequestRise`
- `NvAPI_UninstallRise`

### 2. Safe capability-first APIs (`Try*`) added
New `Try*` wrappers were added so unsupported drivers/GPUs do not throw in common telemetry paths:

- `GPUApi.TryGetGSPInfo`
- `GPUApi.TryGetNVLinkCaps`
- `GPUApi.TryGetPhysicalGPUHandleData`
- `GPUApi.TryGetLogicalGPUHandleData`
- `GPUApi.TryClientPowerTopologyGetStatus`
- `GPUApi.TryClientPowerPoliciesGetStatus`
- `GPUApi.TryClientPowerPoliciesGetInfo`
- `GPUApi.TryGetPerformancePoliciesStatus`
- `GPUApi.TryGetPerformanceDecreaseInfo`
- `GPUApi.TryGetCurrentPerformanceState`
- `GeneralApi.TryGetDisplayDriverInfo`
- `NVIDIA.TryGetDisplayDriverInfo`

### 3. High-level GPU improvements

- `PhysicalGPU.GetPhysicalGPUs()` and `LogicalGPU.GetLogicalGPUs()` now try modern metadata enumeration first, then fall back to legacy enumeration.
- Added `PhysicalGPU.GSPFirmwareVersion`
- Added `PhysicalGPU.NVLinkCapabilities`
- Added `PhysicalGPU.IsNVLinkSupported`
- Added power telemetry snapshot support:
  - `PhysicalGPU.TryGetPowerTelemetrySnapshot(out GPUPowerTelemetrySnapshot?)`
  - `PhysicalGPU.TryGetEstimatedBoardPowerUsageInWatts(...)`
  - `PhysicalGPU.TryGetEstimatedGPUPowerUsageInWatts(...)`
- Added safe thermal/usage helpers:
  - `GPUThermalInformation.TryGetCurrentThermalLevel(...)`
  - `GPUThermalInformation.TryGetThermalSensors(...)`
  - `GPUUsageInformation.TryGetUtilizationDomainsStatus(...)`
  - `GPUUsageInformation.TryGetDynamicPerformanceStatesEnabled(...)`
  - `GPUPowerTopologyInformation.TryGetPowerTopologyEntries(...)`
  - `GPUPowerTopologyInformation.TryGetPowerUsageInPercent(...)`
  - `GPUPowerTopologyInformation.TryGetEstimatedPowerUsageInWatts(...)`

### 4. Updated enum coverage for modern GPU reporting

- Memory: `GDDR6`, `GDDR6X`, `GDDR7`, `HBM2`, `HBM2e`, `HBM3`, `HBM3e`, `LPDDR5`, `DDR5`
- PCIe: `PCIe4`, `PCIe5` (plus display formatting updates)
- System: `Workstation`, `DataCenter`, `Hyperscale`, `Edge`
- Public clock domains: `BaseClock`, `VideoEncode`, `Tensor`, `Display`
- Voltage domains: `PCIeCore`, `SOCCore`, `Memory`

### 5. Packaging metadata modernized for NuGet

- Uses modern package metadata:
  - `<license type="file">`
  - `<icon>`
  - `<readme>`
- Package includes:
  - `LICENSE`
  - `README.md`
  - `Icon.png`

## Quick Start

```csharp
using NvAPIWrapper;
using NvAPIWrapper.GPU;

NVIDIA.Initialize();

foreach (var gpu in PhysicalGPU.GetPhysicalGPUs())
{
    Console.WriteLine(gpu.FullName);

    // Modern capability data with safe fallback behavior
    Console.WriteLine($"GSP FW: {gpu.GSPFirmwareVersion ?? "N/A"}");
    Console.WriteLine($"NVLink supported: {gpu.IsNVLinkSupported}");

    // Power telemetry snapshot (when available)
    if (gpu.TryGetPowerTelemetrySnapshot(out var snapshot) && snapshot != null)
    {
        Console.WriteLine($"GPU power %: {snapshot.GPUPowerUsageInPercent}");
        Console.WriteLine($"Board power %: {snapshot.BoardPowerUsageInPercent}");
    }
}

NVIDIA.Unload();
```

## Power In Watts (Important Note)

NVAPI commonly reports power telemetry as percentages. The new watt helpers estimate watts by multiplying:

`powerPercent * providedPowerLimitWatts / 100`

So these values are estimates and depend on the limit you provide (for example board TGP/TDP).

## Build And Pack

```powershell
dotnet build NvAPIWrapper\NvAPIWrapper.csproj -c Release -p:SignAssembly=false
dotnet pack NvAPIWrapper\NvAPIWrapper.csproj -c Release -p:SignAssembly=false
```

Artifacts are generated in `Release\`.

## API Surface

- `NvAPIWrapper.Display` - Display and display-control APIs
- `NvAPIWrapper.GPU` - GPU APIs and high-level telemetry helpers
- `NvAPIWrapper.Mosaic` - Mosaic/Surround APIs
- `NvAPIWrapper.DRS` - Driver profile APIs
- `NvAPIWrapper.Stereo` - Stereo APIs
- `NvAPIWrapper.Native` - Low-level NVAPI delegates/structures
- `NvAPIWrapper.NVIDIA` - General system entry points

## Compatibility Philosophy

This fork prioritizes:

1. Additive changes
2. Capability checks before feature usage
3. Legacy fallback paths to avoid regressions on older drivers/GPUs

## License

Copyright (C) 2017-2026 Soroush Falahati and contributors

This project is licensed under LGPL v3. See `LICENSE`.
