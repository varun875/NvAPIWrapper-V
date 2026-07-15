using Xunit;
using NvAPIWrapper.GPU;
using NvAPIWrapper.Native.GPU;

namespace NvAPIWrapper.Tests
{
    public class GPUFamilyClassifierTests
    {
        // ── DetectFamily tests ─────────────────────────────────

        [Theory]
        [InlineData("GK104", GPUFamilyClassifier.GPUFamily.Kepler)]
        [InlineData("GK110", GPUFamilyClassifier.GPUFamily.Kepler)]
        [InlineData("GM200", GPUFamilyClassifier.GPUFamily.Maxwell)]
        [InlineData("GM107", GPUFamilyClassifier.GPUFamily.Maxwell)]
        [InlineData("GP102", GPUFamilyClassifier.GPUFamily.Pascal)]
        [InlineData("GP104", GPUFamilyClassifier.GPUFamily.Pascal)]
        [InlineData("GV100", GPUFamilyClassifier.GPUFamily.Volta)]
        [InlineData("TU102", GPUFamilyClassifier.GPUFamily.Turing)]
        [InlineData("TU104", GPUFamilyClassifier.GPUFamily.Turing)]
        [InlineData("GA102", GPUFamilyClassifier.GPUFamily.Ampere)]
        [InlineData("GA104", GPUFamilyClassifier.GPUFamily.Ampere)]
        [InlineData("AD102", GPUFamilyClassifier.GPUFamily.Ada)]
        [InlineData("AD104", GPUFamilyClassifier.GPUFamily.Ada)]
        [InlineData("GB202", GPUFamilyClassifier.GPUFamily.Blackwell)]
        [InlineData("BL100", GPUFamilyClassifier.GPUFamily.Blackwell)]
        [InlineData("GH100", GPUFamilyClassifier.GPUFamily.Hopper)]
        [InlineData("H100", GPUFamilyClassifier.GPUFamily.Hopper)]
        [InlineData("H200", GPUFamilyClassifier.GPUFamily.Hopper)]
        [InlineData("ORIN", GPUFamilyClassifier.GPUFamily.Orin)]
        public void DetectFamily_IdentifiesKnownCodenames(string shortName, GPUFamilyClassifier.GPUFamily expected)
        {
            var result = GPUFamilyClassifier.DetectFamily(shortName);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void DetectFamily_ReturnsUnknownForNull()
        {
            Assert.Equal(GPUFamilyClassifier.GPUFamily.Unknown, GPUFamilyClassifier.DetectFamily(null));
        }

        [Fact]
        public void DetectFamily_ReturnsUnknownForEmpty()
        {
            Assert.Equal(GPUFamilyClassifier.GPUFamily.Unknown, GPUFamilyClassifier.DetectFamily(""));
        }

        [Fact]
        public void DetectFamily_ReturnsUnknownForUnrecognized()
        {
            Assert.Equal(GPUFamilyClassifier.GPUFamily.Unknown, GPUFamilyClassifier.DetectFamily("XYZ123"));
        }

        [Fact]
        public void DetectFamily_IsCaseInsensitive()
        {
            Assert.Equal(GPUFamilyClassifier.GPUFamily.Ada, GPUFamilyClassifier.DetectFamily("ad102"));
            Assert.Equal(GPUFamilyClassifier.GPUFamily.Turing, GPUFamilyClassifier.DetectFamily("tu104"));
        }

        // ── SupportsNVLink tests ───────────────────────────────

        [Theory]
        [InlineData(GPUFamilyClassifier.GPUFamily.Volta, true)]
        [InlineData(GPUFamilyClassifier.GPUFamily.Ampere, true)]
        [InlineData(GPUFamilyClassifier.GPUFamily.Ada, true)]
        [InlineData(GPUFamilyClassifier.GPUFamily.Hopper, true)]
        [InlineData(GPUFamilyClassifier.GPUFamily.Kepler, false)]
        [InlineData(GPUFamilyClassifier.GPUFamily.Maxwell, false)]
        [InlineData(GPUFamilyClassifier.GPUFamily.Pascal, false)]
        [InlineData(GPUFamilyClassifier.GPUFamily.Turing, false)]
        [InlineData(GPUFamilyClassifier.GPUFamily.Blackwell, false)]
        [InlineData(GPUFamilyClassifier.GPUFamily.Orin, false)]
        [InlineData(GPUFamilyClassifier.GPUFamily.Unknown, false)]
        public void SupportsNVLink_MatchesExpected(GPUFamilyClassifier.GPUFamily family, bool expected)
        {
            Assert.Equal(expected, GPUFamilyClassifier.SupportsNVLink(family));
        }

        // ── UsesModernMemory tests ─────────────────────────────

        [Theory]
        [InlineData(GPUMemoryType.GDDR6, true)]
        [InlineData(GPUMemoryType.GDDR6X, true)]
        [InlineData(GPUMemoryType.GDDR7, true)]
        [InlineData(GPUMemoryType.HBM2, true)]
        [InlineData(GPUMemoryType.HBM2e, true)]
        [InlineData(GPUMemoryType.HBM3, true)]
        [InlineData(GPUMemoryType.HBM3e, true)]
        [InlineData(GPUMemoryType.GDDR2, false)]
        [InlineData(GPUMemoryType.GDDR3, false)]
        [InlineData(GPUMemoryType.GDDR4, false)]
        [InlineData(GPUMemoryType.GDDR5, false)]
        [InlineData(GPUMemoryType.GDDR5X, false)]
        [InlineData(GPUMemoryType.LPDDR5, false)]
        [InlineData(GPUMemoryType.DDR5, false)]
        [InlineData(GPUMemoryType.Unknown, false)]
        public void UsesModernMemory_MatchesExpected(GPUMemoryType memoryType, bool expected)
        {
            Assert.Equal(expected, GPUFamilyClassifier.UsesModernMemory(memoryType));
        }

        // ── SupportsModernPCIe tests ───────────────────────────

        [Theory]
        [InlineData(PCIeGeneration.PCIe4, true)]
        [InlineData(PCIeGeneration.PCIe5, true)]
        [InlineData(PCIeGeneration.PCIe1, false)]
        [InlineData(PCIeGeneration.PCIe1Minor1, false)]
        [InlineData(PCIeGeneration.PCIe2, false)]
        [InlineData(PCIeGeneration.PCIe3, false)]
        public void SupportsModernPCIe_MatchesExpected(PCIeGeneration generation, bool expected)
        {
            Assert.Equal(expected, GPUFamilyClassifier.SupportsModernPCIe(generation));
        }

        // ── IsComputeCapable tests ─────────────────────────────

        [Fact]
        public void IsComputeCapable_ReturnsTrueForAllKnownFamilies()
        {
            var knownFamilies = new[]
            {
                GPUFamilyClassifier.GPUFamily.Kepler,
                GPUFamilyClassifier.GPUFamily.Maxwell,
                GPUFamilyClassifier.GPUFamily.Pascal,
                GPUFamilyClassifier.GPUFamily.Volta,
                GPUFamilyClassifier.GPUFamily.Turing,
                GPUFamilyClassifier.GPUFamily.Ampere,
                GPUFamilyClassifier.GPUFamily.Ada,
                GPUFamilyClassifier.GPUFamily.Blackwell,
                GPUFamilyClassifier.GPUFamily.Hopper,
                GPUFamilyClassifier.GPUFamily.Orin
            };

            foreach (var family in knownFamilies)
            {
                Assert.True(GPUFamilyClassifier.IsComputeCapable(family),
                    $"{family} should be compute capable");
            }
        }

        [Fact]
        public void IsComputeCapable_ReturnsFalseForUnknown()
        {
            Assert.False(GPUFamilyClassifier.IsComputeCapable(GPUFamilyClassifier.GPUFamily.Unknown));
        }

        // ── GetFamilyDescription tests ─────────────────────────

        [Theory]
        [InlineData(GPUFamilyClassifier.GPUFamily.Kepler, "Kepler")]
        [InlineData(GPUFamilyClassifier.GPUFamily.Ada, "Ada")]
        [InlineData(GPUFamilyClassifier.GPUFamily.Blackwell, "Blackwell")]
        [InlineData(GPUFamilyClassifier.GPUFamily.Hopper, "Hopper")]
        public void GetFamilyDescription_ContainsFamilyName(GPUFamilyClassifier.GPUFamily family, string expectedSubstring)
        {
            var description = GPUFamilyClassifier.GetFamilyDescription(family);
            Assert.Contains(expectedSubstring, description);
        }

        [Fact]
        public void GetFamilyDescription_ReturnsUnknownForUnknown()
        {
            Assert.Equal("Unknown", GPUFamilyClassifier.GetFamilyDescription(GPUFamilyClassifier.GPUFamily.Unknown));
        }

        [Fact]
        public void GetFamilyDescription_AllFamiliesHaveNonEmptyDescription()
        {
            foreach (GPUFamilyClassifier.GPUFamily family in System.Enum.GetValues(typeof(GPUFamilyClassifier.GPUFamily)))
            {
                var description = GPUFamilyClassifier.GetFamilyDescription(family);
                Assert.False(string.IsNullOrWhiteSpace(description),
                    $"Family {family} should have a non-empty description");
            }
        }

        // ── GetTierDescription tests ───────────────────────────

        [Fact]
        public void GetTierDescription_AllTiersHaveNonEmptyDescription()
        {
            foreach (GPUFamilyClassifier.GPUTier tier in System.Enum.GetValues(typeof(GPUFamilyClassifier.GPUTier)))
            {
                var description = GPUFamilyClassifier.GetTierDescription(tier);
                Assert.False(string.IsNullOrWhiteSpace(description),
                    $"Tier {tier} should have a non-empty description");
            }
        }
    }
}
