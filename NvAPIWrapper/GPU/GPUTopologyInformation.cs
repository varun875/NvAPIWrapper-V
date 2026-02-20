#nullable enable

using System.Linq;

namespace NvAPIWrapper.GPU
{
    /// <summary>
    ///     Provides information about GPU topology, NVLink interconnects, and multi-GPU capabilities
    /// </summary>
    public class GPUTopologyInformation
    {
        /// <summary>
        ///     Represents an NVLink connection between two GPUs
        /// </summary>
        public class NVLinkConnection
        {
            /// <summary>
            ///     GPU ID of the source GPU
            /// </summary>
            public uint SourceGPUId { get; set; }

            /// <summary>
            ///     GPU ID of the destination GPU
            /// </summary>
            public uint DestinationGPUId { get; set; }

            /// <summary>
            ///     Link width in bits (50GB/s per link on NVLink 3.0)
            /// </summary>
            public uint LinkWidthBits { get; set; }

            /// <summary>
            ///     NVLink version (e.g., 3.0, 4.0)
            /// </summary>
            public string? LinkVersion { get; set; }

            /// <summary>
            ///     Maximum bandwidth in GB/s for this link
            /// </summary>
            public double MaxBandwidthGBps { get; set; }

            /// <summary>
            ///     Whether this link is currently active
            /// </summary>
            public bool IsActive { get; set; }

            /// <summary>
            ///     Get human-readable string representation
            /// </summary>
            public override string ToString() =>
                $"NVLink {LinkVersion}: GPU{SourceGPUId} <-> GPU{DestinationGPUId} ({MaxBandwidthGBps:F1} GB/s)";
        }

        /// <summary>
        ///     Information about GPU connection topology
        /// </summary>
        public class GPUEndpoint
        {
            /// <summary>
            ///     GPU identifier
            /// </summary>
            public uint GPUId { get; set; }

            /// <summary>
            ///     Number of times this GPU appears in the topology
            /// </summary>
            public uint Multiplicity { get; set; }

            /// <summary>
            ///     Whether this GPU supports NVLink
            /// </summary>
            public bool SupportsNVLink { get; set; }

            /// <summary>
            ///     NVLink-connected GPUs
            /// </summary>
            public uint[]? ConnectedGPUIds { get; set; }
        }

        /// <summary>
        ///     Represents GPU-to-GPU connectivity information
        /// </summary>
        public class GPUPeerAccess
        {
            /// <summary>
            ///     Source GPU ID
            /// </summary>
            public uint SourceGPUId { get; set; }

            /// <summary>
            ///     Target GPU ID
            /// </summary>
            public uint TargetGPUId { get; set; }

            /// <summary>
            ///     Can source GPU directly access target GPU memory via P2P
            /// </summary>
            public bool IsPeerAccessSupported { get; set; }

            /// <summary>
            ///     Can source GPU directly access target GPU via NVLink
            /// </summary>
            public bool IsNVLinkSupported { get; set; }

            /// <summary>
            ///     Direct PCIe P2P is available (without NVLink)
            /// </summary>
            public bool IsPCIeP2PSupported { get; set; }

            /// <summary>
            ///     Maximum latency for direct access (in microseconds)
            /// </summary>
            public uint LatencyMicroseconds { get; set; }

            /// <summary>
            ///     Get topology status description
            /// </summary>
            public override string ToString()
            {
                if (IsNVLinkSupported)
                {
                    return $"GPU{SourceGPUId} -> GPU{TargetGPUId}: NVLink ({LatencyMicroseconds}us)";
                }

                if (IsPeerAccessSupported)
                {
                    return $"GPU{SourceGPUId} -> GPU{TargetGPUId}: P2P/PCIe ({LatencyMicroseconds}us)";
                }

                return $"GPU{SourceGPUId} -> GPU{TargetGPUId}: No direct access";
            }
        }

        /// <summary>
        ///     Initialize topology information for a GPU
        /// </summary>
        public GPUTopologyInformation(PhysicalGPU gpu)
        {
            PhysicalGPU = gpu;
        }

        /// <summary>
        ///     Reference to the physical GPU
        /// </summary>
        public PhysicalGPU PhysicalGPU { get; }

        /// <summary>
        ///     Whether this GPU supports NVLink for multi-GPU communication
        /// </summary>
        public bool SupportsNVLink { get; set; }

        /// <summary>
        ///     Number of active NVLink connections from this GPU
        /// </summary>
        public int NVLinkCount { get; set; }

        /// <summary>
        ///     Array of connected GPU IDs via NVLink
        /// </summary>
        public uint[]? ConnectedGPUIds { get; set; }

        /// <summary>
        ///     Detailed NVLink connection information
        /// </summary>
        public NVLinkConnection[]? NVLinkConnections { get; set; }

        /// <summary>
        ///     Whether this GPU topology includes NVSwitch (Ampere+ multi-GPU fabric)
        /// </summary>
        public bool SupportsNVSwitch { get; set; }

        /// <summary>
        ///     Information about GPU-to-GPU peer access capabilities
        /// </summary>
        public GPUPeerAccess[]? PeerAccessMatrix { get; set; }

        /// <summary>
        ///     NVLink version supported (e.g., "3.0", "4.0")
        /// </summary>
        public string? NVLinkVersion { get; set; }

        /// <summary>
        ///     Maximum aggregate NVLink bandwidth from this GPU in GB/s
        /// </summary>
        public double MaxAggregateNVLinkBandwidthGBps { get; set; }

        /// <summary>
        ///     Check if given GPU is directly connected via NVLink
        /// </summary>
        public bool IsConnectedToGPU(uint gpuId) =>
            ConnectedGPUIds?.Contains(gpuId) ?? false;

        /// <summary>
        ///     Get peer access information for a specific GPU pair
        /// </summary>
        public GPUPeerAccess? GetPeerAccessInfo(uint targetGpuId) =>
            PeerAccessMatrix?.FirstOrDefault(p => p.TargetGPUId == targetGpuId);

        /// <summary>
        ///     Get topology summary
        /// </summary>
        public override string ToString()
        {
            if (SupportsNVLink && SupportsNVSwitch)
            {
                return $"GPU{PhysicalGPU.GPUId}: NVLink ({NVLinkCount} links) + NVSwitch";
            }

            if (SupportsNVLink)
            {
                return $"GPU{PhysicalGPU.GPUId}: NVLink ({NVLinkCount} links)";
            }

            if (SupportsNVSwitch)
            {
                return $"GPU{PhysicalGPU.GPUId}: NVSwitch fabric";
            }

            return $"GPU{PhysicalGPU.GPUId}: No NVLink/NVSwitch";
        }
    }
}
