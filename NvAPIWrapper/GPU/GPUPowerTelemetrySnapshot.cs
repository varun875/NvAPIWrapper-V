#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using NvAPIWrapper.Native.GPU;
using NvAPIWrapper.Native.Power;

namespace NvAPIWrapper.GPU
{
    /// <summary>
    ///     Captures a single point-in-time power telemetry snapshot for a physical GPU.
    ///     Prefers direct driver watt readings and preserves the source and availability of every watt value.
    ///     Queries are sequential, not an atomic hardware sample. NVML usage may be averaged by the driver.
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
            IReadOnlyList<GPUPowerLimitInfo> powerLimitInformation,
            NvmlPowerData driverPower,
            GPUPowerSpecDatabase.GPUPowerSpec? powerSpec,
            GPUPowerTelemetryOptions options)
        {
            PhysicalGPU = physicalGPU;
            CapturedAtUtc = capturedAtUtc;
            CurrentPerformanceState = currentPerformanceState;
            CurrentPerformanceLimit = currentPerformanceLimit;
            PerformanceDecreaseReason = performanceDecreaseReason;
            PowerTopologyEntries = Array.AsReadOnly(powerTopologyEntries.ToArray());
            PowerLimitPolicies = Array.AsReadOnly(powerLimitPolicies.ToArray());
            PowerLimitInformation = Array.AsReadOnly(powerLimitInformation.ToArray());
            PowerDiagnostics = Array.AsReadOnly(driverPower.Diagnostics.ToArray());
            NVMLPCIAddress = driverPower.PciAddress;
            _powerSpec = powerSpec;

            BoardPowerReference = DriverValue(driverPower.DefaultLimitWatts);
            if (!BoardPowerReference.IsAvailable && options.BoardPowerReferenceInWatts.HasValue)
            {
                BoardPowerReference = new GPUPowerValue(options.BoardPowerReferenceInWatts, GPUPowerDataSource.UserProvided, true);
            }
            if (!BoardPowerReference.IsAvailable && powerSpec != null &&
                (powerSpec.IsUserProvided || options.AllowReferenceDatabaseEstimates))
            {
                BoardPowerReference = new GPUPowerValue(powerSpec.DefaultTDPWatts,
                    powerSpec.IsUserProvided ? GPUPowerDataSource.UserProvided : GPUPowerDataSource.ReferenceDatabase, true);
            }

            BoardPower = DriverValue(driverPower.UsageWatts);
            if (!BoardPower.IsAvailable)
            {
                BoardPower = GPUPowerMath.Estimate(BoardPowerUsageInPercent, BoardPowerReference);
            }

            // NVML usage includes associated circuitry (such as memory). It is not
            // a core-only reading, and a board TGP is not a GPU-domain reference.
            GPUPower = options.GPUPowerReferenceInWatts.HasValue
                ? GPUPowerMath.Estimate(GPUPowerUsageInPercent,
                    new GPUPowerValue(options.GPUPowerReferenceInWatts, GPUPowerDataSource.UserProvided, true))
                : GPUPowerValue.Unavailable;

            DefaultPowerLimit = DriverValue(driverPower.DefaultLimitWatts);
            if (!DefaultPowerLimit.IsAvailable) DefaultPowerLimit = BoardPowerReference;
            CurrentPowerLimit = PreferDriverLimit(driverPower.CurrentLimitWatts, ActivePowerTargetInPercent);
            EnforcedPowerLimit = DriverValue(driverPower.EnforcedLimitWatts);
            MinimumPowerLimit = PreferDriverLimit(driverPower.MinimumLimitWatts, MinimumPowerTargetInPercent);
            MaximumPowerLimit = PreferDriverLimit(driverPower.MaximumLimitWatts, MaximumPowerTargetInPercent);

            // Legacy database ranges were not sourced from the actual board's
            // firmware. Only explicit custom ranges may supplement driver limits.
            if (powerSpec?.IsUserProvided == true)
            {
                if (!MinimumPowerLimit.IsAvailable)
                    MinimumPowerLimit = new GPUPowerValue(powerSpec.MinTDPWatts, GPUPowerDataSource.UserProvided, true);
                if (!MaximumPowerLimit.IsAvailable)
                    MaximumPowerLimit = new GPUPowerValue(powerSpec.MaxTDPWatts, GPUPowerDataSource.UserProvided, true);
            }
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
        // Percentage-based properties (available only when the corresponding query succeeds)
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
        // Watt values with provenance, and compatibility projections of those values
        // =====================================================

        /// <summary>
        ///     Gets whether a reference for 100% board power is available.
        ///     This does not imply that power usage or current limits are available.
        /// </summary>
        public bool IsTDPKnown => BoardPowerReference.IsAvailable;

        /// <summary>Gets board/GPU-and-associated-circuitry power, with its source and estimation status.</summary>
        public GPUPowerValue BoardPower { get; }

        /// <summary>Gets GPU-domain power, available only with an explicit domain-specific reference.</summary>
        public GPUPowerValue GPUPower { get; }

        /// <summary>Gets the 100% board reference used by estimates. This is independent of the active slider.</summary>
        public GPUPowerValue BoardPowerReference { get; }

        /// <summary>Gets the default limit and its provenance.</summary>
        public GPUPowerValue DefaultPowerLimit { get; }

        /// <summary>Gets the configured limit. Missing data never falls back to an assumed default limit.</summary>
        public GPUPowerValue CurrentPowerLimit { get; }

        /// <summary>Gets the driver-enforced limit, which can differ from the configured limit.</summary>
        public GPUPowerValue EnforcedPowerLimit { get; }

        /// <summary>Gets the minimum allowed limit when reported or explicitly configured.</summary>
        public GPUPowerValue MinimumPowerLimit { get; }

        /// <summary>Gets the maximum allowed limit when reported or explicitly configured.</summary>
        public GPUPowerValue MaximumPowerLimit { get; }

        /// <summary>Gets diagnostics for unavailable or failed optional power queries.</summary>
        public IReadOnlyList<string> PowerDiagnostics { get; }

        /// <summary>Gets the matched NVML PCI address, when device association succeeded.</summary>
        public string? NVMLPCIAddress { get; }

        /// <summary>
        ///     Gets usage relative to the enforced limit, or configured limit if enforcement data is unavailable.
        ///     This is distinct from the raw NVAPI percentage, which is referenced to the domain's baseline.
        /// </summary>
        public double? PowerUsageRelativeToLimitInPercent
        {
            get
            {
                var limit = EnforcedPowerLimit.Watts ?? CurrentPowerLimit.Watts;
                return BoardPower.Watts.HasValue && limit > 0
                    ? BoardPower.Watts.Value / limit.Value * 100d
                    : null;
            }
        }

        /// <summary>
        ///     Gets the selected board reference in watts. Inspect BoardPowerReference for provenance.
        /// </summary>
        public double? DefaultTDPInWatts => BoardPowerReference.Watts;

        /// <summary>
        ///     Gets the maximum available power limit. Unverified built-in slider ranges are not returned.
        /// </summary>
        public double? MaxTDPInWatts => MaximumPowerLimit.Watts;

        /// <summary>
        ///     Gets the minimum available power limit.
        /// </summary>
        public double? MinTDPInWatts => MinimumPowerLimit.Watts;

        /// <summary>
        ///     Gets the matched GPU architecture name (e.g. "Ada Lovelace", "Blackwell").
        /// </summary>
        public string? MatchedArchitecture => _powerSpec?.Architecture;

        /// <summary>
        ///     Gets board power in watts. Direct NVML data takes precedence over estimates.
        ///     Inspect BoardPower.IsEstimated and BoardPower.Source before presenting this reading.
        /// </summary>
        /// <remarks>
        ///     Estimates use the reference for 100% board usage, not the active power slider.
        ///     NVML reports the GPU and associated circuitry; it does not measure whole-system power.
        ///     Its sampling period is device-dependent and may be a one-second average.
        /// </remarks>
        public double? BoardPowerDrawInWatts
        {
            get => BoardPower.Watts;
        }

        /// <summary>
        ///     Gets estimated current GPU-domain power draw in watts.
        ///     Requires both GPU-domain telemetry and an explicit GPU-domain reference in the options.
        /// </summary>
        public double? GPUPowerDrawInWatts
        {
            get => GPUPower.Watts;
        }

        /// <summary>
        ///     Gets the configured power limit in watts, or null. See EnforcedPowerLimit for the effective ceiling.
        /// </summary>
        public double? CurrentPowerLimitInWatts
        {
            get => CurrentPowerLimit.Watts;
        }

        /// <summary>
        ///     Gets the default power limit in watts if the power limit percentage and TDP are known.
        /// </summary>
        public double? DefaultPowerLimitInWatts
        {
            get => DefaultPowerLimit.Watts;
        }

        /// <summary>
        ///     Gets the minimum power limit in watts if TDP is known.
        /// </summary>
        public double? MinimumPowerLimitInWatts
        {
            get => MinimumPowerLimit.Watts;
        }

        /// <summary>
        ///     Gets the maximum power limit in watts if TDP is known.
        /// </summary>
        public double? MaximumPowerLimitInWatts
        {
            get => MaximumPowerLimit.Watts;
        }

        // =====================================================
        // Throttle/limit detection
        // =====================================================

        /// <summary>Gets whether performance-limit flags were actually retrieved.</summary>
        public bool IsThrottleStatusAvailable => CurrentPerformanceLimit.HasValue;

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
                if (!CurrentPerformanceLimit.HasValue)
                {
                    return "Unavailable";
                }

                if (CurrentPerformanceLimit.Value == PerformanceLimit.None)
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

        private static GPUPowerValue DriverValue(double? watts)
        {
            return watts.HasValue ? new GPUPowerValue(watts, GPUPowerDataSource.NVML, false) : GPUPowerValue.Unavailable;
        }

        private GPUPowerValue PreferDriverLimit(double? driverWatts, float? percentage)
        {
            return driverWatts.HasValue ? DriverValue(driverWatts) :
                percentage > 0 ? GPUPowerMath.Estimate(percentage, BoardPowerReference) : GPUPowerValue.Unavailable;
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
            return forAllStates;
        }

        private static bool TryEstimatePowerInWatts(
            float? usageInPercent,
            double powerLimitInWatts,
            out double estimatedPowerInWatts)
        {
            return GPUPowerMath.TryEstimate(usageInPercent, powerLimitInWatts, out estimatedPowerInWatts);
        }
    }
}
