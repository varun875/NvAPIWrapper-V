#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace NvAPIWrapper.GPU
{
    /// <summary>
    ///     Contains known TDP/TGP specifications for NVIDIA GPUs.
    ///     This database is used to convert NVAPI power percentage telemetry into actual watt values.
    ///     The NVAPI private power topology API reports power as a percentage of the board's default power limit (TDP/TGP).
    ///     Without knowing the reference TDP in watts, the percentage is meaningless for watt estimation.
    /// </summary>
    public static class GPUPowerSpecDatabase
    {
        /// <summary>
        ///     Represents the power specification for a known GPU model.
        /// </summary>
        public sealed class GPUPowerSpec
        {
            /// <summary>
            ///     GPU model name pattern (matched against NvAPI FullName).
            /// </summary>
            public string NamePattern { get; }

            /// <summary>
            ///     Default board TDP/TGP in watts.
            /// </summary>
            public double DefaultTDPWatts { get; }

            /// <summary>
            ///     Maximum board power limit in watts (when slider is at max in tools like Afterburner).
            /// </summary>
            public double MaxTDPWatts { get; }

            /// <summary>
            ///     Minimum board power limit in watts (when power limit is reduced).
            /// </summary>
            public double MinTDPWatts { get; }

            /// <summary>
            ///     GPU architecture generation.
            /// </summary>
            public string Architecture { get; }

            internal GPUPowerSpec(string namePattern, double defaultTdp, double maxTdp, double minTdp, string architecture)
            {
                NamePattern = namePattern;
                DefaultTDPWatts = defaultTdp;
                MaxTDPWatts = maxTdp;
                MinTDPWatts = minTdp;
                Architecture = architecture;
            }
        }

        private static readonly List<GPUPowerSpec> KnownSpecs = new List<GPUPowerSpec>
        {
            // =====================================================
            // RTX 50 Series (Blackwell) - Desktop
            // =====================================================
            new GPUPowerSpec("RTX 5090", 575, 660, 400, "Blackwell"),
            new GPUPowerSpec("RTX 5080", 360, 410, 250, "Blackwell"),
            new GPUPowerSpec("RTX 5070 Ti", 300, 350, 200, "Blackwell"),
            new GPUPowerSpec("RTX 5070", 250, 300, 175, "Blackwell"),
            new GPUPowerSpec("RTX 5060 Ti", 180, 210, 120, "Blackwell"),
            new GPUPowerSpec("RTX 5060", 150, 175, 100, "Blackwell"),

            // RTX 50 Series (Blackwell) - Laptop
            new GPUPowerSpec("RTX 5090 Laptop", 150, 175, 80, "Blackwell"),
            new GPUPowerSpec("RTX 5080 Laptop", 150, 175, 80, "Blackwell"),
            new GPUPowerSpec("RTX 5070 Ti Laptop", 120, 140, 60, "Blackwell"),
            new GPUPowerSpec("RTX 5070 Laptop", 115, 135, 60, "Blackwell"),
            new GPUPowerSpec("RTX 5060 Laptop", 100, 115, 50, "Blackwell"),

            // =====================================================
            // RTX 40 Series (Ada Lovelace) - Desktop
            // =====================================================
            new GPUPowerSpec("RTX 4090", 450, 660, 300, "Ada Lovelace"),
            new GPUPowerSpec("RTX 4090 D", 425, 550, 300, "Ada Lovelace"),
            new GPUPowerSpec("RTX 4080 SUPER", 320, 380, 220, "Ada Lovelace"),
            new GPUPowerSpec("RTX 4080", 320, 380, 220, "Ada Lovelace"),
            new GPUPowerSpec("RTX 4070 Ti SUPER", 285, 330, 200, "Ada Lovelace"),
            new GPUPowerSpec("RTX 4070 Ti", 285, 330, 200, "Ada Lovelace"),
            new GPUPowerSpec("RTX 4070 SUPER", 220, 260, 150, "Ada Lovelace"),
            new GPUPowerSpec("RTX 4070", 200, 240, 140, "Ada Lovelace"),
            new GPUPowerSpec("RTX 4060 Ti 16GB", 165, 195, 115, "Ada Lovelace"),
            new GPUPowerSpec("RTX 4060 Ti", 160, 190, 115, "Ada Lovelace"),
            new GPUPowerSpec("RTX 4060", 115, 140, 80, "Ada Lovelace"),

            // RTX 40 Series (Ada Lovelace) - Laptop
            new GPUPowerSpec("RTX 4090 Laptop", 150, 175, 80, "Ada Lovelace"),
            new GPUPowerSpec("RTX 4080 Laptop", 150, 175, 80, "Ada Lovelace"),
            new GPUPowerSpec("RTX 4070 Laptop", 115, 140, 60, "Ada Lovelace"),
            new GPUPowerSpec("RTX 4060 Laptop", 115, 140, 35, "Ada Lovelace"),
            new GPUPowerSpec("RTX 4050 Laptop", 115, 140, 35, "Ada Lovelace"),

            // =====================================================
            // RTX 30 Series (Ampere) - Desktop
            // =====================================================
            new GPUPowerSpec("RTX 3090 Ti", 450, 516, 350, "Ampere"),
            new GPUPowerSpec("RTX 3090", 350, 400, 280, "Ampere"),
            new GPUPowerSpec("RTX 3080 Ti", 350, 400, 280, "Ampere"),
            new GPUPowerSpec("RTX 3080 12GB", 350, 400, 280, "Ampere"),
            new GPUPowerSpec("RTX 3080", 320, 370, 250, "Ampere"),
            new GPUPowerSpec("RTX 3070 Ti", 290, 335, 220, "Ampere"),
            new GPUPowerSpec("RTX 3070", 220, 260, 170, "Ampere"),
            new GPUPowerSpec("RTX 3060 Ti", 200, 240, 150, "Ampere"),
            new GPUPowerSpec("RTX 3060", 170, 200, 120, "Ampere"),

            // =====================================================
            // RTX 20 Series (Turing) - Desktop
            // =====================================================
            new GPUPowerSpec("RTX 2080 Ti", 250, 300, 200, "Turing"),
            new GPUPowerSpec("RTX 2080 SUPER", 250, 300, 200, "Turing"),
            new GPUPowerSpec("RTX 2080", 215, 260, 170, "Turing"),
            new GPUPowerSpec("RTX 2070 SUPER", 215, 260, 170, "Turing"),
            new GPUPowerSpec("RTX 2070", 175, 215, 140, "Turing"),
            new GPUPowerSpec("RTX 2060 SUPER", 175, 215, 140, "Turing"),
            new GPUPowerSpec("RTX 2060", 160, 190, 125, "Turing"),

            // =====================================================
            // GTX 16 Series (Turing) - Desktop
            // =====================================================
            new GPUPowerSpec("GTX 1660 Ti", 120, 145, 90, "Turing"),
            new GPUPowerSpec("GTX 1660 SUPER", 125, 150, 95, "Turing"),
            new GPUPowerSpec("GTX 1660", 120, 145, 90, "Turing"),
            new GPUPowerSpec("GTX 1650 SUPER", 100, 120, 75, "Turing"),
            new GPUPowerSpec("GTX 1650", 75, 90, 55, "Turing"),

            // =====================================================
            // Professional / Data Center
            // =====================================================
            new GPUPowerSpec("RTX 6000 Ada", 300, 350, 200, "Ada Lovelace"),
            new GPUPowerSpec("RTX 5880 Ada", 285, 330, 200, "Ada Lovelace"),
            new GPUPowerSpec("RTX 5000 Ada", 250, 290, 170, "Ada Lovelace"),
            new GPUPowerSpec("RTX 4500 Ada", 210, 250, 150, "Ada Lovelace"),
            new GPUPowerSpec("RTX 4000 Ada", 130, 155, 90, "Ada Lovelace"),
            new GPUPowerSpec("RTX A6000", 300, 350, 200, "Ampere"),
            new GPUPowerSpec("RTX A5500", 230, 270, 160, "Ampere"),
            new GPUPowerSpec("RTX A5000", 230, 270, 160, "Ampere"),
            new GPUPowerSpec("RTX A4500", 200, 240, 140, "Ampere"),
            new GPUPowerSpec("RTX A4000", 140, 170, 100, "Ampere"),
            new GPUPowerSpec("L40S", 350, 400, 250, "Ada Lovelace"),
            new GPUPowerSpec("L40", 300, 350, 200, "Ada Lovelace"),
            new GPUPowerSpec("L4", 72, 85, 50, "Ada Lovelace"),
            new GPUPowerSpec("H100 SXM", 700, 800, 500, "Hopper"),
            new GPUPowerSpec("H100 PCIe", 350, 400, 250, "Hopper"),
            new GPUPowerSpec("H100 NVL", 400, 460, 300, "Hopper"),
            new GPUPowerSpec("H200", 700, 800, 500, "Hopper"),
            new GPUPowerSpec("A100 SXM", 400, 460, 300, "Ampere"),
            new GPUPowerSpec("A100 PCIe", 300, 350, 200, "Ampere"),
            new GPUPowerSpec("A40", 300, 350, 200, "Ampere"),
            new GPUPowerSpec("A30", 165, 195, 115, "Ampere"),
            new GPUPowerSpec("A16", 250, 290, 175, "Ampere"),
            new GPUPowerSpec("A10", 150, 175, 100, "Ampere"),
            new GPUPowerSpec("A2", 60, 70, 40, "Ampere"),

            // =====================================================
            // GTX 10 Series (Pascal) - Desktop
            // =====================================================
            new GPUPowerSpec("GTX 1080 Ti", 250, 300, 200, "Pascal"),
            new GPUPowerSpec("GTX 1080", 180, 215, 140, "Pascal"),
            new GPUPowerSpec("GTX 1070 Ti", 180, 215, 140, "Pascal"),
            new GPUPowerSpec("GTX 1070", 150, 180, 115, "Pascal"),
            new GPUPowerSpec("GTX 1060 6GB", 120, 145, 90, "Pascal"),
            new GPUPowerSpec("GTX 1060 3GB", 120, 145, 90, "Pascal"),
            new GPUPowerSpec("GTX 1060", 120, 145, 90, "Pascal"),
            new GPUPowerSpec("GTX 1050 Ti", 75, 90, 55, "Pascal"),
            new GPUPowerSpec("GTX 1050", 75, 90, 55, "Pascal"),

            // TITAN
            new GPUPowerSpec("TITAN RTX", 280, 330, 210, "Turing"),
            new GPUPowerSpec("TITAN V", 250, 300, 200, "Volta"),
            new GPUPowerSpec("TITAN Xp", 250, 300, 200, "Pascal"),
        };

        /// <summary>
        ///     All known GPU power specifications in the database.
        /// </summary>
        public static IReadOnlyList<GPUPowerSpec> AllSpecs => KnownSpecs;

        /// <summary>
        ///     Tries to find a matching power spec for the given GPU name.
        ///     Matches are case-insensitive and use substring matching.
        ///     More specific names (longer patterns) are preferred.
        /// </summary>
        /// <param name="gpuFullName">The GPU full name from NVAPI (e.g. "NVIDIA GeForce RTX 4090").</param>
        /// <param name="spec">The matched power specification, or null.</param>
        /// <returns>True when a match is found.</returns>
        public static bool TryGetSpec(string gpuFullName, out GPUPowerSpec? spec)
        {
            spec = null;

            if (string.IsNullOrWhiteSpace(gpuFullName))
            {
                return false;
            }

            // Sort by name pattern length descending so "RTX 4080 SUPER" matches before "RTX 4080"
            var bestMatch = KnownSpecs
                .Where(s => gpuFullName.IndexOf(s.NamePattern, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderByDescending(s => s.NamePattern.Length)
                .FirstOrDefault();

            if (bestMatch != null)
            {
                spec = bestMatch;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Gets the default TDP in watts for a GPU name, or null if unknown.
        /// </summary>
        /// <param name="gpuFullName">The GPU full name from NVAPI.</param>
        /// <returns>Default TDP in watts, or null if the GPU is not in the database.</returns>
        public static double? GetDefaultTDP(string gpuFullName)
        {
            return TryGetSpec(gpuFullName, out var spec) ? spec?.DefaultTDPWatts : null;
        }

        /// <summary>
        ///     Registers a custom GPU power specification at runtime.
        ///     This allows users to add support for GPUs not yet in the built-in database.
        /// </summary>
        /// <param name="namePattern">Name pattern to match (case-insensitive substring of GPU FullName).</param>
        /// <param name="defaultTdpWatts">Default board TDP in watts.</param>
        /// <param name="maxTdpWatts">Maximum board power limit in watts.</param>
        /// <param name="minTdpWatts">Minimum board power limit in watts.</param>
        /// <param name="architecture">Architecture name (e.g. "Ada Lovelace").</param>
        public static void RegisterSpec(
            string namePattern,
            double defaultTdpWatts,
            double maxTdpWatts,
            double minTdpWatts,
            string architecture = "Unknown")
        {
            if (string.IsNullOrWhiteSpace(namePattern))
            {
                throw new ArgumentException("Name pattern cannot be null or empty.", nameof(namePattern));
            }

            if (defaultTdpWatts <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(defaultTdpWatts));
            }

            // Insert at the beginning so user-registered specs take priority
            KnownSpecs.Insert(0, new GPUPowerSpec(namePattern, defaultTdpWatts, maxTdpWatts, minTdpWatts, architecture));
        }
    }
}
