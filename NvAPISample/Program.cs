using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleUtilities;
using NvAPIWrapper;
using NvAPIWrapper.Display;
using NvAPIWrapper.GPU;
using NvAPIWrapper.Mosaic;

namespace NvAPISample
{
    internal class Program
    {
        // ReSharper disable once TooManyDeclarations
        private static void Main()
        {
            NVIDIA.Initialize();
            var navigation = new Dictionary<object, Action>
            {
                {"Connected Displays", PrintConnectedDisplays},
                {"Disconnected Displays", PrintDisconnectedDisplays},
                {"Display Configurations", PrintDisplayPathInformation},
                {"Physical GPUs", PrintPhysicalGPUs},
                {"GPU Temperatures", PrintGPUSensors},
                {"GPU Coolers", PrintGPUCoolers},
                {"GPU Power Telemetry", PrintGPUPowerTelemetry},
                {"GPU Performance States", PrintGPUPerformanceStates},
                {"TCC GPUs", PrintTCCGPUs},
                {"Grid Topologies (Mosaic - NVIDIA Surround)", PrintGridTopologies},
                {"NVIDIA Driver and API version", PrintDriverVersion},
                {"System Chipset Information", PrintChipsetInformation},
                {"Lid and Dock Information", PrintDockInformation}
            };

            ConsoleNavigation.Default.PrintNavigation(
                navigation.Keys.ToArray(),
                (i, o) => navigation[o](),
                "Select an execution line to browse NvAPIWrapper functionalities."
            );
        }

        private static void PrintChipsetInformation()
        {
            ConsoleWriter.Default.PrintCaption("NVIDIA.ChipsetInfo");
            ConsoleWriter.Default.WriteObject(new
            {
                NVIDIA.ChipsetInfo.ChipsetName,
                NVIDIA.ChipsetInfo.DeviceId,
                NVIDIA.ChipsetInfo.Flags,
                NVIDIA.ChipsetInfo.VendorId,
                NVIDIA.ChipsetInfo.VendorName
            });
        }

        private static void PrintConnectedDisplays()
        {
            ConsoleWriter.Default.PrintCaption("Display.GetDisplays()");
            ConsoleNavigation.Default.PrintNavigation(Display.GetDisplays(),
                (i, display) => ConsoleWriter.Default.WriteObject(display),
                "Select a display to show device information");
        }

        private static void PrintDisconnectedDisplays()
        {
            ConsoleWriter.Default.PrintCaption("UnAttachedDisplay.GetUnAttachedDisplays()");
            ConsoleNavigation.Default.PrintNavigation(UnAttachedDisplay.GetUnAttachedDisplays(),
                (i, unAttachedDisplay) => ConsoleWriter.Default.WriteObject(unAttachedDisplay, 0),
                "Select a display to show additional information");
        }

        private static void PrintDisplayPathInformation()
        {
            ConsoleWriter.Default.PrintCaption("PathInfo.GetDisplaysConfig()");
            ConsoleNavigation.Default.PrintNavigation(PathInfo.GetDisplaysConfig().ToArray(), (i, info) =>
            {
                ConsoleWriter.Default.WriteObject(info, 2);
            }, "Select a path info to show additional information");
        }

        private static void PrintDockInformation()
        {
            ConsoleWriter.Default.PrintCaption("NVIDIA.LidAndDockParameters");
            ConsoleWriter.Default.WriteObject(new
            {
                NVIDIA.LidAndDockParameters.CurrentDockPolicy,
                NVIDIA.LidAndDockParameters.CurrentDockState,
                NVIDIA.LidAndDockParameters.CurrentLidPolicy,
                NVIDIA.LidAndDockParameters.CurrentLidState,
                NVIDIA.LidAndDockParameters.ForcedDockMechanismPresent,
                NVIDIA.LidAndDockParameters.ForcedLidMechanismPresent
            });
        }

        private static void PrintDriverVersion()
        {
            ConsoleWriter.Default.PrintCaption("NVIDIA");
            ConsoleWriter.Default.WriteObject(new
            {
                NVIDIA.DriverVersion,
                NVIDIA.DriverBranchVersion,
                NVIDIA.InterfaceVersionString
            });
        }

        private static void PrintGPUCoolers()
        {
            ConsoleWriter.Default.PrintCaption("PhysicalGPU.GetPhysicalGPUs()");
            ConsoleNavigation.Default.PrintNavigation(PhysicalGPU.GetPhysicalGPUs(), (i, gpu) =>
            {
                ConsoleWriter.Default.PrintCaption("PhysicalGPU.CoolerInformation");
                ConsoleWriter.Default.WriteObject(gpu.CoolerInformation.Coolers.ToArray());
            }, "Select a GPU to show cooler values");
        }

        private static void PrintGPUPerformanceStates()
        {
            ConsoleWriter.Default.PrintCaption("PhysicalGPU.GetPhysicalGPUs()");
            ConsoleNavigation.Default.PrintNavigation(PhysicalGPU.GetPhysicalGPUs(), (i, gpu) =>
            {
                ConsoleWriter.Default.PrintCaption("PhysicalGPU.PerformanceStatesInfo");
                ConsoleWriter.Default.WriteObject(gpu.PerformanceStatesInfo, 3);
            }, "Select a GPU to show performance states configuration");
        }

        private static void PrintGPUPowerTelemetry()
        {
            ConsoleWriter.Default.PrintCaption("PhysicalGPU.GetPhysicalGPUs()");
            ConsoleNavigation.Default.PrintNavigation(PhysicalGPU.GetPhysicalGPUs(), (i, gpu) =>
            {
                ConsoleWriter.Default.PrintCaption("PhysicalGPU.TryGetPowerTelemetrySnapshot");

                if (!gpu.TryGetPowerTelemetrySnapshot(out var snapshot) || snapshot == null)
                {
                    Console.WriteLine("Power telemetry is not available on this GPU/driver.");
                    return;
                }

                // Basic telemetry (percentage-based)
                ConsoleWriter.Default.PrintCaption("Power Usage (Percentage)");
                ConsoleWriter.Default.WriteObject(new
                {
                    snapshot.CapturedAtUtc,
                    snapshot.CurrentPerformanceState,
                    snapshot.CurrentPerformanceLimit,
                    snapshot.PerformanceDecreaseReason,
                    GPUPowerPercent = snapshot.GPUPowerUsageInPercent?.ToString("F1") + "%" ?? "N/A",
                    BoardPowerPercent = snapshot.BoardPowerUsageInPercent?.ToString("F1") + "%" ?? "N/A",
                    ActivePowerTarget = snapshot.ActivePowerTargetInPercent?.ToString("F1") + "%" ?? "N/A",
                    DefaultPowerTarget = snapshot.DefaultPowerTargetInPercent?.ToString("F1") + "%" ?? "N/A",
                    MinPowerTarget = snapshot.MinimumPowerTargetInPercent?.ToString("F1") + "%" ?? "N/A",
                    MaxPowerTarget = snapshot.MaximumPowerTargetInPercent?.ToString("F1") + "%" ?? "N/A",
                });

                // Watt-based readings (automatic from TDP database)
                ConsoleWriter.Default.PrintCaption("Power Usage (Watts) - Auto-resolved from GPU TDP Database");
                if (snapshot.IsTDPKnown)
                {
                    ConsoleWriter.Default.WriteObject(new
                    {
                        MatchedArchitecture = snapshot.MatchedArchitecture ?? "Unknown",
                        DefaultTDP = snapshot.DefaultTDPInWatts?.ToString("F0") + " W" ?? "N/A",
                        CurrentPowerLimit = snapshot.CurrentPowerLimitInWatts?.ToString("F1") + " W" ?? "N/A",
                        DefaultPowerLimit = snapshot.DefaultPowerLimitInWatts?.ToString("F1") + " W" ?? "N/A",
                        MinPowerLimit = snapshot.MinimumPowerLimitInWatts?.ToString("F1") + " W" ?? "N/A",
                        MaxPowerLimit = snapshot.MaximumPowerLimitInWatts?.ToString("F1") + " W" ?? "N/A",
                        BoardPowerDraw = snapshot.BoardPowerDrawInWatts?.ToString("F1") + " W" ?? "N/A",
                        GPUPowerDraw = snapshot.GPUPowerDrawInWatts?.ToString("F1") + " W" ?? "N/A",
                    });
                }
                else
                {
                    Console.WriteLine($"  GPU '{gpu.FullName}' is not in the built-in TDP database.");
                    Console.WriteLine("  Use GPUPowerSpecDatabase.RegisterSpec() to add support.");
                    Console.WriteLine("  Or use TryEstimateBoardPowerInWatts(tdpWatts) with a manual TDP value.");
                }

                // Throttle status
                ConsoleWriter.Default.PrintCaption("Throttle Status");
                ConsoleWriter.Default.WriteObject(new
                {
                    snapshot.ThrottleStatus,
                    snapshot.IsPowerLimitActive,
                    snapshot.IsThermalLimitActive,
                    snapshot.IsVoltageLimitActive,
                    snapshot.IsNoLoadLimitActive,
                });

                // Raw topology entries
                ConsoleWriter.Default.PrintCaption("Raw Power Topology Entries");
                ConsoleWriter.Default.WriteObject(snapshot.PowerTopologyEntries.ToArray());
            }, "Select a GPU to show power telemetry");
        }

        private static void PrintGPUSensors()
        {
            ConsoleWriter.Default.PrintCaption("PhysicalGPU.GetPhysicalGPUs()");
            ConsoleNavigation.Default.PrintNavigation(PhysicalGPU.GetPhysicalGPUs(), (i, gpu) =>
            {
                ConsoleWriter.Default.PrintCaption("PhysicalGPU.ThermalSensors");
                ConsoleWriter.Default.WriteObject(gpu.ThermalInformation.ThermalSensors.ToArray());
            }, "Select a GPU to show thermal sensor values");
        }

        private static void PrintGridTopologies()
        {
            ConsoleWriter.Default.PrintCaption("GridTopology.GetGridTopologies()");
            ConsoleNavigation.Default.PrintNavigation(GridTopology.GetGridTopologies(), (i, topology) =>
            {
                ConsoleWriter.Default.WriteObject(topology, 3);
            }, "Select a grid topology to show additional information");
        }

        private static void PrintPhysicalGPUs()
        {
            ConsoleWriter.Default.PrintCaption("PhysicalGPU.GetPhysicalGPUs()");
            ConsoleNavigation.Default.PrintNavigation(PhysicalGPU.GetPhysicalGPUs(), (i, gpu) =>
            {
                ConsoleWriter.Default.WriteObject(gpu, 0);
            }, "Select a GPU to show additional information");
        }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once IdentifierTypo
        private static void PrintTCCGPUs()
        {
            ConsoleWriter.Default.PrintCaption("PhysicalGPU.GetTCCPhysicalGPUs()");
            ConsoleNavigation.Default.PrintNavigation(PhysicalGPU.GetTCCPhysicalGPUs(), (i, gpu) =>
            {
                ConsoleWriter.Default.WriteObject(gpu, 0);
            }, "Select a GPU to show additional information");
        }
    }
}
