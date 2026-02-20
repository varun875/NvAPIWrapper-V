#nullable enable

using System;
using System.Linq;

namespace NvAPIWrapper.GPU
{
    /// <summary>
    ///     Advanced thermal sensor management for modern GPU architectures (Ada, Hopper)
    /// </summary>
    public class GPUThermalSensorConfiguration
    {
        /// <summary>
        ///     Types of thermal sensors available in modern GPUs
        /// </summary>
        public enum SensorType
        {
            /// <summary>
            ///     Unknown sensor type
            /// </summary>
            Unknown = 0,

            /// <summary>
            ///     GPU die temperature sensor
            /// </summary>
            GPUCore = 1,

            /// <summary>
            ///     Memory (HBM/GDDR) temperature sensor
            /// </summary>
            Memory = 2,

            /// <summary>
            ///     Power delivery network (VRM) temperature sensor
            /// </summary>
            PowerDelivery = 3,

            /// <summary>
            ///     Vapor chamber temperature sensor (liquid cooling)
            /// </summary>
            VaporChamber = 4,

            /// <summary>
            ///     Board ambient temperature sensor
            /// </summary>
            Ambient = 5,

            /// <summary>
            ///     Liquid inlet temperature (AIO coolers)
            /// </summary>
            LiquidInlet = 6,

            /// <summary>
            ///     Liquid outlet temperature (AIO coolers)
            /// </summary>
            LiquidOutlet = 7,

            /// <summary>
            ///     Skin temperature (external case)
            /// </summary>
            Skin = 8,

            /// <summary>
            ///     Inductor/Choke temperature (power delivery)
            /// </summary>
            Inductor = 9
        }

        /// <summary>
        ///     Thermal zone location on GPU (Ada+ added multiple zones)
        /// </summary>
        public enum ThermalZone
        {
            /// <summary>
            ///     Unknown zone
            /// </summary>
            Unknown = 0,

            /// <summary>
            ///     Primary/main GPU core zone
            /// </summary>
            CorePrimary = 1,

            /// <summary>
            ///     Secondary GPU core zone (multi-domain GPUs)
            /// </summary>
            CoreSecondary = 2,

            /// <summary>
            ///     HBM/Memory module zone 0
            /// </summary>
            HBMZone0 = 10,

            /// <summary>
            ///     HBM/Memory module zone 1
            /// </summary>
            HBMZone1 = 11,

            /// <summary>
            ///     HBM/Memory module zone 2
            /// </summary>
            HBMZone2 = 12,

            /// <summary>
            ///     HBM/Memory module zone 3
            /// </summary>
            HBMZone3 = 13,

            /// <summary>
            ///     Front/Input side thermal zone
            /// </summary>
            Front = 20,

            /// <summary>
            ///     Back/Output side thermal zone
            /// </summary>
            Back = 21,

            /// <summary>
            ///     Internal enclosure zone
            /// </summary>
            Internal = 22,

            /// <summary>
            ///     External case/skin zone
            /// </summary>
            External = 23
        }

        /// <summary>
        ///     Single thermal sensor configuration and status
        /// </summary>
        public class ThermalSensor
        {
            /// <summary>
            ///     Sensor unique identifier
            /// </summary>
            public uint SensorId { get; set; }

            /// <summary>
            ///     Type of thermal sensor
            /// </summary>
            public SensorType Type { get; set; }

            /// <summary>
            ///     Location of this sensor
            /// </summary>
            public ThermalZone Zone { get; set; }

            /// <summary>
            ///     Current temperature reading in Celsius
            /// </summary>
            public int CurrentTemperatureC { get; set; }

            /// <summary>
            ///     Throttling activation temperature for this sensor in Celsius
            /// </summary>
            public int ThrottleTemperatureC { get; set; }

            /// <summary>
            ///     Shutdown temperature for this sensor in Celsius
            /// </summary>
            public int ShutdownTemperatureC { get; set; }

            /// <summary>
            ///     Maximum recorded temperature since boot in Celsius
            /// </summary>
            public int MaxRecordedTemperatureC { get; set; }

            /// <summary>
            ///     Whether this sensor is currently active/enabled
            /// </summary>
            public bool IsEnabled { get; set; }

            /// <summary>
            ///     Whether this sensor is currently causing throttling
            /// </summary>
            public bool IsThrottling { get; set; }

            /// <summary>
            ///     Is sensor reading valid (vs disconnected/faulty)
            /// </summary>
            public bool IsValid { get; set; }

            /// <summary>
            ///     Thermal headroom percentage (0-100), relative to throttle/shutdown span
            /// </summary>
            public double ThermalHeadroomPercent
            {
                get
                {
                    var thermalSpan = ShutdownTemperatureC - ThrottleTemperatureC;
                    if (thermalSpan <= 0)
                    {
                        return 0;
                    }

                    var remainingSpan = ShutdownTemperatureC - CurrentTemperatureC;
                    if (remainingSpan <= 0)
                    {
                        return 0;
                    }

                    var percent = (100.0 * remainingSpan) / thermalSpan;
                    if (percent > 100)
                    {
                        return 100;
                    }

                    return percent;
                }
            }

            public override string ToString() =>
                $"Sensor {SensorId} ({Type}, {Zone}): {CurrentTemperatureC}C " +
                $"(throttle: {ThrottleTemperatureC}C, shutdown: {ShutdownTemperatureC}C)" +
                (IsThrottling ? " [THROTTLING]" : (IsValid ? $" [OK] ({ThermalHeadroomPercent:F1}%)" : " [INVALID]"));
        }

        /// <summary>
        ///     Thermal zone status information
        /// </summary>
        public class ThermalZoneStatus
        {
            /// <summary>
            ///     Zone identifier
            /// </summary>
            public ThermalZone Zone { get; set; }

            /// <summary>
            ///     Peak temperature in this zone in Celsius
            /// </summary>
            public int PeakTemperatureC { get; set; }

            /// <summary>
            ///     Location-specific throttle threshold
            /// </summary>
            public int ThrottleThresholdC { get; set; }

            /// <summary>
            ///     Number of sensors in this zone
            /// </summary>
            public int SensorCount { get; set; }

            /// <summary>
            ///     Is any sensor in this zone throttling
            /// </summary>
            public bool IsAnyThrottling { get; set; }

            /// <summary>
            ///     Average temperature across all sensors in zone
            /// </summary>
            public double AverageTemperatureC { get; set; }

            public override string ToString() =>
                $"{Zone}: {PeakTemperatureC}C avg: {AverageTemperatureC:F1}C" +
                (IsAnyThrottling ? " [THROTTLING]" : string.Empty);
        }

        /// <summary>
        ///     Initialize for a physical GPU
        /// </summary>
        public GPUThermalSensorConfiguration(PhysicalGPU gpu)
        {
            PhysicalGPU = gpu;
            Sensors = Array.Empty<ThermalSensor>();
            Zones = Array.Empty<ThermalZoneStatus>();
        }

        /// <summary>
        ///     Reference to the physical GPU
        /// </summary>
        public PhysicalGPU PhysicalGPU { get; }

        /// <summary>
        ///     Array of all thermal sensors on this GPU
        /// </summary>
        public ThermalSensor[] Sensors { get; set; }

        /// <summary>
        ///     Thermal zone status information
        /// </summary>
        public ThermalZoneStatus[] Zones { get; set; }

        /// <summary>
        ///     Total number of thermal sensors
        /// </summary>
        public int SensorCount => Sensors.Length;

        /// <summary>
        ///     Hottest sensor reading across entire GPU
        /// </summary>
        public int HottestTemperatureC =>
            Sensors.Where(s => s.IsValid).Max(s => (int?)s.CurrentTemperatureC) ?? 0;

        /// <summary>
        ///     Average temperature across all valid sensors
        /// </summary>
        public double AverageTemperatureC
        {
            get
            {
                var validSensors = Sensors.Where(s => s.IsValid).ToArray();
                return validSensors.Length == 0 ? 0 : validSensors.Average(s => s.CurrentTemperatureC);
            }
        }

        /// <summary>
        ///     Is any thermal sensor currently throttling
        /// </summary>
        public bool IsAnyThrottling => Sensors.Any(s => s.IsThrottling);

        /// <summary>
        ///     Get sensors of specific type
        /// </summary>
        public ThermalSensor[] GetSensorsByType(SensorType type) =>
            Sensors.Where(s => s.Type == type).ToArray();

        /// <summary>
        ///     Get all sensors in a specific thermal zone
        /// </summary>
        public ThermalSensor[] GetSensorsInZone(ThermalZone zone) =>
            Sensors.Where(s => s.Zone == zone).ToArray();

        /// <summary>
        ///     Check if GPU needs thermal management intervention
        /// </summary>
        public bool NeedsThermalIntervention =>
            HottestTemperatureC > 85 ||
            IsAnyThrottling ||
            AverageTemperatureC > 75;

        /// <summary>
        ///     Get thermal health summary
        /// </summary>
        public string ThermalHealthSummary
        {
            get
            {
                if (IsAnyThrottling && HottestTemperatureC >= 95)
                {
                    return "CRITICAL: Thermal emergency - shutdown imminent";
                }

                if (IsAnyThrottling && HottestTemperatureC >= 85)
                {
                    return "CRITICAL: Thermal throttling active";
                }

                if (HottestTemperatureC >= 85)
                {
                    return "WARNING: Approaching throttle threshold";
                }

                if (HottestTemperatureC >= 75)
                {
                    return "CAUTION: High temperature zone";
                }

                return "HEALTHY: All sensors nominal";
            }
        }

        /// <summary>
        ///     Get comprehensive sensor report
        /// </summary>
        public override string ToString() =>
            $"GPU {PhysicalGPU.GPUId} Thermal Report:\n" +
            $"  Peak: {HottestTemperatureC}C, Avg: {AverageTemperatureC:F1}C\n" +
            $"  Sensors: {SensorCount} active, {Sensors.Count(s => s.IsThrottling)} throttling\n" +
            $"  Health: {ThermalHealthSummary}";
    }
}
