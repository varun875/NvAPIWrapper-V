using System;
using NvAPIWrapper.Native.GPU;

namespace NvAPIWrapper.GPU
{
    /// <summary>
    ///     Provides GPU family and classification utilities for modern architectures
    /// </summary>
    public static class GPUFamilyClassifier
    {
        /// <summary>
        ///     Enumeration of known GPU families/architectures
        /// </summary>
        public enum GPUFamily
        {
            /// <summary>
            ///     Unknown architecture
            /// </summary>
            Unknown,

            /// <summary>
            ///     Kepler (GK, GTX 750/750Ti)
            /// </summary>
            Kepler,

            /// <summary>
            ///     Maxwell (GM, GTX 950/960/970/980)
            /// </summary>
            Maxwell,

            /// <summary>
            ///     Pascal (GP, GTX 1030/1050/1060/1070/1080)
            /// </summary>
            Pascal,

            /// <summary>
            ///     Volta (GV, Titan V)
            /// </summary>
            Volta,

            /// <summary>
            ///     Turing (TU, RTX 2060/2070/2080/2090)
            /// </summary>
            Turing,

            /// <summary>
            ///     Ampere (GA, RTX 3060/3070/3080/3090, RTX 6000)
            /// </summary>
            Ampere,

            /// <summary>
            ///     Ada Lovelace (AD, RTX 4060/4070/4080/4090, RTX 6000 Ada, L40)
            /// </summary>
            Ada,

            /// <summary>
            ///     Blackwell (BL, RTX 5000/5090, H100 successor)
            /// </summary>
            Blackwell,

            /// <summary>
            ///     Hopper (H100, H200) - Data center
            /// </summary>
            Hopper,

            /// <summary>
            ///     Orin (ARM-based, edge/mobile)
            /// </summary>
            Orin
        }

        /// <summary>
        ///     GPU market tier classification
        /// </summary>
        public enum GPUTier
        {
            /// <summary>
            ///     Embedded/Edge (Orin, TX2)
            /// </summary>
            Embedded,

            /// <summary>
            ///     Budget consumer (GTX 1030, RTX 3050)
            /// </summary>
            Budget,

            /// <summary>
            ///     Mainstream consumer (GTX 1060, RTX 3060/4060)
            /// </summary>
            Mainstream,

            /// <summary>
            ///     High-end consumer (GTX 1080, RTX 3080/4080)
            /// </summary>
            HighEnd,

            /// <summary>
            ///     Ultra enthusiast (Titan, RTX 4090)
            /// </summary>
            Ultra,

            /// <summary>
            ///     Professional workstation (RTX 6000 series)
            /// </summary>
            Professional,

            /// <summary>
            ///     Data center (Tesla, H100, H200)
            /// </summary>
            DataCenter
        }

        /// <summary>
        ///     Detects GPU family based on codename/shortname
        /// </summary>
        /// <param name="shortName">GPU codename from architecture info</param>
        /// <returns>Detected GPU family</returns>
        public static GPUFamily DetectFamily(string shortName)
        {
            if (string.IsNullOrWhiteSpace(shortName))
                return GPUFamily.Unknown;

            var name = shortName.ToUpperInvariant();

            // Codenames by architecture
            if (name.StartsWith("GK"))
                return GPUFamily.Kepler;
            if (name.StartsWith("GM"))
                return GPUFamily.Maxwell;
            if (name.StartsWith("GP"))
                return GPUFamily.Pascal;
            if (name.StartsWith("GV"))
                return GPUFamily.Volta;
            if (name.StartsWith("TU"))
                return GPUFamily.Turing;
            if (name.StartsWith("GA"))
                return GPUFamily.Ampere;
            if (name.StartsWith("AD"))
                return GPUFamily.Ada;
            if (name.StartsWith("BL"))
                return GPUFamily.Blackwell;
            if (name.StartsWith("GH") || name == "H100" || name == "H200")
                return GPUFamily.Hopper;
            if (name.StartsWith("ORIN") || name == "ORIN")
                return GPUFamily.Orin;

            return GPUFamily.Unknown;
        }

        /// <summary>
        ///     Determines if GPU is suitable for CUDA compute workloads
        /// </summary>
        public static bool IsComputeCapable(GPUFamily family)
        {
            return family != GPUFamily.Unknown;
        }

        /// <summary>
        ///     Determines if GPU supports NVLink (multi-GPU interconnect)
        /// </summary>
        public static bool SupportsNVLink(GPUFamily family)
        {
            return family == GPUFamily.Volta ||
                   family == GPUFamily.Ampere ||
                   family == GPUFamily.Ada ||
                   family == GPUFamily.Hopper;
        }

        /// <summary>
        ///     Determines if GPU uses GDDR6 or newer memory
        /// </summary>
        public static bool UsesModernMemory(GPUMemoryType memoryType)
        {
            return memoryType == GPUMemoryType.GDDR6 ||
                   memoryType == GPUMemoryType.GDDR6X ||
                   memoryType == GPUMemoryType.GDDR7 ||
                   memoryType == GPUMemoryType.HBM2 ||
                   memoryType == GPUMemoryType.HBM2e ||
                   memoryType == GPUMemoryType.HBM3 ||
                   memoryType == GPUMemoryType.HBM3e;
        }

        /// <summary>
        ///     Determines if GPU supports PCIe 4.0 or newer
        /// </summary>
        public static bool SupportsModernPCIe(PCIeGeneration generation)
        {
            return generation == PCIeGeneration.PCIe4 ||
                   generation == PCIeGeneration.PCIe5;
        }

        /// <summary>
        ///     Gets human-readable description of GPU family
        /// </summary>
        public static string GetFamilyDescription(GPUFamily family) => family switch
        {
            GPUFamily.Kepler => "Kepler (2012-2014)",
            GPUFamily.Maxwell => "Maxwell (2014-2016)",
            GPUFamily.Pascal => "Pascal (2016-2017)",
            GPUFamily.Volta => "Volta (2017-2018)",
            GPUFamily.Turing => "Turing (2018-2020)",
            GPUFamily.Ampere => "Ampere (2020-2021)",
            GPUFamily.Ada => "Ada Lovelace (2022-2023)",
            GPUFamily.Blackwell => "Blackwell (2025+)",
            GPUFamily.Hopper => "Hopper (2022-2023, Data Center)",
            GPUFamily.Orin => "Orin (Edge/Mobile)",
            _ => "Unknown"
        };

        /// <summary>
        ///     Gets human-readable description of GPU use case tier
        /// </summary>
        public static string GetTierDescription(GPUTier tier) => tier switch
        {
            GPUTier.Embedded => "Embedded/Edge Computing",
            GPUTier.Budget => "Budget Consumer",
            GPUTier.Mainstream => "Mainstream Consumer",
            GPUTier.HighEnd => "High-End Consumer",
            GPUTier.Ultra => "Ultra Enthusiast",
            GPUTier.Professional => "Professional Workstation",
            GPUTier.DataCenter => "Data Center/Compute",
            _ => "Unknown"
        };
    }
}
