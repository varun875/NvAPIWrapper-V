using NvAPIWrapper.Native.GPU;
using NvAPIWrapper.Native.GPU.Structures;

namespace NvAPIWrapper.GPU
{
    /// <summary>
    ///     Holds information regarding a possible power limit policy and its acceptable range.
    ///     All values are in PCM (per cent mille, where 100000 = 100%) relative to the default TDP.
    /// </summary>
    public class GPUPowerLimitInfo
    {
        internal GPUPowerLimitInfo(PrivatePowerPoliciesInfoV1.PowerPolicyInfoEntry powerPolicyInfoEntry)
        {
            PerformanceStateId = powerPolicyInfoEntry.PerformanceStateId;
            MinimumPowerInPCM = powerPolicyInfoEntry.MinimumPowerInPCM;
            DefaultPowerInPCM = powerPolicyInfoEntry.DefaultPowerInPCM;
            MaximumPowerInPCM = powerPolicyInfoEntry.MaximumPowerInPCM;
        }

        /// <summary>
        ///     Gets the default policy target power in per cent mille (PCM).
        ///     A value of 100000 means the GPU is at 100% of its default TDP.
        /// </summary>
        public uint DefaultPowerInPCM { get; }

        /// <summary>
        ///     Gets the default policy target power in percentage (typically 100%).
        /// </summary>
        public float DefaultPowerInPercent
        {
            get => DefaultPowerInPCM / 1000f;
        }

        /// <summary>
        ///     Gets the maximum possible policy target power in per cent mille (PCM).
        ///     For example, 116000 PCM = 116% = the maximum the power slider can be set to.
        /// </summary>
        public uint MaximumPowerInPCM { get; }

        /// <summary>
        ///     Gets the maximum possible policy target power in percentage (e.g. 116%).
        /// </summary>
        public float MaximumPowerInPercent
        {
            get => MaximumPowerInPCM / 1000f;
        }

        /// <summary>
        ///     Gets the minimum possible policy target power in per cent mille (PCM).
        /// </summary>
        public uint MinimumPowerInPCM { get; }

        /// <summary>
        ///     Gets the minimum possible policy target power in percentage (e.g. 70%).
        /// </summary>
        public float MinimumPowerInPercent
        {
            get => MinimumPowerInPCM / 1000f;
        }

        /// <summary>
        ///     Gets the corresponding performance state identification
        /// </summary>
        public PerformanceStateId PerformanceStateId { get; }

        /// <summary>
        ///     Converts power limit percentages to watts using a known TDP reference.
        /// </summary>
        /// <param name="defaultTDPInWatts">The default TDP of the GPU in watts.</param>
        /// <returns>An object describing min/default/max power limits in watts.</returns>
        public PowerLimitInWatts ToWatts(double defaultTDPInWatts)
        {
            return new PowerLimitInWatts(
                defaultTDPInWatts * MinimumPowerInPercent / 100d,
                defaultTDPInWatts * DefaultPowerInPercent / 100d,
                defaultTDPInWatts * MaximumPowerInPercent / 100d
            );
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"[{PerformanceStateId}] Default: {DefaultPowerInPercent:F1}% - Range: ({MinimumPowerInPercent:F1}% - {MaximumPowerInPercent:F1}%)";
        }
    }

    /// <summary>
    ///     Represents power limit values converted to watts.
    /// </summary>
    public class PowerLimitInWatts
    {
        internal PowerLimitInWatts(double minimum, double defaultValue, double maximum)
        {
            MinimumWatts = minimum;
            DefaultWatts = defaultValue;
            MaximumWatts = maximum;
        }

        /// <summary>
        ///     Gets the minimum power limit in watts.
        /// </summary>
        public double MinimumWatts { get; }

        /// <summary>
        ///     Gets the default power limit in watts.
        /// </summary>
        public double DefaultWatts { get; }

        /// <summary>
        ///     Gets the maximum power limit in watts.
        /// </summary>
        public double MaximumWatts { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Default: {DefaultWatts:F1}W - Range: ({MinimumWatts:F1}W - {MaximumWatts:F1}W)";
        }
    }
}