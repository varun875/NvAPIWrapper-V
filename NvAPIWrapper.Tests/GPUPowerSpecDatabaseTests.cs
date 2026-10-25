using Xunit;
using NvAPIWrapper.GPU;

namespace NvAPIWrapper.Tests
{
    public class GPUPowerSpecDatabaseTests
    {
        // ── Known GPU match tests ──────────────────────────────

        [Theory]
        [InlineData("NVIDIA GeForce RTX 4090", 450.0, "Ada Lovelace")]
        [InlineData("NVIDIA GeForce RTX 4080 SUPER", 320.0, "Ada Lovelace")]
        [InlineData("NVIDIA GeForce RTX 4080", 320.0, "Ada Lovelace")]
        [InlineData("NVIDIA GeForce RTX 4070 Ti SUPER", 285.0, "Ada Lovelace")]
        [InlineData("NVIDIA GeForce RTX 4070", 200.0, "Ada Lovelace")]
        [InlineData("NVIDIA GeForce RTX 4060 Ti", 160.0, "Ada Lovelace")]
        [InlineData("NVIDIA GeForce RTX 4060", 115.0, "Ada Lovelace")]
        public void TryGetSpec_ReturnsKnownDesktopAdaGPUs(string fullName, double expectedTDP, string expectedArch)
        {
            var found = GPUPowerSpecDatabase.TryGetSpec(fullName, out var spec);

            Assert.True(found);
            Assert.NotNull(spec);
            Assert.Equal(expectedTDP, spec!.DefaultTDPWatts);
            Assert.Equal(expectedArch, spec.Architecture);
        }

        [Theory]
        [InlineData("NVIDIA GeForce RTX 5090", 575.0, "Blackwell")]
        [InlineData("NVIDIA GeForce RTX 5080", 360.0, "Blackwell")]
        [InlineData("NVIDIA GeForce RTX 5070 Ti", 300.0, "Blackwell")]
        [InlineData("NVIDIA GeForce RTX 5070", 250.0, "Blackwell")]
        public void TryGetSpec_ReturnsKnownDesktopBlackwellGPUs(string fullName, double expectedTDP, string expectedArch)
        {
            var found = GPUPowerSpecDatabase.TryGetSpec(fullName, out var spec);

            Assert.True(found);
            Assert.NotNull(spec);
            Assert.Equal(expectedTDP, spec!.DefaultTDPWatts);
            Assert.Equal(expectedArch, spec.Architecture);
        }

        [Theory]
        [InlineData("NVIDIA GeForce RTX 3090 Ti", 450.0, "Ampere")]
        [InlineData("NVIDIA GeForce RTX 3080", 320.0, "Ampere")]
        [InlineData("NVIDIA GeForce RTX 3070", 220.0, "Ampere")]
        [InlineData("NVIDIA GeForce RTX 3060 Ti", 200.0, "Ampere")]
        [InlineData("NVIDIA GeForce RTX 3050", 130.0, "Ampere")]
        public void TryGetSpec_ReturnsKnownDesktopAmpereGPUs(string fullName, double expectedTDP, string expectedArch)
        {
            var found = GPUPowerSpecDatabase.TryGetSpec(fullName, out var spec);

            Assert.True(found);
            Assert.NotNull(spec);
            Assert.Equal(expectedTDP, spec!.DefaultTDPWatts);
            Assert.Equal(expectedArch, spec.Architecture);
        }

        [Theory]
        [InlineData("NVIDIA GeForce RTX 2080 Ti", 250.0, "Turing")]
        [InlineData("NVIDIA GeForce RTX 2070 SUPER", 215.0, "Turing")]
        [InlineData("NVIDIA GeForce RTX 2060", 160.0, "Turing")]
        public void TryGetSpec_ReturnsKnownDesktopTuringGPUs(string fullName, double expectedTDP, string expectedArch)
        {
            var found = GPUPowerSpecDatabase.TryGetSpec(fullName, out var spec);

            Assert.True(found);
            Assert.NotNull(spec);
            Assert.Equal(expectedTDP, spec!.DefaultTDPWatts);
            Assert.Equal(expectedArch, spec.Architecture);
        }

        [Theory]
        [InlineData("NVIDIA GeForce GTX 1080 Ti", 250.0, "Pascal")]
        [InlineData("NVIDIA GeForce GTX 1070", 150.0, "Pascal")]
        [InlineData("NVIDIA GeForce GTX 1060 6GB", 120.0, "Pascal")]
        public void TryGetSpec_ReturnsKnownDesktopPascalGPUs(string fullName, double expectedTDP, string expectedArch)
        {
            var found = GPUPowerSpecDatabase.TryGetSpec(fullName, out var spec);

            Assert.True(found);
            Assert.NotNull(spec);
            Assert.Equal(expectedTDP, spec!.DefaultTDPWatts);
            Assert.Equal(expectedArch, spec.Architecture);
        }

        // ── Laptop GPU tests ───────────────────────────────────

        [Theory]
        [InlineData("NVIDIA GeForce RTX 4090 Laptop GPU", 150.0, "Ada Lovelace")]
        [InlineData("NVIDIA GeForce RTX 4080 Laptop GPU", 150.0, "Ada Lovelace")]
        [InlineData("NVIDIA GeForce RTX 4070 Laptop GPU", 115.0, "Ada Lovelace")]
        [InlineData("NVIDIA GeForce RTX 4060 Laptop GPU", 115.0, "Ada Lovelace")]
        [InlineData("NVIDIA GeForce RTX 4050 Laptop GPU", 115.0, "Ada Lovelace")]
        public void TryGetSpec_ReturnsKnownLaptopAdaGPUs(string fullName, double expectedTDP, string expectedArch)
        {
            var found = GPUPowerSpecDatabase.TryGetSpec(fullName, out var spec);

            Assert.True(found);
            Assert.NotNull(spec);
            Assert.Equal(expectedTDP, spec!.DefaultTDPWatts);
            Assert.Equal(expectedArch, spec.Architecture);
        }

        // ── Professional / Data Center tests ───────────────────

        [Theory]
        [InlineData("NVIDIA RTX 6000 Ada Generation", 300.0, "Ada Lovelace")]
        [InlineData("NVIDIA RTX A6000", 300.0, "Ampere")]
        [InlineData("NVIDIA H100", 700.0, "Hopper")]
        [InlineData("NVIDIA H200", 700.0, "Hopper")]
        public void TryGetSpec_ReturnsKnownProfessionalGPUs(string fullName, double expectedTDP, string expectedArch)
        {
            var found = GPUPowerSpecDatabase.TryGetSpec(fullName, out var spec);

            Assert.True(found);
            Assert.NotNull(spec);
            Assert.Equal(expectedTDP, spec!.DefaultTDPWatts);
            Assert.Equal(expectedArch, spec.Architecture);
        }

        // ── Edge cases ─────────────────────────────────────────

        [Fact]
        public void TryGetSpec_ReturnsFalseForNullOrEmpty()
        {
            Assert.False(GPUPowerSpecDatabase.TryGetSpec(null, out _));
            Assert.False(GPUPowerSpecDatabase.TryGetSpec("", out _));
            Assert.False(GPUPowerSpecDatabase.TryGetSpec("   ", out _));
        }

        [Fact]
        public void TryGetSpec_ReturnsFalseForUnknownGPU()
        {
            Assert.False(GPUPowerSpecDatabase.TryGetSpec("NVIDIA GeForce GT 710", out _));
            Assert.False(GPUPowerSpecDatabase.TryGetSpec("Intel UHD Graphics 630", out _));
        }

        [Fact]
        public void TryGetSpec_IsCaseInsensitive()
        {
            var found = GPUPowerSpecDatabase.TryGetSpec("nvidia geforce rtx 4090", out var spec);
            Assert.True(found);
            Assert.NotNull(spec);
        }

        // ── Matching priority tests ────────────────────────────

        [Fact]
        public void TryGetSpec_PrefersMoreSpecificName()
        {
            // "RTX 4080 SUPER" (length 13) should match before "RTX 4080" (length 9)
            var found = GPUPowerSpecDatabase.TryGetSpec("NVIDIA GeForce RTX 4080 SUPER", out var spec);
            Assert.True(found);
            Assert.NotNull(spec);
            Assert.Equal("RTX 4080 SUPER", spec!.NamePattern);
            Assert.Equal(320.0, spec.DefaultTDPWatts);
        }

        [Fact]
        public void TryGetSpec_PrefersMoreSpecificNameForTi()
        {
            // "RTX 3070 Ti" should match before "RTX 3070"
            var found = GPUPowerSpecDatabase.TryGetSpec("NVIDIA GeForce RTX 3070 Ti", out var spec);
            Assert.True(found);
            Assert.NotNull(spec);
            Assert.Equal("RTX 3070 Ti", spec!.NamePattern);
            Assert.Equal(290.0, spec.DefaultTDPWatts);
        }

        [Fact]
        public void TryGetSpec_PrefersMoreSpecificNameForSuper()
        {
            // "RTX 4070 SUPER" should match before "RTX 4070"
            var found = GPUPowerSpecDatabase.TryGetSpec("NVIDIA GeForce RTX 4070 SUPER", out var spec);
            Assert.True(found);
            Assert.NotNull(spec);
            Assert.Equal("RTX 4070 SUPER", spec!.NamePattern);
            Assert.Equal(220.0, spec.DefaultTDPWatts);
        }

        // ── GetDefaultTDP tests ────────────────────────────────

        [Fact]
        public void GetDefaultTDP_ReturnsCorrectValue()
        {
            var tdp = GPUPowerSpecDatabase.GetDefaultTDP("NVIDIA GeForce RTX 4090");
            Assert.Equal(450.0, tdp);
        }

        [Fact]
        public void GetDefaultTDP_ReturnsNullForUnknown()
        {
            var tdp = GPUPowerSpecDatabase.GetDefaultTDP("Unknown GPU");
            Assert.Null(tdp);
        }

        // ── AllSpecs tests ─────────────────────────────────────

        [Fact]
        public void AllSpecs_ContainsExpectedCount()
        {
            // At minimum, should contain all entries defined in the static list.
            // This is a sanity check - update the expected count as the DB grows.
            var all = GPUPowerSpecDatabase.AllSpecs;
            Assert.NotNull(all);
            Assert.True(all.Count >= 80, $"Expected at least 80 specs, found {all.Count}");
        }

        [Fact]
        public void AllSpecs_EverySpecHasValidTDP()
        {
            foreach (var spec in GPUPowerSpecDatabase.AllSpecs)
            {
                Assert.False(string.IsNullOrWhiteSpace(spec.NamePattern), "NamePattern should not be empty");
                Assert.True(spec.DefaultTDPWatts > 0, $"DefaultTDPWatts for '{spec.NamePattern}' should be > 0");
                Assert.True(spec.MaxTDPWatts >= spec.DefaultTDPWatts,
                    $"MaxTDPWatts for '{spec.NamePattern}' should be >= DefaultTDPWatts");
                Assert.True(spec.MaxTDPWatts > 0, $"MaxTDPWatts for '{spec.NamePattern}' should be > 0");
                Assert.True(spec.MinTDPWatts > 0, $"MinTDPWatts for '{spec.NamePattern}' should be > 0");
                Assert.False(string.IsNullOrWhiteSpace(spec.Architecture), $"Architecture for '{spec.NamePattern}' should not be empty");
            }
        }

        // ── RegisterSpec tests ─────────────────────────────────
        // Always remove overrides: xUnit does not guarantee test order.

        [Fact]
        public void RegisterSpec_AddsNewGPU()
        {
            try
            {
                GPUPowerSpecDatabase.RegisterSpec("RTX 9090", 999, 1100, 700, "Future");
                Assert.True(GPUPowerSpecDatabase.TryGetSpec("NVIDIA GeForce RTX 9090", out var spec));
                Assert.Equal(999.0, spec!.DefaultTDPWatts);
                Assert.Equal("Future", spec.Architecture);
                Assert.True(spec.IsUserProvided);
            }
            finally { GPUPowerSpecDatabase.UnregisterSpec("RTX 9090"); }
        }

        [Fact]
        public void RegisterSpec_UserSpecTakesPriority()
        {
            try
            {
                // A different test class may read standard desktop entries concurrently.
                GPUPowerSpecDatabase.RegisterSpec("TITAN Xp", 270, 300, 200, "Custom");
                Assert.True(GPUPowerSpecDatabase.TryGetSpec("NVIDIA TITAN Xp", out var spec));
                Assert.Equal(270.0, spec!.DefaultTDPWatts);
                GPUPowerSpecDatabase.RegisterSpec("nvidia titan xp", 280, 300, 200, "Custom");
                Assert.Equal(280.0, GPUPowerSpecDatabase.GetDefaultTDP("TITAN Xp"));
            }
            finally { GPUPowerSpecDatabase.UnregisterSpec("TITAN Xp"); }
            Assert.Equal(250.0, GPUPowerSpecDatabase.GetDefaultTDP("TITAN Xp"));
        }

        [Fact]
        public void RegisterSpec_ThrowsOnInvalidInput()
        {
            Assert.Throws<System.ArgumentException>(() =>
                GPUPowerSpecDatabase.RegisterSpec("", 100, 150, 50));

            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                GPUPowerSpecDatabase.RegisterSpec("Test GPU", -1, 150, 50));

            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                GPUPowerSpecDatabase.RegisterSpec("Test GPU", 0, 150, 50));
        }

        [Fact]
        public void RegisterSpec_MatchesExactVariant()
        {
            try
            {
                GPUPowerSpecDatabase.RegisterSpec("RTX 4090 Ti SUPER", 600, 700, 400, "Custom");
                Assert.True(GPUPowerSpecDatabase.TryGetSpec("NVIDIA GeForce RTX 4090 Ti SUPER", out var spec));
                Assert.Equal("RTX 4090 Ti SUPER", spec!.NamePattern);
                Assert.Equal(450.0, GPUPowerSpecDatabase.GetDefaultTDP("RTX 4090"));
            }
            finally { GPUPowerSpecDatabase.UnregisterSpec("RTX 4090 Ti SUPER"); }
        }

        [Theory]
        [InlineData("NVIDIA RTX A2000")]
        [InlineData("NVIDIA RTX A2000 12GB")]
        [InlineData("NVIDIA A100 UNKNOWN")]
        [InlineData("NVIDIA RTX 40900")]
        [InlineData("NVIDIA RTX 4090 Ti")]
        [InlineData("NVIDIA RTX 4090 Laptop Super")]
        [InlineData("NVIDIA RTX 5060 Max-Q")]
        [InlineData("AMD A2")]
        [InlineData("prefix A10 suffix")]
        [InlineData("NVIDIA RTX 3080 20GB")]
        public void TryGetSpec_UnknownModelsNeverInheritPartialMatches(string name)
        {
            Assert.False(GPUPowerSpecDatabase.TryGetSpec(name, out var spec));
            Assert.Null(spec);
        }

        [Theory]
        [InlineData("NVIDIA A100-SXM4-40GB", "A100 SXM", 400)]
        [InlineData("NVIDIA A100_SXM4_80GB", "A100 SXM", 400)]
        [InlineData("  nvidia  geforce  rtx  4090  laptop  gpu ", "RTX 4090 Laptop", 150)]
        [InlineData("NVIDIA GeForce RTX 4090 Mobile GPU", "RTX 4090 Laptop", 150)]
        [InlineData("NVIDIA RTX 6000 Ada Generation", "RTX 6000 Ada", 300)]
        [InlineData("NVIDIA RTX 4060 Ti 16 GB", "RTX 4060 Ti 16GB", 165)]
        [InlineData("NVIDIA H100 PCI-E", "H100 PCIe", 350)]
        [InlineData("NVIDIA L40S", "L40S", 350)]
        [InlineData("NVIDIA® GeForce™ RTX 4090", "RTX 4090", 450)]
        public void TryGetSpec_NormalizesOnlyKnownDecorationsAndAliases(string name, string expectedName, double watts)
        {
            Assert.True(GPUPowerSpecDatabase.TryGetSpec(name, out var spec));
            Assert.Equal(expectedName, spec!.NamePattern);
            Assert.Equal(watts, spec.DefaultTDPWatts);
            Assert.False(spec.IsUserProvided);
        }

        [Theory]
        [InlineData(double.NaN, 150, 50)]
        [InlineData(double.PositiveInfinity, 150, 50)]
        [InlineData(100, double.PositiveInfinity, 50)]
        [InlineData(100, 150, double.NaN)]
        [InlineData(100, 90, 50)]
        [InlineData(100, 150, 110)]
        [InlineData(100, 150, -10)]
        public void RegisterSpec_RejectsNonFiniteAndInvertedLimits(double normal, double maximum, double minimum)
        {
            Assert.ThrowsAny<System.ArgumentException>(() =>
                GPUPowerSpecDatabase.RegisterSpec("Invalid test specification", normal, maximum, minimum));
        }

        [Fact]
        public void AllSpecs_IsAnImmutablePointInTimeSnapshot()
        {
            var before = GPUPowerSpecDatabase.AllSpecs;
            var count = before.Count;
            try
            {
                GPUPowerSpecDatabase.RegisterSpec("Snapshot test GPU", 100, 150, 50);
                Assert.Equal(count, before.Count);
                Assert.DoesNotContain(before, s => s.NamePattern == "Snapshot test GPU");
                Assert.Throws<System.NotSupportedException>(() =>
                    ((System.Collections.Generic.IList<GPUPowerSpecDatabase.GPUPowerSpec>)before).Clear());
            }
            finally { GPUPowerSpecDatabase.UnregisterSpec("Snapshot test GPU"); }
        }

        [Fact]
        public void ConcurrentRegistrationAndEnumerationDoNotCorruptTheRegistry()
        {
            try
            {
                System.Threading.Tasks.Parallel.For(0, 50, i =>
                {
                    GPUPowerSpecDatabase.RegisterSpec("Concurrent test GPU " + i, 100, 150, 50);
                    foreach (var spec in GPUPowerSpecDatabase.AllSpecs) Assert.True(spec.DefaultTDPWatts > 0);
                    Assert.True(GPUPowerSpecDatabase.TryGetSpec("Concurrent test GPU " + i, out _));
                });
            }
            finally
            {
                for (var i = 0; i < 50; i++) GPUPowerSpecDatabase.UnregisterSpec("Concurrent test GPU " + i);
            }
        }
    }
}
