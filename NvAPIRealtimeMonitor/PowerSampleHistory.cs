using System;
using System.Collections.Generic;
using NvAPIWrapper.GPU;

namespace NvAPIRealtimeMonitor
{
    internal sealed class PowerSampleHistory
    {
        private readonly int _capacity;
        private readonly Queue<double?> _samples = new();
        private double _total;
        private GPUPowerDataSource _source;
        private bool _estimated;

        internal PowerSampleHistory(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            _capacity = capacity;
        }

        internal long ValidSampleCount { get; private set; }
        internal double? PeakWatts { get; private set; }
        internal double? AverageWatts => ValidSampleCount > 0 ? _total / ValidSampleCount : null;
        internal bool IsEstimated => _estimated;
        internal GPUPowerDataSource Source => _source;
        internal IReadOnlyList<double?> Samples => _samples.ToArray();

        internal void Add(GPUPowerValue? reading)
        {
            if (reading?.IsAvailable == true)
            {
                // Do not combine direct and estimated watts, or estimates based on
                // different sources, into an apparently homogeneous statistic.
                if (ValidSampleCount > 0 && (_source != reading.Source || _estimated != reading.IsEstimated)) Reset();
                _source = reading.Source;
                _estimated = reading.IsEstimated;
                var watts = reading.Watts!.Value;
                _total += watts;
                ValidSampleCount++;
                PeakWatts = !PeakWatts.HasValue || watts > PeakWatts.Value ? watts : PeakWatts;
            }

            // Preserve a visible gap for an unavailable poll rather than a false zero.
            _samples.Enqueue(reading?.Watts);
            while (_samples.Count > _capacity) _samples.Dequeue();
        }

        internal void Reset()
        {
            _samples.Clear();
            _total = 0;
            ValidSampleCount = 0;
            PeakWatts = null;
            _source = GPUPowerDataSource.Unavailable;
            _estimated = false;
        }
    }
}
