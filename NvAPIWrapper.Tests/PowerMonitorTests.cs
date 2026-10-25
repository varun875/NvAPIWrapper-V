using NvAPIRealtimeMonitor;
using NvAPIWrapper.GPU;
using NvAPIWrapper.Native.GPU;
using NvAPIWrapper.Native.Power;
using Xunit;

namespace NvAPIWrapper.Tests
{
    public class PowerMonitorTests
    {
        [Fact]
        public void MissingSamplesStayGapsAndDoNotDepressTheAverage()
        {
            var history = new PowerSampleHistory(4);
            history.Add(new GPUPowerValue(10, GPUPowerDataSource.NVML, false));
            history.Add(null);
            history.Add(GPUPowerValue.Unavailable);
            history.Add(new GPUPowerValue(20, GPUPowerDataSource.NVML, false));
            Assert.Equal(2, history.ValidSampleCount);
            Assert.Equal(15d, history.AverageWatts);
            Assert.Equal(20d, history.PeakWatts);
            Assert.Equal(new double?[] { 10, null, null, 20 }, history.Samples);
        }

        [Fact]
        public void ZeroIsIncludedInStatisticsAndHistoryIsBounded()
        {
            var history = new PowerSampleHistory(2);
            history.Add(new GPUPowerValue(20, GPUPowerDataSource.NVML, false));
            history.Add(new GPUPowerValue(0, GPUPowerDataSource.NVML, false));
            history.Add(null);
            Assert.Equal(10d, history.AverageWatts);
            Assert.Equal(new double?[] { 0, null }, history.Samples);
        }

        [Fact]
        public void ChangingSourceOrQualityStartsANewStatisticsSeries()
        {
            var history = new PowerSampleHistory(4);
            history.Add(new GPUPowerValue(100, GPUPowerDataSource.ReferenceDatabase, true));
            history.Add(new GPUPowerValue(20, GPUPowerDataSource.NVML, false));
            Assert.Equal(1, history.ValidSampleCount);
            Assert.Equal(20d, history.PeakWatts);
            Assert.Equal(GPUPowerDataSource.NVML, history.Source);
            Assert.False(history.IsEstimated);
            history.Add(new GPUPowerValue(30, GPUPowerDataSource.NVML, true));
            Assert.Equal(1, history.ValidSampleCount);
            Assert.Equal(30d, history.AverageWatts);
            Assert.True(history.IsEstimated);
        }

        [Fact]
        public void EmptyAndResetStatisticsAreUnavailable()
        {
            var history = new PowerSampleHistory(4);
            Assert.Null(history.AverageWatts);
            history.Add(new GPUPowerValue(10, GPUPowerDataSource.NVML, false));
            history.Reset();
            Assert.Null(history.PeakWatts);
            Assert.Null(history.AverageWatts);
            Assert.Empty(history.Samples);
        }

        [Fact]
        public void PowerStateMonitorClearsOldPowerAndLimitFlagsOnFailure()
        {
            var monitor = new GPUPowerStateMonitoring(new PhysicalGPU(default));
            monitor.ApplyPowerSnapshot(GPUPowerTelemetryTests.Snapshot(
                new NvmlPowerData { UsageWatts = 50, EnforcedLimitWatts = 100 }, limit: PerformanceLimit.PowerLimit));
            Assert.Equal(50d, monitor.PowerLimitStatus.CurrentPowerW);
            Assert.Equal(100d, monitor.PowerLimitStatus.ActivePowerLimitW);
            Assert.True(monitor.BoostClockInfo.IsThrottledByPower);

            monitor.ApplyPowerSnapshot(null);
            Assert.Null(monitor.PowerSnapshot);
            Assert.Equal(-1d, monitor.PowerLimitStatus.CurrentPowerW);
            Assert.Equal(-1d, monitor.PowerLimitStatus.PowerUtilizationPercent);
            Assert.False(monitor.BoostClockInfo.IsThrottledByPower);
            Assert.False(monitor.PowerLimitStatus.IsLimitStatusAvailable);
            Assert.Contains("N/A", monitor.PowerLimitStatus.ToString());
            Assert.Contains("UNKNOWN", monitor.HealthStatus);
        }
    }
}
