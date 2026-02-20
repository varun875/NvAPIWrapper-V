#nullable enable

using System;
using System.Linq;
using NvAPIWrapper.Native;
using NvAPIWrapper.Native.GPU;

namespace NvAPIWrapper.GPU
{
    /// <summary>
    ///     GPU power state and performance monitoring for modern architectures (Ada/Blackwell+).
    ///     Provides a comprehensive, unified view of GPU power, thermal, and clock status.
    ///     Call <see cref="Refresh"/> to update all values from the GPU.
    /// </summary>
    public class GPUPowerStateMonitoring
    {
        /// <summary>
        ///     Boost clock configuration and status
        /// </summary>
        public class BoostClockDetails
        {
            /// <summary>
            ///     Current boost clock in MHz
            /// </summary>
            public uint CurrentBoostClockMHz { get; set; }

            /// <summary>
            ///     Maximum boost clock in MHz (hardware limit)
            /// </summary>
            public uint MaxBoostClockMHz { get; set; }

            /// <summary>
            ///     Base boost clock in MHz (firmware default)
            /// </summary>
            public uint BaseBoostClockMHz { get; set; }

            /// <summary>
            ///     Current boost offset applied in MHz
            /// </summary>
            public int BoostOffsetMHz { get; set; }

            /// <summary>
            ///     Whether boost is being thermally throttled
            /// </summary>
            public bool IsThrottledByTemperature { get; set; }

            /// <summary>
            ///     Whether boost is being power throttled
            /// </summary>
            public bool IsThrottledByPower { get; set; }

            public override string ToString() =>
                $"Boost Clock: {CurrentBoostClockMHz}MHz (max: {MaxBoostClockMHz}MHz, offset: {BoostOffsetMHz:+0;-#}MHz)" +
                (IsThrottledByTemperature ? " [TEMP_THROTTLED]" : string.Empty) +
                (IsThrottledByPower ? " [POWER_THROTTLED]" : string.Empty);
        }

        /// <summary>
        ///     Power limit configuration for modern GPUs
        /// </summary>
        public class PowerLimitDetails
        {
            /// <summary>
            ///     Current power consumption in watts (auto-resolved from TDP database).
            ///     -1 if unavailable.
            /// </summary>
            public double CurrentPowerW { get; set; }

            /// <summary>
            ///     Default TDP in watts (from built-in database).
            ///     -1 if unknown GPU.
            /// </summary>
            public double DefaultTDPW { get; set; }

            /// <summary>
            ///     Currently active power limit in watts (accounts for user slider).
            ///     -1 if unavailable.
            /// </summary>
            public double ActivePowerLimitW { get; set; }

            /// <summary>
            ///     Power consumption as percentage of limit (0-100+)
            /// </summary>
            public double PowerUtilizationPercent { get; set; }

            /// <summary>
            ///     Whether power limit is being exceeded (throttling triggered)
            /// </summary>
            public bool IsExceedingLimit { get; set; }

            /// <summary>
            ///     Whether TDP information was found in the built-in database.
            /// </summary>
            public bool IsTDPKnown { get; set; }

            public override string ToString()
            {
                if (!IsTDPKnown)
                {
                    return $"Power: {PowerUtilizationPercent:F1}% (TDP unknown - use GPUPowerSpecDatabase.RegisterSpec)" +
                           (IsExceedingLimit ? " [EXCEEDING]" : string.Empty);
                }

                return $"Power: {CurrentPowerW:F1}W / {ActivePowerLimitW:F1}W ({PowerUtilizationPercent:F1}%)" +
                       (IsExceedingLimit ? " [EXCEEDING]" : string.Empty);
            }
        }

        /// <summary>
        ///     Thermal throttling status and limits
        /// </summary>
        public class ThermalThrottleDetails
        {
            /// <summary>
            ///     Current GPU temperature in Celsius
            /// </summary>
            public int CurrentTemperatureC { get; set; }

            /// <summary>
            ///     Thermal throttle activation temperature in Celsius
            /// </summary>
            public int ThrottleActivationTempC { get; set; }

            /// <summary>
            ///     Thermal shutdown temperature in Celsius
            /// </summary>
            public int ShutdownTemperatureC { get; set; }

            /// <summary>
            ///     Is thermal throttling currently active
            /// </summary>
            public bool IsThrottlingActive { get; set; }

            /// <summary>
            ///     Percentage of thermal headroom (0-100, 0 means at limit)
            /// </summary>
            public double ThermalHeadroomPercent { get; set; }

            /// <summary>
            ///     Number of thermal slowdown events since last reset
            /// </summary>
            public uint ThrottleEventCount { get; set; }

            public override string ToString() =>
                $"Temperature: {CurrentTemperatureC}C (throttle at {ThrottleActivationTempC}C, shutdown at {ShutdownTemperatureC}C)" +
                (IsThrottlingActive ? " [THROTTLING]" : $" [OK] ({ThermalHeadroomPercent:F1}% headroom)");
        }

        /// <summary>
        ///     Dynamic performance state information
        /// </summary>
        public enum DynamicPState
        {
            /// <summary>
            ///     Unknown or unsupported state
            /// </summary>
            Unknown = 0,

            /// <summary>
            ///     P0 - Maximum performance state (full boost)
            /// </summary>
            P0 = 1,

            /// <summary>
            ///     P1 - High performance state
            /// </summary>
            P1 = 2,

            /// <summary>
            ///     P2 - Balanced performance/power state
            /// </summary>
            P2 = 3,

            /// <summary>
            ///     P3 - Power saving state
            /// </summary>
            P3 = 4,

            /// <summary>
            ///     P4 - Minimal power consumption state (idle)
            /// </summary>
            P4 = 5,

            /// <summary>
            ///     P5 - Deep sleep/off state
            /// </summary>
            P5 = 6,

            /// <summary>
            ///     Throttled state - Performance reduced due to thermal/power
            /// </summary>
            PThrottled = 255
        }

        /// <summary>
        ///     Initialize for a physical GPU
        /// </summary>
        public GPUPowerStateMonitoring(PhysicalGPU gpu)
        {
            PhysicalGPU = gpu;
            BoostClockInfo = new BoostClockDetails();
            PowerLimitStatus = new PowerLimitDetails();
            ThermalThrottleStatus = new ThermalThrottleDetails();
        }

        /// <summary>
        ///     Reference to the physical GPU
        /// </summary>
        public PhysicalGPU PhysicalGPU { get; }

        /// <summary>
        ///     Current dynamic performance state
        /// </summary>
        public DynamicPState CurrentPerformanceState { get; set; } = DynamicPState.Unknown;

        /// <summary>
        ///     Boost clock information
        /// </summary>
        public BoostClockDetails BoostClockInfo { get; }

        /// <summary>
        ///     Power limit status
        /// </summary>
        public PowerLimitDetails PowerLimitStatus { get; }

        /// <summary>
        ///     Thermal throttle information
        /// </summary>
        public ThermalThrottleDetails ThermalThrottleStatus { get; }

        /// <summary>
        ///     Is GPU currently in a throttled state
        /// </summary>
        public bool IsThrottled =>
            BoostClockInfo.IsThrottledByTemperature ||
            BoostClockInfo.IsThrottledByPower ||
            ThermalThrottleStatus.IsThrottlingActive ||
            CurrentPerformanceState == DynamicPState.PThrottled;

        /// <summary>
        ///     GPU operating state (performance vs efficiency)
        /// </summary>
        public string OperatingMode => CurrentPerformanceState switch
        {
            DynamicPState.P0 => "Full Performance (P0)",
            DynamicPState.P1 => "High Performance (P1)",
            DynamicPState.P2 => "Balanced (P2)",
            DynamicPState.P3 => "Power Saving (P3)",
            DynamicPState.P4 => "Minimal Power (P4)",
            DynamicPState.P5 => "Sleep (P5)",
            DynamicPState.PThrottled => "Throttled",
            _ => "Unknown"
        };

        /// <summary>
        ///     Get overall system health status
        /// </summary>
        public string HealthStatus
        {
            get
            {
                if (IsThrottled && PowerLimitStatus.IsExceedingLimit && ThermalThrottleStatus.IsThrottlingActive)
                {
                    return "CRITICAL: Thermal AND Power Throttling";
                }

                if (IsThrottled && PowerLimitStatus.IsExceedingLimit)
                {
                    return "WARNING: Power Throttling Active";
                }

                if (IsThrottled && ThermalThrottleStatus.IsThrottlingActive)
                {
                    return "WARNING: Thermal Throttling Active";
                }

                if (IsThrottled)
                {
                    return "CAUTION: Performance Throttled";
                }

                if (PowerLimitStatus.PowerUtilizationPercent > 90)
                {
                    return "CAUTION: High Power Utilization (>90%)";
                }

                if (ThermalThrottleStatus.ThermalHeadroomPercent < 10)
                {
                    return "CAUTION: Low Thermal Headroom (<10%)";
                }

                return "HEALTHY";
            }
        }

        /// <summary>
        ///     Refreshes all telemetry values from the GPU using NVAPI calls.
        ///     This populates BoostClockInfo, PowerLimitStatus, ThermalThrottleStatus, and CurrentPerformanceState.
        /// </summary>
        /// <returns>True if at least some telemetry was successfully retrieved.</returns>
        public bool Refresh()
        {
            var anySuccess = false;

            // Refresh performance state
            if (GPUApi.TryGetCurrentPerformanceState(PhysicalGPU.Handle, out var perfState))
            {
                CurrentPerformanceState = MapPerformanceState(perfState);
                anySuccess = true;
            }

            // Refresh power telemetry
            if (PhysicalGPU.TryGetPowerTelemetrySnapshot(out var snapshot) && snapshot != null)
            {
                anySuccess = true;

                // Power percentages
                PowerLimitStatus.PowerUtilizationPercent = snapshot.BoardPowerUsageInPercent ?? 0;
                PowerLimitStatus.IsTDPKnown = snapshot.IsTDPKnown;
                PowerLimitStatus.IsExceedingLimit = snapshot.IsPowerLimitActive;
                BoostClockInfo.IsThrottledByPower = snapshot.IsPowerLimitActive;
                BoostClockInfo.IsThrottledByTemperature = snapshot.IsThermalLimitActive;

                // Watt-based values
                PowerLimitStatus.DefaultTDPW = snapshot.DefaultTDPInWatts ?? -1;
                PowerLimitStatus.CurrentPowerW = snapshot.BoardPowerDrawInWatts ?? -1;
                PowerLimitStatus.ActivePowerLimitW = snapshot.CurrentPowerLimitInWatts ?? -1;
            }

            // Refresh clocks
            try
            {
                var currentClocks = PhysicalGPU.CurrentClockFrequencies;
                var boostClocks = PhysicalGPU.BoostClockFrequencies;
                var baseClocks = PhysicalGPU.BaseClockFrequencies;

                if (currentClocks.GraphicsClock.IsPresent)
                {
                    BoostClockInfo.CurrentBoostClockMHz = currentClocks.GraphicsClock.Frequency / 1000;
                    anySuccess = true;
                }

                if (boostClocks.GraphicsClock.IsPresent)
                {
                    BoostClockInfo.MaxBoostClockMHz = boostClocks.GraphicsClock.Frequency / 1000;
                }

                if (baseClocks.GraphicsClock.IsPresent)
                {
                    BoostClockInfo.BaseBoostClockMHz = baseClocks.GraphicsClock.Frequency / 1000;
                }

                BoostClockInfo.BoostOffsetMHz = (int)(BoostClockInfo.CurrentBoostClockMHz - BoostClockInfo.BaseBoostClockMHz);
            }
            catch
            {
                // Clock queries may fail on some GPUs/drivers
            }

            // Refresh thermal
            try
            {
                var thermalSensors = PhysicalGPU.ThermalInformation.ThermalSensors.ToArray();

                if (thermalSensors.Length > 0)
                {
                    var gpuSensor = thermalSensors[0]; // First sensor is usually the GPU core

                    ThermalThrottleStatus.CurrentTemperatureC = gpuSensor.CurrentTemperature;
                    ThermalThrottleStatus.ThrottleActivationTempC = gpuSensor.DefaultMaximumTemperature;
                    ThermalThrottleStatus.ShutdownTemperatureC = gpuSensor.DefaultMaximumTemperature + 10; // Estimate

                    if (gpuSensor.DefaultMaximumTemperature > 0)
                    {
                        ThermalThrottleStatus.ThermalHeadroomPercent =
                            Math.Max(0, (1.0 - (double)gpuSensor.CurrentTemperature / gpuSensor.DefaultMaximumTemperature) * 100);
                    }

                    ThermalThrottleStatus.IsThrottlingActive =
                        gpuSensor.CurrentTemperature >= gpuSensor.DefaultMaximumTemperature;

                    anySuccess = true;
                }
            }
            catch
            {
                // Thermal queries may fail on some GPUs/drivers
            }

            return anySuccess;
        }

        private static DynamicPState MapPerformanceState(PerformanceStateId stateId)
        {
            return stateId switch
            {
                PerformanceStateId.P0_3DPerformance => DynamicPState.P0,
                PerformanceStateId.P1_3DPerformance => DynamicPState.P1,
                PerformanceStateId.P2_Balanced => DynamicPState.P2,
                PerformanceStateId.P3_Balanced => DynamicPState.P3,
                PerformanceStateId.P4 => DynamicPState.P4,
                PerformanceStateId.P5 => DynamicPState.P5,
                _ => DynamicPState.Unknown
            };
        }

        /// <summary>
        ///     Get comprehensive status summary
        /// </summary>
        public override string ToString() =>
            $"GPU {PhysicalGPU.GPUId}: {OperatingMode}\n" +
            $"  {BoostClockInfo}\n" +
            $"  {PowerLimitStatus}\n" +
            $"  {ThermalThrottleStatus}\n" +
            $"  Health: {HealthStatus}";
    }
}
