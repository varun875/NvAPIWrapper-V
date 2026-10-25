namespace NvAPIWrapper.GPU
{
    /// <summary>Controls optional watt-estimation fallbacks for one power snapshot.</summary>
    public sealed class GPUPowerTelemetryOptions
    {
        internal static GPUPowerTelemetryOptions Default { get; } = new GPUPowerTelemetryOptions();

        /// <summary>Creates power reporting options. Direct NVML readings always have priority.</summary>
        /// <param name="boardPowerReferenceInWatts">Explicit reference for 100% board-domain usage, if known.</param>
        /// <param name="gpuPowerReferenceInWatts">Explicit reference for 100% GPU-domain usage, if known.</param>
        /// <param name="allowReferenceDatabaseEstimates">Allows unverified model-level TDP estimates; disabled by default.</param>
        /// <param name="useNVML">Enables the optional driver-provided NVML backend.</param>
        public GPUPowerTelemetryOptions(double? boardPowerReferenceInWatts = null,
            double? gpuPowerReferenceInWatts = null, bool allowReferenceDatabaseEstimates = false, bool useNVML = true)
        {
            if (boardPowerReferenceInWatts.HasValue)
            {
                GPUPowerMath.ValidateReference(boardPowerReferenceInWatts.Value, nameof(boardPowerReferenceInWatts));
            }

            if (gpuPowerReferenceInWatts.HasValue)
            {
                GPUPowerMath.ValidateReference(gpuPowerReferenceInWatts.Value, nameof(gpuPowerReferenceInWatts));
            }

            BoardPowerReferenceInWatts = boardPowerReferenceInWatts;
            GPUPowerReferenceInWatts = gpuPowerReferenceInWatts;
            AllowReferenceDatabaseEstimates = allowReferenceDatabaseEstimates;
            UseNVML = useNVML;
        }

        /// <summary>Gets the explicitly configured 100% board-domain reference.</summary>
        public double? BoardPowerReferenceInWatts { get; }

        /// <summary>Gets the explicitly configured 100% GPU-domain reference, which may differ from board TGP.</summary>
        public double? GPUPowerReferenceInWatts { get; }

        /// <summary>Gets whether legacy model-level reference estimates are allowed.</summary>
        public bool AllowReferenceDatabaseEstimates { get; }

        /// <summary>Gets whether optional NVML queries are enabled.</summary>
        public bool UseNVML { get; }
    }
}
