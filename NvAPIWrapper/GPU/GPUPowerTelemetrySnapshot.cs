#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using NvAPIWrapper.Native.GPU;

namespace NvAPIWrapper.GPU
{
    /// <summary>
    ///     Captures a single point-in-time power telemetry snapshot for a physical GPU.
    ///     Provides both percentage-based and automatic watt-based power readings.
    /// </summary>
    public sealed class GPUPowerTelemetrySnapshot
    {
        internal GPUPowerTelemetrySnapshot(
            PhysicalGPU physicalGPU,
            DateTimeOffset capturedAtUtc,
            PerformanceStateId? currentPerformanceState,
            PerformanceLimit? currentPerformanceLimit,
            PerformanceDecreaseReason? performanceDecreaseReason,
            IReadOnlyList<GPUPowerTopologyStatus> powerTopologyEntries,
            IReadOnlyList<GPUPowerLimitPolicy> powerLimitPolicies,
            IReadOnlyList<GPUPowerLimitInfo> powerLimitInformation)
        {
            PhysicalGPU = physicalGPU;
            CapturedAtUtc = capturedAtUtc;
            CurrentPerformanceState = currentPerformanceState;
            CurrentPerformanceLimit = currentPerformanceLimit;
            PerformanceDecreaseReason = performanceDecreaseReason;
            PowerTopologyEntries = powerTopologyEntries;
            PowerLimitPolicies = powerLimitPolicies;
            PowerLimitInformation = powerLimitInformation;

            // Try to auto-resolve the GPU power spec
            GPUPowerSpecDatabase.TryGetSpec(physicalGPU.FullName, out _powerSpec);
        }

        private readonly GPUPowerSpecDatabase.GPUPowerSpec? _powerSpec;

        /// <summary>
        ///     Gets the physical GPU that this snapshot describes.
        /// </summary>
        public PhysicalGPU PhysicalGPU { get; }

        /// <summary>
        ///     Gets the snapshot capture time in UTC.
        /// </summary>
        public DateTimeOffset CapturedAtUtc { get; }

        /// <summary>
        ///     Gets current P-State when available.
        /// </summary>
        public PerformanceStateId? CurrentPerformanceState { get; }

        /// <summary>
        ///     Gets active performance limit flags when available.
        /// </summary>
        public PerformanceLimit? CurrentPerformanceLimit { get; }

        /// <summary>
        ///     Gets current performance decrease reason when available.
        /// </summary>
        public PerformanceDecreaseReason? PerformanceDecreaseReason { get; }

        /// <summary>
        ///     Gets raw power topology entries (typically GPU and Board domains).
        /// </summary>
        public IReadOnlyList<GPUPowerTopologyStatus> PowerTopologyEntries { get; }

        /// <summary>
        ///     Gets active power limit policy entries.
        /// </summary>
        public IReadOnlyList<GPUPowerLimitPolicy> PowerLimitPolicies { get; }

        /// <summary>
        ///     Gets power limit capability entries.
        /// </summary>
        public IReadOnlyList<GPUPowerLimitInfo> PowerLimitInformation { get; }

        // =====================================================
        // Percentage-based properties (always available)
        // =====================================================

        /// <summary>
        ///     Gets board power usage percentage when available.
        /// </summary>
        public float? BoardPowerUsageInPercent
        {
            get => FindDomainUsage(PowerTopologyDomain.Board);
        }

        /// <summary>
        ///     Gets GPU core power usage percentage when available.
        /// </summary>
        public float? GPUPowerUsageInPercent
        {
            get => FindDomainUsage(PowerTopologyDomain.GPU);
        }

        /// <summary>
        ///     Gets active power target percentage for current state (or all states fallback) when available.
        /// </summary>
        public float? ActivePowerTargetInPercent
        {
            get => FindActivePowerTarget();
        }

        /// <summary>
        ///     Gets default power target percentage for current state (or all states fallback) when available.
        /// </summary>
        public float? DefaultPowerTargetInPercent
        {
            get => FindDefaultPowerTarget(info => info.DefaultPowerInPercent);
        }

        /// <summary>
        ///     Gets minimum power target percentage for current state (or all states fallback) when available.
        /// </summary>
        public float? MinimumPowerTargetInPercent
        {
            get => FindDefaultPowerTarget(info => info.MinimumPowerInPercent);
        }

        /// <summary>
        ///     Gets maximum power target percentage for current state (or all states fallback) when available.
        /// </summary>
        public float? MaximumPowerTargetInPercent
        {
            get => FindDefaultPowerTarget(info => info.MaximumPowerInPercent);
        }

        // =====================================================
        // Watt-based properties (auto-resolved from GPU database)
        // =====================================================

        /// <summary>
        ///     Gets whether the GPU's TDP is known in the built-in database.
        ///     When true, all watt-based properties will return values.
        /// </summary>
        public bool IsTDPKnown => _powerSpec != null;

        /// <summary>
        ///     Gets the GPU's default TDP/TGP in watts from the built-in database.
        ///     Returns null if the GPU is not in the database.
        /// </summary>
        public double? DefaultTDPInWatts => _powerSpec?.DefaultTDPWatts;

        /// <summary>
        ///     Gets the GPU's maximum power limit in watts from the built-in database.
        ///     This is the maximum the power limit slider can be set to (e.g. in MSI Afterburner).
        /// </summary>
        public double? MaxTDPInWatts => _powerSpec?.MaxTDPWatts;

        /// <summary>
        ///     Gets the GPU's minimum power limit in watts from the built-in database.
        /// </summary>
        public double? MinTDPInWatts => _powerSpec?.MinTDPWatts;

        /// <summary>
        ///     Gets the matched GPU architecture name (e.g. "Ada Lovelace", "Blackwell").
        /// </summary>
        public string? MatchedArchitecture => _powerSpec?.Architecture;

        /// <summary>
        ///     Gets estimated current board power draw in watts.
        ///     Uses the board-domain power percentage and the known TDP.
        ///     Returns null if TDP is unknown or board telemetry is unavailable.
        /// </summary>
        /// <remarks>
        ///     Formula: BoardPowerUsageInPercent / 100 * CurrentEffectivePowerLimitInWatts
        ///     The "current effective power limit" accounts for any user-set power limit slider position.
        /// </remarks>
        public double? BoardPowerDrawInWatts
        {
            get => ComputePowerInWatts(BoardPowerUsageInPercent);
        }

        /// <summary>
        ///     Gets estimated current GPU-domain power draw in watts.
        ///     Returns null if TDP is unknown or GPU-domain telemetry is unavailable.
        /// </summary>
        public double? GPUPowerDrawInWatts
        {
            get => ComputePowerInWatts(GPUPowerUsageInPercent);
        }

        /// <summary>
        ///     Gets the current effective power limit in watts (accounts for user-set power limit slider).
        ///     This is the actual watt ceiling the GPU is operating under right now.
        ///     Returns null if TDP is unknown.
        /// </summary>
        public double? CurrentPowerLimitInWatts
        {
            get
            {
                if (_powerSpec == null)
                {
                    return null;
                }

                var activePct = ActivePowerTargetInPercent;

                if (activePct.HasValue)
                {
                    return _powerSpec.DefaultTDPWatts * activePct.Value / 100d;
                }

                // Fallback: assume default TDP
                return _powerSpec.DefaultTDPWatts;
            }
        }

        /// <summary>
        ///     Gets the default power limit in watts if the power limit percentage and TDP are known.
        /// </summary>
        public double? DefaultPowerLimitInWatts
        {
            get
            {
                if (_powerSpec == null)
                {
                    return null;
                }

                var defaultPct = DefaultPowerTargetInPercent;

                if (defaultPct.HasValue)
                {
                    return _powerSpec.DefaultTDPWatts * defaultPct.Value / 100d;
                }

                return _powerSpec.DefaultTDPWatts;
            }
        }

        /// <summary>
        ///     Gets the minimum power limit in watts if TDP is known.
        /// </summary>
        public double? MinimumPowerLimitInWatts
        {
            get
            {
                if (_powerSpec == null)
                {
                    return null;
                }

                var minPct = MinimumPowerTargetInPercent;

                if (minPct.HasValue)
                {
                    return _powerSpec.DefaultTDPWatts * minPct.Value / 100d;
                }

                return _powerSpec.MinTDPWatts;
            }
        }

        /// <summary>
        ///     Gets the maximum power limit in watts if TDP is known.
        /// </summary>
        public double? MaximumPowerLimitInWatts
        {
            get
            {
                if (_powerSpec == null)
                {
                    return null;
                }

                var maxPct = MaximumPowerTargetInPercent;

                if (maxPct.HasValue)
                {
                    return _powerSpec.DefaultTDPWatts * maxPct.Value / 100d;
                }

                return _powerSpec.MaxTDPWatts;
            }
        }

        // =====================================================
        // Throttle/limit detection
        // =====================================================

        /// <summary>
        ///     Gets whether a power limit condition is currently active.
        /// </summary>
        public bool IsPowerLimitActive
        {
            get => CurrentPerformanceLimit.HasValue &&
                   (CurrentPerformanceLimit.Value & PerformanceLimit.PowerLimit) == PerformanceLimit.PowerLimit;
        }

        /// <summary>
        ///     Gets whether a thermal limit condition is currently active.
        /// </summary>
        public bool IsThermalLimitActive
        {
            get => CurrentPerformanceLimit.HasValue &&
                   (CurrentPerformanceLimit.Value & PerformanceLimit.TemperatureLimit) ==
                   PerformanceLimit.TemperatureLimit;
        }

        /// <summary>
        ///     Gets whether a voltage limit condition is currently active.
        /// </summary>
        public bool IsVoltageLimitActive
        {
            get => CurrentPerformanceLimit.HasValue &&
                   (CurrentPerformanceLimit.Value & PerformanceLimit.VoltageLimit) ==
                   PerformanceLimit.VoltageLimit;
        }

        /// <summary>
        ///     Gets whether performance is reduced due to no load (GPU is idle/underutilized).
        /// </summary>
        public bool IsNoLoadLimitActive
        {
            get => CurrentPerformanceLimit.HasValue &&
                   (CurrentPerformanceLimit.Value & PerformanceLimit.NoLoadLimit) ==
                   PerformanceLimit.NoLoadLimit;
        }

        /// <summary>
        ///     Gets a human-readable summary of the current throttle state.
        /// </summary>
        public string ThrottleStatus
        {
            get
            {
                if (!CurrentPerformanceLimit.HasValue || CurrentPerformanceLimit.Value == PerformanceLimit.None)
                {
                    return "No Throttling";
                }

                var reasons = new List<string>();

                if (IsPowerLimitActive) reasons.Add("Power");
                if (IsThermalLimitActive) reasons.Add("Thermal");
                if (IsVoltageLimitActive) reasons.Add("Voltage");
                if (IsNoLoadLimitActive) reasons.Add("No Load");

                return reasons.Count > 0
                    ? $"Throttled: {string.Join(" + ", reasons)}"
                    : $"Throttled: {CurrentPerformanceLimit.Value}";
            }
        }

        // =====================================================
        // Manual estimation methods (for users who know their TDP)
        // =====================================================

        /// <summary>
        ///     Tries to estimate board power usage in watts from board-domain percentage telemetry.
        /// </summary>
        /// <param name="boardPowerLimitInWatts">Reference board power limit in watts (TGP/TDP).</param>
        /// <param name="estimatedPowerInWatts">Estimated board power usage in watts.</param>
        /// <returns>True when estimation is available; otherwise false.</returns>
        public bool TryEstimateBoardPowerInWatts(double boardPowerLimitInWatts, out double estimatedPowerInWatts)
        {
            return TryEstimatePowerInWatts(BoardPowerUsageInPercent, boardPowerLimitInWatts, out estimatedPowerInWatts);
        }

        /// <summary>
        ///     Tries to estimate GPU-domain power usage in watts from GPU-domain percentage telemetry.
        /// </summary>
        /// <param name="gpuPowerLimitInWatts">Reference GPU-domain power limit in watts.</param>
        /// <param name="estimatedPowerInWatts">Estimated GPU-domain power usage in watts.</param>
        /// <returns>True when estimation is available; otherwise false.</returns>
        public bool TryEstimateGPUPowerInWatts(double gpuPowerLimitInWatts, out double estimatedPowerInWatts)
        {
            return TryEstimatePowerInWatts(GPUPowerUsageInPercent, gpuPowerLimitInWatts, out estimatedPowerInWatts);
        }

        // =====================================================
        // Private helpers
        // =====================================================

        private double? ComputePowerInWatts(float? usagePercent)
        {
            if (_powerSpec == null || !usagePercent.HasValue)
            {
                return null;
            }

            // The effective power limit takes into account the user's power limit slider setting.
            // If ActivePowerTarget is 110%, the GPU can use up to 110% of the default TDP.
            // The topology percentage is relative to this effective limit.
            var effectivePowerLimitWatts = CurrentPowerLimitInWatts ?? _powerSpec.DefaultTDPWatts;

            return effectivePowerLimitWatts * usagePercent.Value / 100d;
        }

        private float? FindDomainUsage(PowerTopologyDomain domain)
        {
            var entry = PowerTopologyEntries.FirstOrDefault(status => status.Domain == domain);
            return entry?.PowerUsageInPercent;
        }

        private float? FindActivePowerTarget()
        {
            var entry = SelectPolicyEntry(PowerLimitPolicies, policy => policy.PerformanceStateId);
            return entry?.PowerTargetInPercent;
        }

        private float? FindDefaultPowerTarget(Func<GPUPowerLimitInfo, float> selector)
        {
            var entry = SelectPolicyEntry(PowerLimitInformation, info => info.PerformanceStateId);
            return entry == null ? (float?) null : selector(entry);
        }

        private TEntry? SelectPolicyEntry<TEntry>(
            IReadOnlyList<TEntry> entries,
            Func<TEntry, PerformanceStateId> stateSelector)
            where TEntry : class
        {
            if (entries.Count == 0)
            {
                return null;
            }

            if (CurrentPerformanceState.HasValue)
            {
                var currentState = CurrentPerformanceState.Value;
                var forCurrentState = entries.FirstOrDefault(entry => stateSelector(entry) == currentState);

                if (forCurrentState != null)
                {
                    return forCurrentState;
                }
            }

            var forAllStates = entries.FirstOrDefault(entry => stateSelector(entry) == PerformanceStateId.All);
            return forAllStates ?? entries[0];
        }

        private static bool TryEstimatePowerInWatts(
            float? usageInPercent,
            double powerLimitInWatts,
            out double estimatedPowerInWatts)
        {
            if (powerLimitInWatts <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(powerLimitInWatts));
            }

            estimatedPowerInWatts = 0;

            if (!usageInPercent.HasValue)
            {
                return false;
            }

            estimatedPowerInWatts = powerLimitInWatts * usageInPercent.Value / 100d;
            return true;
        }
    }
}
