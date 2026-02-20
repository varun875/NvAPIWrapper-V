using NvAPIWrapper.Native.GPU;
using NvAPIWrapper.Native.GPU.Structures;

namespace NvAPIWrapper.GPU
{
    /// <summary>
    ///     Holds information regarding a currently active power limit policy.
    ///     The power target is expressed as a percentage of the GPU's default TDP.
    /// </summary>
    public class GPUPowerLimitPolicy
    {
        internal GPUPowerLimitPolicy(PrivatePowerPoliciesStatusV1.PowerPolicyStatusEntry powerPolicyStatusEntry)
        {
            PerformanceStateId = powerPolicyStatusEntry.PerformanceStateId;
            PowerTargetInPCM = powerPolicyStatusEntry.PowerTargetInPCM;
        }

        /// <summary>
        ///     Gets the corresponding performance state identification
        /// </summary>
        public PerformanceStateId PerformanceStateId { get; }

        /// <summary>
        ///     Gets the current policy target power in per cent mille (PCM).
        ///     100000 PCM = 100% = GPU is at default TDP.
        ///     110000 PCM = 110% = user raised power limit by 10%.
        /// </summary>
        public uint PowerTargetInPCM { get; }

        /// <summary>
        ///     Gets the current policy target power in percentage (e.g. 100%, 110%).
        /// </summary>
        public float PowerTargetInPercent
        {
            get => PowerTargetInPCM / 1000f;
        }

        /// <summary>
        ///     Converts the power target percentage to watts using a known TDP reference.
        /// </summary>
        /// <param name="defaultTDPInWatts">The GPU's default TDP in watts.</param>
        /// <returns>The current power target in watts.</returns>
        public double ToWatts(double defaultTDPInWatts)
        {
            return defaultTDPInWatts * PowerTargetInPercent / 100d;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{PerformanceStateId} Target: {PowerTargetInPercent:F1}%";
        }
    }
}