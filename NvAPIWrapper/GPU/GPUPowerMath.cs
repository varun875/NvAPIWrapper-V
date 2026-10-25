using System;

namespace NvAPIWrapper.GPU
{
    internal static class GPUPowerMath
    {
        internal static void ValidateReference(double watts, string parameterName)
        {
            if (!double.IsFinite(watts) || watts <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Power references must be finite and greater than zero.");
            }
        }

        internal static bool TryEstimate(float? percentage, double referenceWatts, out double watts)
        {
            ValidateReference(referenceWatts, nameof(referenceWatts));
            watts = 0;
            if (!percentage.HasValue || !float.IsFinite(percentage.Value) || percentage.Value < 0)
            {
                return false;
            }

            // The reference describes 100% of this domain. Never multiply it again
            // by an active slider percentage; doing so double-scales the reading.
            var result = referenceWatts * (percentage.Value / 100d);
            if (!double.IsFinite(result))
            {
                return false;
            }

            watts = result;
            return true;
        }

        internal static GPUPowerValue Estimate(float? percentage, GPUPowerValue reference)
        {
            return reference.Watts.HasValue && TryEstimate(percentage, reference.Watts.Value, out var watts)
                ? new GPUPowerValue(watts, reference.Source, true)
                : GPUPowerValue.Unavailable;
        }
    }
}
