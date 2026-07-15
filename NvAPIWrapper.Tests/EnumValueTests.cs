using System;
using Xunit;
using NvAPIWrapper.Native.GPU;

namespace NvAPIWrapper.Tests
{
    /// <summary>
    ///     Verifies that all expected enum values exist for modern GPU support.
    ///     These tests ensure the enum modernization work is intact.
    /// </summary>
    public class EnumValueTests
    {
        // ── GPUMemoryType ──────────────────────────────────────

        [Fact]
        public void GPUMemoryType_HasGDDR6()
        {
            Assert.Contains(GPUMemoryType.GDDR6, Enum.GetValues<GPUMemoryType>());
        }

        [Fact]
        public void GPUMemoryType_HasGDDR6X()
        {
            Assert.Contains(GPUMemoryType.GDDR6X, Enum.GetValues<GPUMemoryType>());
        }

        [Fact]
        public void GPUMemoryType_HasGDDR7()
        {
            Assert.Contains(GPUMemoryType.GDDR7, Enum.GetValues<GPUMemoryType>());
        }

        [Fact]
        public void GPUMemoryType_HasHBM2()
        {
            Assert.Contains(GPUMemoryType.HBM2, Enum.GetValues<GPUMemoryType>());
        }

        [Fact]
        public void GPUMemoryType_HasHBM2e()
        {
            Assert.Contains(GPUMemoryType.HBM2e, Enum.GetValues<GPUMemoryType>());
        }

        [Fact]
        public void GPUMemoryType_HasHBM3()
        {
            Assert.Contains(GPUMemoryType.HBM3, Enum.GetValues<GPUMemoryType>());
        }

        [Fact]
        public void GPUMemoryType_HasHBM3e()
        {
            Assert.Contains(GPUMemoryType.HBM3e, Enum.GetValues<GPUMemoryType>());
        }

        [Fact]
        public void GPUMemoryType_HasLPDDR5()
        {
            Assert.Contains(GPUMemoryType.LPDDR5, Enum.GetValues<GPUMemoryType>());
        }

        [Fact]
        public void GPUMemoryType_HasDDR5()
        {
            Assert.Contains(GPUMemoryType.DDR5, Enum.GetValues<GPUMemoryType>());
        }

        [Fact]
        public void GPUMemoryType_TotalValues_AtLeast19()
        {
            // Should have at least 19 values: legacy + 9 modern additions
            Assert.True(Enum.GetValues<GPUMemoryType>().Length >= 19,
                $"Expected at least 19 GPUMemoryType values, got {Enum.GetValues<GPUMemoryType>().Length}");
        }

        // ── GPUFoundry ─────────────────────────────────────────

        [Fact]
        public void GPUFoundry_HasSamsung()
        {
            Assert.Contains(GPUFoundry.Samsung, Enum.GetValues<GPUFoundry>());
        }

        [Fact]
        public void GPUFoundry_HasIntelFoundryServices()
        {
            Assert.Contains(GPUFoundry.IntelFoundryServices, Enum.GetValues<GPUFoundry>());
        }

        [Fact]
        public void GPUFoundry_TotalValues_AtLeast8()
        {
            Assert.True(Enum.GetValues<GPUFoundry>().Length >= 8,
                $"Expected at least 8 GPUFoundry values, got {Enum.GetValues<GPUFoundry>().Length}");
        }

        // ── GPUMemoryMaker ─────────────────────────────────────

        [Fact]
        public void GPUMemoryMaker_HasSamsung()
        {
            Assert.Contains(GPUMemoryMaker.Samsung, Enum.GetValues<GPUMemoryMaker>());
        }

        [Fact]
        public void GPUMemoryMaker_HasMicron()
        {
            Assert.Contains(GPUMemoryMaker.Micron, Enum.GetValues<GPUMemoryMaker>());
        }

        [Fact]
        public void GPUMemoryMaker_HasHynix()
        {
            Assert.Contains(GPUMemoryMaker.Hynix, Enum.GetValues<GPUMemoryMaker>());
        }

        // ── SystemType ─────────────────────────────────────────

        [Fact]
        public void SystemType_HasWorkstation()
        {
            Assert.Contains(SystemType.Workstation, Enum.GetValues<SystemType>());
        }

        [Fact]
        public void SystemType_HasDataCenter()
        {
            Assert.Contains(SystemType.DataCenter, Enum.GetValues<SystemType>());
        }

        [Fact]
        public void SystemType_HasHyperscale()
        {
            Assert.Contains(SystemType.Hyperscale, Enum.GetValues<SystemType>());
        }

        [Fact]
        public void SystemType_HasEdge()
        {
            Assert.Contains(SystemType.Edge, Enum.GetValues<SystemType>());
        }

        [Fact]
        public void SystemType_TotalValues_AtLeast7()
        {
            Assert.True(Enum.GetValues<SystemType>().Length >= 7,
                $"Expected at least 7 SystemType values, got {Enum.GetValues<SystemType>().Length}");
        }

        // ── CoolerType ─────────────────────────────────────────

        [Fact]
        public void CoolerType_HasAIOLiquid()
        {
            Assert.Contains(CoolerType.AIOLiquid, Enum.GetValues<CoolerType>());
        }

        [Fact]
        public void CoolerType_HasPassive()
        {
            Assert.Contains(CoolerType.Passive, Enum.GetValues<CoolerType>());
        }

        [Fact]
        public void CoolerType_HasImmersion()
        {
            Assert.Contains(CoolerType.Immersion, Enum.GetValues<CoolerType>());
        }

        [Fact]
        public void CoolerType_TotalValues_AtLeast7()
        {
            Assert.True(Enum.GetValues<CoolerType>().Length >= 7,
                $"Expected at least 7 CoolerType values, got {Enum.GetValues<CoolerType>().Length}");
        }

        // ── PCIeGeneration ─────────────────────────────────────

        [Fact]
        public void PCIeGeneration_HasPCIe4()
        {
            Assert.Contains(PCIeGeneration.PCIe4, Enum.GetValues<PCIeGeneration>());
        }

        [Fact]
        public void PCIeGeneration_HasPCIe5()
        {
            Assert.Contains(PCIeGeneration.PCIe5, Enum.GetValues<PCIeGeneration>());
        }

        [Fact]
        public void PCIeGeneration_TotalValues_AtLeast6()
        {
            Assert.True(Enum.GetValues<PCIeGeneration>().Length >= 6,
                $"Expected at least 6 PCIeGeneration values, got {Enum.GetValues<PCIeGeneration>().Length}");
        }

        // ── PublicClockDomain ──────────────────────────────────

        [Fact]
        public void PublicClockDomain_HasBaseClock()
        {
            Assert.Contains(PublicClockDomain.BaseClock, Enum.GetValues<PublicClockDomain>());
        }

        [Fact]
        public void PublicClockDomain_HasVideoEncode()
        {
            Assert.Contains(PublicClockDomain.VideoEncode, Enum.GetValues<PublicClockDomain>());
        }

        [Fact]
        public void PublicClockDomain_HasTensor()
        {
            Assert.Contains(PublicClockDomain.Tensor, Enum.GetValues<PublicClockDomain>());
        }

        [Fact]
        public void PublicClockDomain_HasDisplay()
        {
            Assert.Contains(PublicClockDomain.Display, Enum.GetValues<PublicClockDomain>());
        }

        // ── PerformanceVoltageDomain ───────────────────────────

        [Fact]
        public void PerformanceVoltageDomain_HasPCIeCore()
        {
            Assert.Contains(PerformanceVoltageDomain.PCIeCore, Enum.GetValues<PerformanceVoltageDomain>());
        }

        [Fact]
        public void PerformanceVoltageDomain_HasSOCCore()
        {
            Assert.Contains(PerformanceVoltageDomain.SOCCore, Enum.GetValues<PerformanceVoltageDomain>());
        }

        [Fact]
        public void PerformanceVoltageDomain_HasMemory()
        {
            Assert.Contains(PerformanceVoltageDomain.Memory, Enum.GetValues<PerformanceVoltageDomain>());
        }

        [Fact]
        public void PerformanceVoltageDomain_TotalValues_AtLeast5()
        {
            Assert.True(Enum.GetValues<PerformanceVoltageDomain>().Length >= 5,
                $"Expected at least 5 PerformanceVoltageDomain values, got {Enum.GetValues<PerformanceVoltageDomain>().Length}");
        }

        // ── NVLinkCapabilityFlags ──────────────────────────────

        [Fact]
        public void NVLinkCapabilityFlags_HasSupported()
        {
            Assert.Contains(NVLinkCapabilityFlags.Supported, Enum.GetValues<NVLinkCapabilityFlags>());
        }

        [Fact]
        public void NVLinkCapabilityFlags_HasP2PSupported()
        {
            Assert.Contains(NVLinkCapabilityFlags.P2PSupported, Enum.GetValues<NVLinkCapabilityFlags>());
        }

        [Fact]
        public void NVLinkCapabilityFlags_HasSysmemAccess()
        {
            Assert.Contains(NVLinkCapabilityFlags.SysmemAccess, Enum.GetValues<NVLinkCapabilityFlags>());
        }

        [Fact]
        public void NVLinkCapabilityFlags_HasSliBridge()
        {
            Assert.Contains(NVLinkCapabilityFlags.SliBridge, Enum.GetValues<NVLinkCapabilityFlags>());
        }

        // ── FanCoolersControlMode ──────────────────────────────

        [Fact]
        public void FanCoolersControlMode_HasAuto()
        {
            Assert.Contains(FanCoolersControlMode.Auto, Enum.GetValues<FanCoolersControlMode>());
        }

        [Fact]
        public void FanCoolersControlMode_HasManual()
        {
            Assert.Contains(FanCoolersControlMode.Manual, Enum.GetValues<FanCoolersControlMode>());
        }
    }
}
