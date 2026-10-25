using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Power;
using Xunit;

namespace NvAPIWrapper.Tests
{
    public class NvmlPowerProviderTests
    {
        private static readonly PowerDeviceIdentity Identity = new PowerDeviceIdentity(1, 0x28E110DE, 0x123410DE);

        private static NvmlPciInfo Pci(uint bus = 1, uint domain = 0, uint deviceId = 0x28E110DE,
            uint subsystem = 0x123410DE) => new NvmlPciInfo
        {
            Bus = bus, Domain = domain, DeviceId = deviceId, SubSystemId = subsystem,
            BusId = $"{domain:X8}:{bus:X2}:00.0", BusIdLegacy = $"{domain:X4}:{bus:X2}:00.0"
        };

        [Fact]
        public void PciV3LayoutMatchesTheNvmlHeader()
        {
            Assert.Equal(68, Marshal.SizeOf<NvmlPciInfo>());
            Assert.Equal(new IntPtr(16), Marshal.OffsetOf<NvmlPciInfo>(nameof(NvmlPciInfo.Domain)));
            Assert.Equal(new IntPtr(28), Marshal.OffsetOf<NvmlPciInfo>(nameof(NvmlPciInfo.DeviceId)));
            Assert.Equal(new IntPtr(36), Marshal.OffsetOf<NvmlPciInfo>(nameof(NvmlPciInfo.BusId)));
        }

        [Fact]
        public void ConvertsMilliwattsAndPreservesIndependentLimits()
        {
            var api = new FakeApi(Pci());
            using var provider = new NvmlPowerProvider(api);
            var data = provider.Read(Identity);
            Assert.Equal(7.251, data.UsageWatts);
            Assert.Equal(55d, data.DefaultLimitWatts);
            Assert.Equal(60d, data.CurrentLimitWatts);
            Assert.Equal(48.27, data.EnforcedLimitWatts);
            Assert.Equal(5d, data.MinimumLimitWatts);
            Assert.Equal(105d, data.MaximumLimitWatts);
            Assert.Empty(data.Diagnostics);
        }

        [Fact]
        public void MatchesPciIdentityInsteadOfEnumerationOrder()
        {
            var api = new FakeApi(Pci(bus: 9), Pci());
            using var provider = new NvmlPowerProvider(api);
            var data = provider.Read(Identity);
            Assert.True(data.HasAnyData);
            Assert.Equal(new IntPtr(2), api.LastPowerDevice);
            Assert.Equal("00000000:01:00.0", data.PciAddress);
        }

        [Theory]
        [InlineData(0x28E110DEu, 0x999910DEu)]
        [InlineData(0x123410DEu, 0x123410DEu)]
        public void DifferentPciIdentifiersNeverMatch(uint deviceId, uint subsystem)
        {
            using var provider = new NvmlPowerProvider(new FakeApi(Pci(deviceId: deviceId, subsystem: subsystem)));
            var data = provider.Read(Identity);
            Assert.False(data.HasAnyData);
            Assert.Contains(data.Diagnostics, d => d.Contains("No NVML device matches"));
        }

        [Fact]
        public void AmbiguousPciDomainsAreRejected()
        {
            var api = new FakeApi(Pci(domain: 0), Pci(domain: 1));
            using var provider = new NvmlPowerProvider(api);
            var data = provider.Read(Identity);
            Assert.False(data.HasAnyData);
            Assert.Equal(IntPtr.Zero, api.LastPowerDevice);
            Assert.Contains(data.Diagnostics, d => d.Contains("ambiguous"));
        }

        [Theory]
        [InlineData(3)]
        [InlineData(13)]
        [InlineData(4)]
        public void OptionalQueryFailureDoesNotEraseOtherSuccessfulReadings(int failureCode)
        {
            var failure = (NvmlReturn)failureCode;
            var api = new FakeApi(Pci());
            api.Statuses[NvmlPowerQuery.CurrentLimit] = failure;
            using var provider = new NvmlPowerProvider(api);
            var data = provider.Read(Identity);
            Assert.Equal(55d, data.DefaultLimitWatts);
            Assert.Equal(7.251, data.UsageWatts);
            Assert.Null(data.CurrentLimitWatts);
            Assert.Contains(data.Diagnostics, d => d.Contains(failure.ToString()));
        }

        [Fact]
        public void MissingNvmlDoesNotThrowOrInventData()
        {
            using var provider = new NvmlPowerProvider(null);
            var data = provider.Read(Identity);
            Assert.False(data.HasAnyData);
            Assert.Single(data.Diagnostics);
        }

        [Fact]
        public void InitializationFailureCanRecoverOnALaterRead()
        {
            var api = new FakeApi(Pci()) { InitializeStatus = NvmlReturn.DriverNotLoaded };
            using var provider = new NvmlPowerProvider(api);
            Assert.False(provider.Read(Identity).HasAnyData);
            api.InitializeStatus = NvmlReturn.Success;
            Assert.True(provider.Read(Identity).HasAnyData);
            Assert.Equal(2, api.InitializeCalls);
        }

        [Fact]
        public void DeviceLossDiscardsPartialDataAndNextReadReenumerates()
        {
            var api = new FakeApi(Pci());
            using var provider = new NvmlPowerProvider(api);
            Assert.True(provider.Read(Identity).HasAnyData);
            api.Statuses[NvmlPowerQuery.CurrentLimit] = NvmlReturn.GpuLost;
            var lost = provider.Read(Identity);
            Assert.False(lost.HasAnyData);
            Assert.Null(lost.UsageWatts);
            api.Statuses.Clear();
            Assert.True(provider.Read(Identity).HasAnyData);
            Assert.Equal(3, api.Enumerations);
        }

        [Fact]
        public void InvalidLimitsAreUnavailableButZeroUsageIsValid()
        {
            var api = new FakeApi(Pci()) { Minimum = 100000, Maximum = 50000 };
            api.Values[NvmlPowerQuery.Usage] = 0;
            api.Values[NvmlPowerQuery.DefaultLimit] = 0;
            api.Values[NvmlPowerQuery.CurrentLimit] = uint.MaxValue;
            using var provider = new NvmlPowerProvider(api);
            var data = provider.Read(Identity);
            Assert.Equal(0d, data.UsageWatts);
            Assert.Null(data.DefaultLimitWatts);
            Assert.Null(data.CurrentLimitWatts);
            Assert.Null(data.MinimumLimitWatts);
            Assert.Null(data.MaximumLimitWatts);
        }

        [Fact]
        public void OwnsOneInitializationReferenceAndReleasesItExactlyOnce()
        {
            var api = new FakeApi(Pci());
            var provider = new NvmlPowerProvider(api);
            provider.Read(Identity);
            provider.Read(Identity);
            provider.Dispose();
            provider.Dispose();
            Assert.Equal(1, api.InitializeCalls);
            Assert.Equal(1, api.ShutdownCalls);
            Assert.Equal(1, api.DisposeCalls);
            Assert.False(provider.Read(Identity).HasAnyData);
        }

        private sealed class FakeApi : INvmlApi
        {
            private readonly NvmlPciInfo[] _devices;
            internal int InitializeCalls, ShutdownCalls, DisposeCalls, Enumerations;
            internal IntPtr LastPowerDevice;
            internal NvmlReturn InitializeStatus = NvmlReturn.Success;
            internal uint Minimum = 5000, Maximum = 105000;
            internal readonly Dictionary<NvmlPowerQuery, NvmlReturn> Statuses = new Dictionary<NvmlPowerQuery, NvmlReturn>();
            internal readonly Dictionary<NvmlPowerQuery, uint> Values = new Dictionary<NvmlPowerQuery, uint>
            {
                [NvmlPowerQuery.Usage] = 7251,
                [NvmlPowerQuery.DefaultLimit] = 55000,
                [NvmlPowerQuery.CurrentLimit] = 60000,
                [NvmlPowerQuery.EnforcedLimit] = 48270
            };

            internal FakeApi(params NvmlPciInfo[] devices) { _devices = devices; }
            public NvmlReturn Initialize() { InitializeCalls++; return InitializeStatus; }
            public NvmlReturn Shutdown() { ShutdownCalls++; return NvmlReturn.Success; }
            public void Dispose() { DisposeCalls++; }
            public NvmlReturn GetCount(out uint count)
            {
                Enumerations++;
                count = (uint)_devices.Length;
                return NvmlReturn.Success;
            }
            public NvmlReturn GetDevice(uint index, out IntPtr device)
            {
                device = new IntPtr(index + 1);
                return NvmlReturn.Success;
            }
            public NvmlReturn GetPciInfo(IntPtr device, out NvmlPciInfo info)
            {
                info = _devices[device.ToInt32() - 1];
                return NvmlReturn.Success;
            }
            public NvmlReturn GetPowerValue(IntPtr device, NvmlPowerQuery query, out uint milliwatts)
            {
                LastPowerDevice = device;
                milliwatts = Values[query];
                return Statuses.TryGetValue(query, out var status) ? status : NvmlReturn.Success;
            }
            public NvmlReturn GetPowerConstraints(IntPtr device, out uint minimum, out uint maximum)
            {
                minimum = Minimum;
                maximum = Maximum;
                return NvmlReturn.Success;
            }
        }
    }
}
