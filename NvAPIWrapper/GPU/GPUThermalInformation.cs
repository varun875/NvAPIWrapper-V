using System;
using System.Collections.Generic;
using System.Linq;
using NvAPIWrapper.Native;
using NvAPIWrapper.Native.Exceptions;
using NvAPIWrapper.Native.General;

namespace NvAPIWrapper.GPU
{
    /// <summary>
    ///     Holds information regarding the available thermal sensors and current thermal level of a GPU
    /// </summary>
    public class GPUThermalInformation
    {
        internal GPUThermalInformation(PhysicalGPU physicalGPU)
        {
            PhysicalGPU = physicalGPU;
        }

        /// <summary>
        ///     Gets the current thermal level of the GPU
        /// </summary>
        public int CurrentThermalLevel
        {
            get => (int) GPUApi.GetCurrentThermalLevel(PhysicalGPU.Handle);
        }

        /// <summary>
        ///     Gets the physical GPU that this instance describes
        /// </summary>
        public PhysicalGPU PhysicalGPU { get; }

        /// <summary>
        ///     Gets the list of available thermal sensors
        /// </summary>
        public IEnumerable<GPUThermalSensor> ThermalSensors
        {
            get
            {
                return GPUApi.GetThermalSettings(PhysicalGPU.Handle).Sensors
                    .Select((sensor, i) => new GPUThermalSensor(i, sensor));
            }
        }

        /// <summary>
        ///     Tries to get current thermal level without throwing for unsupported GPUs/drivers.
        /// </summary>
        /// <param name="currentThermalLevel">Current thermal level when available.</param>
        /// <returns>True when telemetry is available; otherwise false.</returns>
        public bool TryGetCurrentThermalLevel(out int currentThermalLevel)
        {
            try
            {
                currentThermalLevel = CurrentThermalLevel;
                return true;
            }
            catch (NVIDIANotSupportedException)
            {
                currentThermalLevel = 0;
                return false;
            }
            catch (NVIDIAApiException ex) when (IsCapabilityUnavailableStatus(ex.Status))
            {
                currentThermalLevel = 0;
                return false;
            }
        }

        /// <summary>
        ///     Tries to get thermal sensors without throwing for unsupported GPUs/drivers.
        /// </summary>
        /// <param name="thermalSensors">Thermal sensors when available.</param>
        /// <returns>True when telemetry is available; otherwise false.</returns>
        public bool TryGetThermalSensors(out GPUThermalSensor[] thermalSensors)
        {
            try
            {
                thermalSensors = ThermalSensors.ToArray();
                return true;
            }
            catch (NVIDIANotSupportedException)
            {
                thermalSensors = Array.Empty<GPUThermalSensor>();
                return false;
            }
            catch (NVIDIAApiException ex) when (IsCapabilityUnavailableStatus(ex.Status))
            {
                thermalSensors = Array.Empty<GPUThermalSensor>();
                return false;
            }
        }

        private static bool IsCapabilityUnavailableStatus(Status status)
        {
            return status == Status.NotSupported ||
                   status == Status.NoImplementation ||
                   status == Status.FunctionNotFound;
        }
    }
}

