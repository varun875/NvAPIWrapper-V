# Release Notes — 9.0.2 / 9.0.3

Consolidated history of the RTX 40/50 modernization releases. Replaces the
overlapping per-pass summaries (`CHANGES_SUMMARY.md`, `COMPLETION_SUMMARY.md`,
`CODE_CHANGES_DETAILED.md`, `DOCUMENTATION_INDEX.md`, `IMPLEMENTATION_SUMMARY.md`,
`VISUAL_SUMMARY.md`), removed 2026-09-09. For the canonical per-version log see
[CHANGELOG.md](CHANGELOG.md); for usage see [QUICK_REFERENCE.md](QUICK_REFERENCE.md);
for the technical roadmap see [GPU_SUPPORT_UPGRADE_GUIDE.md](GPU_SUPPORT_UPGRADE_GUIDE.md).

## 9.0.3 — 2026-09-09

- New unit test project (`NvAPIWrapper.Tests`, net8.0) with 151 tests covering
  `GPUPowerSpecDatabase`, `GPUFamilyClassifier`, and enum value coverage.
- Fixed `GPUPowerSpecDatabase` longest-pattern-first matching (laptop GPU names
  such as "RTX 4090 Laptop GPU" previously matched the desktop entry) and added
  a bare `H100` fallback entry.
- Publish workflow reverted to `secrets.NUGET_API_KEY` auth.
- No breaking changes to the library API surface.

## 9.0.2 — 2026-03-08

Release-alignment pass: RTX 40 (Ada) / RTX 50 (Blackwell) enum coverage,
library pinned to `net6.0`, package `Varun.NvAPIWrapper.Net`.

### Code changes (all additive, 0 breaking changes)

| File | Change | Impact |
|------|--------|--------|
| `Native/GPU/GPUMemoryType.cs` | +9 values: GDDR6, GDDR6X, HBM2, HBM2e, GDDR7, HBM3, HBM3e, LPDDR5, DDR5 | Critical — RTX 40+ detection |
| `Native/GPU/GPUFoundry.cs` | +Samsung, IntelFoundryServices | Blackwell manufacturing tracking |
| `Native/GPU/SystemType.cs` | +Workstation, DataCenter, Hyperscale, Edge | GPU categorization |
| `Native/GPU/PublicClockDomain.cs` | +BaseClock, VideoEncode, Tensor, Display | Performance monitoring |
| `Native/GPU/PerformanceVoltageDomain.cs` | +PCIeCore, SOCCore, Memory (`Undefined` moved last) | Power control |
| `Native/GPU/PCIeGeneration.cs` | +PCIe4, PCIe5 | Future interconnects |
| `GPU/PCIeInformation.cs` | `ToString()` cases for PCIe 4.0/5.0 | Display formatting |
| `GPU/GPUFamilyClassifier.cs` (new) | `GPUFamily` (10) + `GPUTier` (7) enums, 7 static helpers (`DetectFamily`, `SupportsNVLink`, `UsesModernMemory`, `SupportsModernPCIe`, …) | GPU capability detection |

### GPU support added

- **RTX 40 series (Ada):** RTX 4050–4090 desktop + laptop, GDDR6/6X, PCIe 4.0
- **RTX 50 series (Blackwell):** RTX 5060–5090 desktop + laptop, GDDR7, PCIe 5.0
- **Professional:** RTX 6000/5880/5000/4500/4000 Ada, RTX A6000–A4000
- **Data center:** H100 (SXM/PCIe/NVL), H200, A100, A40/A30/A16/A10/A2, L40S/L40/L4
- **Edge:** Orin (LPDDR5)

### Quick usage

```csharp
var family = GPUFamilyClassifier.DetectFamily(archInfo.ShortName);
bool isModern = GPUFamilyClassifier.UsesModernMemory(memInfo.RAMType);
bool hasNVLink = GPUFamilyClassifier.SupportsNVLink(family);
double? tdp = GPUPowerSpecDatabase.GetDefaultTDP(gpu.FullName);
```

### Known limitations

- Architecture detection relies on NVAPI `GetShortName()` — update NVIDIA drivers
  if new GPUs aren't recognized.
- Newer functions (PCIe 5.0 status, modern power modes) may need NVAPI R545+;
  this release stays compatible with R410.

### Deployment

1. `dotnet build -c Release`, `dotnet test`, `dotnet pack -c Release`
2. Push `Varun.NvAPIWrapper.Net.*.nupkg` to NuGet (CI does this on `v*` tags)
3. Create the GitHub release from [CHANGELOG.md](CHANGELOG.md)

### Still open (future phases)

- NVLink topology detection, NVAPI R545+ refresh, extended power-state/thermal APIs.
