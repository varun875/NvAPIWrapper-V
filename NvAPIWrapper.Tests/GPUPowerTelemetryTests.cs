using System;
using System.Collections.Generic;
using NvAPIWrapper.GPU;
using NvAPIWrapper.Native.GPU;
using NvAPIWrapper.Native.GPU.Structures;
using NvAPIWrapper.Native.Power;
using Xunit;

namespace NvAPIWrapper.Tests
{
    public class GPUPowerTelemetryTests
    {
        internal static GPUPowerTelemetrySnapshot Snapshot(NvmlPowerData? driver = null,
            GPUPowerTelemetryOptions? options = null, float? boardPercent = 50, float? gpuPercent = 25,
            float? targetPercent = null, PerformanceLimit? limit = null,
            GPUPowerSpecDatabase.GPUPowerSpec? spec = null,
            IReadOnlyList<GPUPowerLimitPolicy>? policies = null,
            IReadOnlyList<GPUPowerLimitInfo>? limits = null)
        {
            var topology = new List<GPUPowerTopologyStatus>();
            if (boardPercent.HasValue) topology.Add(Topology(PowerTopologyDomain.Board, boardPercent.Value));
            if (gpuPercent.HasValue) topology.Add(Topology(PowerTopologyDomain.GPU, gpuPercent.Value));
            return new GPUPowerTelemetrySnapshot(new PhysicalGPU(default), DateTimeOffset.UtcNow,
                PerformanceStateId.P2_Balanced, limit, null, topology,
                policies ?? (targetPercent.HasValue ? new[] { Policy(PerformanceStateId.All, targetPercent.Value) } : Array.Empty<GPUPowerLimitPolicy>()),
                limits ?? Array.Empty<GPUPowerLimitInfo>(), driver ?? new NvmlPowerData(), spec,
                options ?? new GPUPowerTelemetryOptions());
        }

        private static GPUPowerTopologyStatus Topology(PowerTopologyDomain domain, float percent) =>
            new GPUPowerTopologyStatus(new PrivatePowerTopologiesStatusV1.PowerTopologiesStatusEntry
            {
                _Domain = domain, _PowerUsageInPCM = (uint)Math.Round(percent * 1000d)
            });

        private static GPUPowerLimitPolicy Policy(PerformanceStateId state, float percent) =>
            new GPUPowerLimitPolicy(new PrivatePowerPoliciesStatusV1.PowerPolicyStatusEntry
            {
                _PerformanceStateId = state, _PowerTargetInPCM = (uint)(percent * 1000)
            });

        private static GPUPowerSpecDatabase.GPUPowerSpec ReferenceSpec(bool custom = false) =>
            new GPUPowerSpecDatabase.GPUPowerSpec("Test board", 400, 500, 200, "Test", custom);

        [Fact]
        public void DirectReadingsWinOverPercentagesAndEveryReferenceSource()
        {
            var snapshot = Snapshot(new NvmlPowerData { UsageWatts = 7.25, DefaultLimitWatts = 55 },
                new GPUPowerTelemetryOptions(115, allowReferenceDatabaseEstimates: true), spec: ReferenceSpec());
            Assert.Equal(7.25, snapshot.BoardPowerDrawInWatts);
            Assert.Equal(55d, snapshot.DefaultTDPInWatts);
            Assert.Equal(GPUPowerDataSource.NVML, snapshot.BoardPower.Source);
            Assert.False(snapshot.BoardPower.IsEstimated);
            Assert.Null(snapshot.GPUPowerDrawInWatts);
        }

        [Fact]
        public void DirectWattsDoNotRequireNameDatabasePercentagesOrDefaultLimit()
        {
            var snapshot = Snapshot(new NvmlPowerData { UsageWatts = 12.345 }, boardPercent: null, gpuPercent: null);
            Assert.Equal(12.345, snapshot.BoardPowerDrawInWatts);
            Assert.False(snapshot.IsTDPKnown);
            Assert.Null(snapshot.CurrentPowerLimitInWatts);
        }

        [Fact]
        public void ZeroPowerIsAValidDriverReading()
        {
            var snapshot = Snapshot(new NvmlPowerData { UsageWatts = 0 });
            Assert.True(snapshot.BoardPower.IsAvailable);
            Assert.Equal(0d, snapshot.BoardPowerDrawInWatts);
            Assert.False(snapshot.BoardPower.IsEstimated);
        }

        [Theory]
        [InlineData(80, 320)]
        [InlineData(100, 400)]
        [InlineData(110, 440)]
        public void UsageIsNotRescaledByTheActivePowerSlider(float target, double configuredLimit)
        {
            var snapshot = Snapshot(new NvmlPowerData { DefaultLimitWatts = 400 }, targetPercent: target);
            Assert.Equal(200d, snapshot.BoardPowerDrawInWatts);
            Assert.Equal(configuredLimit, snapshot.CurrentPowerLimitInWatts!.Value, 8);
            Assert.True(snapshot.BoardPower.IsEstimated);
            Assert.True(snapshot.CurrentPowerLimit.IsEstimated);
            Assert.True(snapshot.TryEstimateBoardPowerInWatts(400, out var manual));
            Assert.Equal(manual, snapshot.BoardPowerDrawInWatts);
            Assert.Equal(manual, Topology(PowerTopologyDomain.Board, 50).EstimatePowerUsageInWatts(400));
        }

        [Fact]
        public void UnverifiedModelEstimatesAreOptInAndDoNotSupplySliderRanges()
        {
            var snapshot = Snapshot(spec: ReferenceSpec());
            Assert.Null(snapshot.BoardPowerDrawInWatts);
            Assert.Null(snapshot.DefaultTDPInWatts);

            snapshot = Snapshot(options: new GPUPowerTelemetryOptions(allowReferenceDatabaseEstimates: true), spec: ReferenceSpec());
            Assert.Equal(200d, snapshot.BoardPowerDrawInWatts);
            Assert.Equal(GPUPowerDataSource.ReferenceDatabase, snapshot.BoardPower.Source);
            Assert.True(snapshot.BoardPower.IsEstimated);
            Assert.Null(snapshot.CurrentPowerLimitInWatts);
            Assert.Null(snapshot.MinimumPowerLimitInWatts);
            Assert.Null(snapshot.MaximumPowerLimitInWatts);
        }

        [Fact]
        public void ExplicitDomainReferencesRemainSeparate()
        {
            var snapshot = Snapshot(options: new GPUPowerTelemetryOptions(300, 120));
            Assert.Equal(150d, snapshot.BoardPowerDrawInWatts);
            Assert.Equal(30d, snapshot.GPUPowerDrawInWatts);
            Assert.Equal(GPUPowerDataSource.UserProvided, snapshot.GPUPower.Source);
            Assert.True(snapshot.GPUPower.IsEstimated);
        }

        [Fact]
        public void CustomModelOverridesAreExplicitEstimates()
        {
            var snapshot = Snapshot(spec: ReferenceSpec(true));
            Assert.Equal(200d, snapshot.BoardPowerDrawInWatts);
            Assert.Equal(GPUPowerDataSource.UserProvided, snapshot.BoardPower.Source);
            Assert.Equal(200d, snapshot.MinimumPowerLimitInWatts);
            Assert.True(snapshot.MinimumPowerLimit.IsEstimated);
        }

        [Fact]
        public void MissingPowerAndPolicyReadingsStayUnavailable()
        {
            var snapshot = Snapshot(new NvmlPowerData { DefaultLimitWatts = 55 }, boardPercent: null, gpuPercent: null);
            Assert.Null(snapshot.BoardPowerDrawInWatts);
            Assert.Null(snapshot.CurrentPowerLimitInWatts);
            Assert.Null(snapshot.PowerUsageRelativeToLimitInPercent);
            Assert.Equal("N/A", snapshot.BoardPower.ToString());
            Assert.False(snapshot.IsThrottleStatusAvailable);
            Assert.Equal("Unavailable", snapshot.ThrottleStatus);
        }

        [Fact]
        public void ConfiguredAndEnforcedLimitsAreDistinct()
        {
            var snapshot = Snapshot(new NvmlPowerData
            {
                UsageWatts = 150, DefaultLimitWatts = 400, CurrentLimitWatts = 350,
                EnforcedLimitWatts = 300, MinimumLimitWatts = 200, MaximumLimitWatts = 450
            }, targetPercent: 110, spec: ReferenceSpec(true));
            Assert.Equal(350d, snapshot.CurrentPowerLimitInWatts);
            Assert.Equal(300d, snapshot.EnforcedPowerLimit.Watts);
            Assert.Equal(50d, snapshot.PowerUsageRelativeToLimitInPercent);
            Assert.Equal(450d, snapshot.MaximumPowerLimitInWatts);
            Assert.False(snapshot.CurrentPowerLimit.IsEstimated);
        }

        [Fact]
        public void APolicyForAnotherPerformanceStateIsNotUsedAsFallback()
        {
            var snapshot = Snapshot(new NvmlPowerData { DefaultLimitWatts = 400 },
                policies: new[] { Policy(PerformanceStateId.P0_3DPerformance, 110) });
            Assert.Null(snapshot.CurrentPowerLimitInWatts);
        }

        [Fact]
        public void SnapshotOwnsACopyOfPoliciesAndDiagnostics()
        {
            var policies = new List<GPUPowerLimitPolicy> { Policy(PerformanceStateId.All, 110) };
            var diagnostics = new List<string> { "test" };
            var snapshot = Snapshot(new NvmlPowerData { DefaultLimitWatts = 400, Diagnostics = diagnostics }, policies: policies);
            policies.Clear();
            diagnostics.Clear();
            Assert.Single(snapshot.PowerLimitPolicies);
            Assert.Single(snapshot.PowerDiagnostics);
            Assert.Equal(440d, snapshot.CurrentPowerLimitInWatts!.Value, 8);
        }

        [Fact]
        public void ANewUnavailableSnapshotNeverReusesAnOldReading()
        {
            var first = Snapshot(new NvmlPowerData { UsageWatts = 123 });
            var second = Snapshot(NvmlPowerData.Unavailable("GPU unavailable"));
            Assert.Equal(123d, first.BoardPowerDrawInWatts);
            Assert.Null(second.BoardPowerDrawInWatts);
            Assert.Contains("GPU unavailable", second.PowerDiagnostics);
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(0)]
        [InlineData(-1)]
        public void InvalidReferencesAreRejectedAcrossEntryPoints(double invalid)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GPUPowerTelemetryOptions(invalid));
            Assert.Throws<ArgumentOutOfRangeException>(() => new GPUPowerTelemetryOptions(gpuPowerReferenceInWatts: invalid));
            Assert.Throws<ArgumentOutOfRangeException>(() => Snapshot().TryEstimateBoardPowerInWatts(invalid, out _));
            Assert.Throws<ArgumentOutOfRangeException>(() => Policy(PerformanceStateId.All, 100).ToWatts(invalid));
            Assert.Throws<ArgumentOutOfRangeException>(() => Topology(PowerTopologyDomain.Board, 50).EstimatePowerUsageInWatts(invalid));
        }

        [Fact]
        public void OutOfRangePercentagesAndOverflowCannotCreateNonFiniteReadings()
        {
            Assert.False(GPUPowerMath.TryEstimate(float.NaN, 100, out _));
            Assert.False(GPUPowerMath.TryEstimate(float.PositiveInfinity, 100, out _));
            Assert.False(GPUPowerMath.TryEstimate(-1, 100, out _));
            Assert.False(GPUPowerMath.TryEstimate(200, double.MaxValue, out _));
            Assert.True(GPUPowerMath.TryEstimate(120, 100, out var watts));
            Assert.Equal(120d, watts);
        }

        [Fact]
        public void KnownNoThrottleAndPowerThrottleAreDistinguished()
        {
            Assert.Equal("No Throttling", Snapshot(limit: PerformanceLimit.None).ThrottleStatus);
            Assert.True(Snapshot(limit: PerformanceLimit.PowerLimit).IsPowerLimitActive);
            Assert.True(Snapshot(limit: PerformanceLimit.None).IsThrottleStatusAvailable);
        }
    }
}
