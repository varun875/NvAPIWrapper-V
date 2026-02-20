#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using NvAPIWrapper;
using NvAPIWrapper.GPU;
using NvAPIWrapper.Native.GPU;

namespace NvAPIRealtimeMonitor
{
    internal static class Program
    {
        // ── Configuration ──────────────────────────────────────
        private const int RefreshIntervalMs = 500;    // Update every 500ms
        private const int HistoryLength = 60;          // 30 seconds of history at 500ms intervals
        private const int BarWidth = 30;               // Width of progress bars

        // ── ANSI Colors ────────────────────────────────────────
        private const string Reset = "\x1b[0m";
        private const string Bold = "\x1b[1m";
        private const string Dim = "\x1b[2m";
        private const string Red = "\x1b[91m";
        private const string Green = "\x1b[92m";
        private const string Yellow = "\x1b[93m";
        private const string Blue = "\x1b[94m";
        private const string Magenta = "\x1b[95m";
        private const string Cyan = "\x1b[96m";
        private const string White = "\x1b[97m";
        private const string DarkGray = "\x1b[90m";
        private const string BgRed = "\x1b[41m";
        private const string BgGreen = "\x1b[42m";
        private const string BgYellow = "\x1b[43m";
        private const string BgBlue = "\x1b[44m";
        private const string BgMagenta = "\x1b[45m";
        private const string BgCyan = "\x1b[46m";

        // ── Sparkline Characters ───────────────────────────────
        private static readonly char[] SparkChars = { '▁', '▂', '▃', '▄', '▅', '▆', '▇', '█' };

        // ── History Buffers ────────────────────────────────────
        private static readonly Queue<double> PowerHistory = new();
        private static readonly Queue<double> GpuUsageHistory = new();
        private static readonly Queue<double> TempHistory = new();
        private static readonly Queue<double> ClockHistory = new();
        private static readonly Queue<double> MemUsageHistory = new();
        private static readonly Queue<double> FanSpeedHistory = new();

        // ── Peak/Avg Trackers ──────────────────────────────────
        private static double _peakPowerW;
        private static double _peakTempC;
        private static double _peakClockMHz;
        private static double _peakGpuUsage;
        private static long _sampleCount;
        private static double _totalPowerW;
        private static double _totalTempC;

        private static int Main()
        {
            // Enable ANSI / Virtual Terminal Processing on Windows
            EnableAnsiSupport();

            try
            {
                NVIDIA.Initialize();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to initialize NVAPI: {ex.Message}");
                Console.Error.WriteLine("Make sure you have an NVIDIA GPU and latest drivers installed.");
                return 1;
            }

            var gpus = PhysicalGPU.GetPhysicalGPUs();

            if (gpus.Length == 0)
            {
                Console.Error.WriteLine("No NVIDIA GPUs detected.");
                return 1;
            }

            // Auto-select GPU (if multiple, let user pick)
            PhysicalGPU gpu;

            if (gpus.Length == 1)
            {
                gpu = gpus[0];
            }
            else
            {
                Console.Clear();
                Console.WriteLine($"{Bold}{Cyan}╔══════════════════════════════════════════╗{Reset}");
                Console.WriteLine($"{Bold}{Cyan}║  NvAPIWrapper Realtime GPU Monitor       ║{Reset}");
                Console.WriteLine($"{Bold}{Cyan}╚══════════════════════════════════════════╝{Reset}");
                Console.WriteLine();

                for (var i = 0; i < gpus.Length; i++)
                {
                    Console.WriteLine($"  {Yellow}[{i + 1}]{Reset} {gpus[i].FullName}");
                }

                Console.Write($"\n  {White}Select GPU [1-{gpus.Length}]: {Reset}");
                var key = Console.ReadKey(true);
                var index = key.KeyChar - '1';

                if (index < 0 || index >= gpus.Length) index = 0;
                gpu = gpus[index];
            }

            // Gather static info once
            var gpuName = gpu.FullName;
            var memInfo = gpu.MemoryInformation;
            var archInfo = gpu.ArchitectInformation;
            var busInfo = gpu.BusInformation;

            string vramTotal;
            try
            {
                vramTotal = $"{memInfo.AvailableDedicatedVideoMemoryInkB / 1024} MB";
            }
            catch
            {
                vramTotal = "N/A";
            }

            string memType;
            try
            {
                memType = memInfo.RAMType.ToString();
            }
            catch
            {
                memType = "Unknown";
            }

            string archName;
            try
            {
                archName = archInfo.ShortName;
            }
            catch
            {
                archName = "Unknown";
            }

            // TDP info
            GPUPowerSpecDatabase.TryGetSpec(gpuName, out var powerSpec);

            string tdpInfo;
            if (powerSpec != null)
            {
                tdpInfo = $"{powerSpec.DefaultTDPWatts}W (max {powerSpec.MaxTDPWatts}W) [{powerSpec.Architecture}]";
            }
            else
            {
                tdpInfo = "Unknown (not in database)";
            }

            // Hide cursor & prepare
            Console.CursorVisible = false;
            Console.Clear();

            var stopwatch = Stopwatch.StartNew();
            var running = true;

            // Monitoring loop
            while (running)
            {
                if (Console.KeyAvailable)
                {
                    var k = Console.ReadKey(true);

                    if (k.Key == ConsoleKey.Q || k.Key == ConsoleKey.Escape)
                    {
                        running = false;
                        continue;
                    }

                    if (k.Key == ConsoleKey.R)
                    {
                        // Reset stats
                        _peakPowerW = 0;
                        _peakTempC = 0;
                        _peakClockMHz = 0;
                        _peakGpuUsage = 0;
                        _sampleCount = 0;
                        _totalPowerW = 0;
                        _totalTempC = 0;
                        PowerHistory.Clear();
                        GpuUsageHistory.Clear();
                        TempHistory.Clear();
                        ClockHistory.Clear();
                        MemUsageHistory.Clear();
                        FanSpeedHistory.Clear();
                        stopwatch.Restart();
                    }
                }

                // ── Collect Telemetry ──────────────────────────
                double boardPowerW = 0, gpuPowerW = 0;
                double boardPowerPct = 0;
                double powerLimitW = 0;
                double gpuUsagePct = 0, memUsagePct = 0, videoUsagePct = 0;
                double gpuTempC = 0;
                double graphicsClockMHz = 0, memClockMHz = 0;
                double fanSpeedPct = 0;
                double vramUsedMB = 0, vramTotalMB = 0;
                string perfState = "N/A";
                string throttleStatus = "None";
                bool isPowerThrottled = false, isThermalThrottled = false;
                bool telemetryAvailable = false;

                // Power telemetry
                if (gpu.TryGetPowerTelemetrySnapshot(out var snapshot) && snapshot != null)
                {
                    telemetryAvailable = true;
                    boardPowerPct = snapshot.BoardPowerUsageInPercent ?? 0;
                    boardPowerW = snapshot.BoardPowerDrawInWatts ?? 0;
                    gpuPowerW = snapshot.GPUPowerDrawInWatts ?? 0;
                    powerLimitW = snapshot.CurrentPowerLimitInWatts ?? 0;
                    isPowerThrottled = snapshot.IsPowerLimitActive;
                    isThermalThrottled = snapshot.IsThermalLimitActive;
                    throttleStatus = snapshot.ThrottleStatus;
                    perfState = snapshot.CurrentPerformanceState?.ToString() ?? "N/A";
                }

                // GPU Utilization
                try
                {
                    if (gpu.UsageInformation.TryGetUtilizationDomainsStatus(out var domains))
                    {
                        var gpuDomain = domains.FirstOrDefault(d => d.Domain == UtilizationDomain.GPU);
                        var fbDomain = domains.FirstOrDefault(d => d.Domain == UtilizationDomain.FrameBuffer);
                        var vidDomain = domains.FirstOrDefault(d => d.Domain == UtilizationDomain.VideoEngine);

                        if (gpuDomain != null) gpuUsagePct = gpuDomain.Percentage;
                        if (fbDomain != null) memUsagePct = fbDomain.Percentage;
                        if (vidDomain != null) videoUsagePct = vidDomain.Percentage;
                    }
                }
                catch { }

                // Temperatures
                try
                {
                    if (gpu.ThermalInformation.TryGetThermalSensors(out var sensors) && sensors.Length > 0)
                    {
                        gpuTempC = sensors[0].CurrentTemperature;
                    }
                }
                catch { }

                // Clocks
                try
                {
                    var clocks = gpu.CurrentClockFrequencies;

                    if (clocks.GraphicsClock.IsPresent)
                    {
                        graphicsClockMHz = clocks.GraphicsClock.Frequency / 1000.0;
                    }

                    if (clocks.MemoryClock.IsPresent)
                    {
                        memClockMHz = clocks.MemoryClock.Frequency / 1000.0;
                    }
                }
                catch { }

                // Fan speed
                try
                {
                    fanSpeedPct = gpu.CoolerInformation.CurrentFanSpeedLevel;
                }
                catch { }

                // VRAM usage
                try
                {
                    vramTotalMB = memInfo.AvailableDedicatedVideoMemoryInkB / 1024.0;
                    vramUsedMB = (memInfo.AvailableDedicatedVideoMemoryInkB -
                                  memInfo.CurrentAvailableDedicatedVideoMemoryInkB) / 1024.0;
                }
                catch { }

                // ── Update Histories ───────────────────────────
                PushHistory(PowerHistory, boardPowerW);
                PushHistory(GpuUsageHistory, gpuUsagePct);
                PushHistory(TempHistory, gpuTempC);
                PushHistory(ClockHistory, graphicsClockMHz);
                PushHistory(MemUsageHistory, memUsagePct);
                PushHistory(FanSpeedHistory, fanSpeedPct);

                // ── Update Peaks ───────────────────────────────
                _sampleCount++;
                _totalPowerW += boardPowerW;
                _totalTempC += gpuTempC;

                if (boardPowerW > _peakPowerW) _peakPowerW = boardPowerW;
                if (gpuTempC > _peakTempC) _peakTempC = gpuTempC;
                if (graphicsClockMHz > _peakClockMHz) _peakClockMHz = graphicsClockMHz;
                if (gpuUsagePct > _peakGpuUsage) _peakGpuUsage = gpuUsagePct;

                // ── Render ─────────────────────────────────────
                var sb = new StringBuilder(4096);
                sb.Append("\x1b[H"); // Move cursor to top-left (no flicker)

                var elapsed = stopwatch.Elapsed;
                var avgPowerW = _sampleCount > 0 ? _totalPowerW / _sampleCount : 0;
                var avgTempC = _sampleCount > 0 ? _totalTempC / _sampleCount : 0;

                // ── Header ─────────────────────────────────────
                sb.AppendLine($"{Bold}{Cyan}╔══════════════════════════════════════════════════════════════════════════════╗{Reset}");
                sb.AppendLine($"{Bold}{Cyan}║{Reset}  {Bold}{White}⚡ NvAPIWrapper Realtime GPU Power Monitor{Reset}                                  {Bold}{Cyan}║{Reset}");
                sb.AppendLine($"{Bold}{Cyan}╠══════════════════════════════════════════════════════════════════════════════╣{Reset}");
                sb.AppendLine($"{Bold}{Cyan}║{Reset}  {DarkGray}GPU:{Reset} {Bold}{White}{Truncate(gpuName, 40),-40}{Reset}  {DarkGray}Arch:{Reset} {Yellow}{archName,-12}{Reset}     {Bold}{Cyan}║{Reset}");
                sb.AppendLine($"{Bold}{Cyan}║{Reset}  {DarkGray}VRAM:{Reset} {Green}{vramTotal,-10}{Reset} {DarkGray}Type:{Reset} {Green}{memType,-10}{Reset} {DarkGray}TDP:{Reset} {Magenta}{Truncate(tdpInfo, 30),-30}{Reset}  {Bold}{Cyan}║{Reset}");
                sb.AppendLine($"{Bold}{Cyan}║{Reset}  {DarkGray}Uptime:{Reset} {Blue}{elapsed:hh\\:mm\\:ss}{Reset}  {DarkGray}Samples:{Reset} {Blue}{_sampleCount,-8}{Reset} {DarkGray}State:{Reset} {Yellow}{perfState,-24}{Reset}  {Bold}{Cyan}║{Reset}");
                sb.AppendLine($"{Bold}{Cyan}╠══════════════════════════════════════════════════════════════════════════════╣{Reset}");

                // ── Power Section ──────────────────────────────
                sb.AppendLine($"{Bold}{Cyan}║{Reset}  {Bold}{Yellow}⚡ POWER{Reset}                                                                   {Bold}{Cyan}║{Reset}");

                if (powerSpec != null && telemetryAvailable)
                {
                    var powerPct = powerLimitW > 0 ? (boardPowerW / powerLimitW * 100) : boardPowerPct;
                    var powerColor = powerPct > 95 ? Red : powerPct > 80 ? Yellow : Green;
                    sb.AppendLine($"{Bold}{Cyan}║{Reset}  {DarkGray}Board Power:{Reset}  {powerColor}{Bold}{boardPowerW,7:F1} W{Reset} / {powerLimitW:F0}W  {RenderBar(powerPct, BarWidth, powerColor)}  {Bold}{Cyan}║{Reset}");
                    sb.AppendLine($"{Bold}{Cyan}║{Reset}  {DarkGray}GPU Power:{Reset}    {Cyan}{gpuPowerW,7:F1} W{Reset}                                                    {Bold}{Cyan}║{Reset}");
                    sb.AppendLine($"{Bold}{Cyan}║{Reset}  {DarkGray}Peak:{Reset} {Red}{_peakPowerW,6:F1}W{Reset}  {DarkGray}Avg:{Reset} {Blue}{avgPowerW,6:F1}W{Reset}  {DarkGray}Limit:{Reset} {Magenta}{powerLimitW:F0}W{Reset}                              {Bold}{Cyan}║{Reset}");
                }
                else
                {
                    var powerColor = boardPowerPct > 95 ? Red : boardPowerPct > 80 ? Yellow : Green;
                    sb.AppendLine($"{Bold}{Cyan}║{Reset}  {DarkGray}Board Power:{Reset}  {powerColor}{Bold}{boardPowerPct,6:F1}%{Reset}   {RenderBar(boardPowerPct, BarWidth, powerColor)}                       {Bold}{Cyan}║{Reset}");
                    sb.AppendLine($"{Bold}{Cyan}║{Reset}  {DarkGray}(TDP unknown — use GPUPowerSpecDatabase.RegisterSpec() for watts){Reset}             {Bold}{Cyan}║{Reset}");
                    sb.AppendLine($"{Bold}{Cyan}║{Reset}                                                                              {Bold}{Cyan}║{Reset}");
                }

                sb.AppendLine($"{Bold}{Cyan}║{Reset}  {DarkGray}Sparkline (30s):{Reset} {RenderSparkline(PowerHistory, Cyan)}                           {Bold}{Cyan}║{Reset}");
                sb.AppendLine($"{Bold}{Cyan}╠══════════════════════════════════════════════════════════════════════════════╣{Reset}");

                // ── GPU Usage Section ──────────────────────────
                sb.AppendLine($"{Bold}{Cyan}║{Reset}  {Bold}{Green}📊 UTILIZATION{Reset}                                                             {Bold}{Cyan}║{Reset}");
                var gpuColor = gpuUsagePct > 95 ? Red : gpuUsagePct > 75 ? Yellow : Green;
                sb.AppendLine($"{Bold}{Cyan}║{Reset}  {DarkGray}GPU Core:{Reset}     {gpuColor}{Bold}{gpuUsagePct,6:F1}%{Reset}   {RenderBar(gpuUsagePct, BarWidth, gpuColor)}  {DarkGray}Peak:{Reset} {Red}{_peakGpuUsage:F0}%{Reset}    {Bold}{Cyan}║{Reset}");
                sb.AppendLine($"{Bold}{Cyan}║{Reset}  {DarkGray}Memory Ctrl:{Reset}  {Blue}{memUsagePct,6:F1}%{Reset}   {RenderBar(memUsagePct, BarWidth, Blue)}              {Bold}{Cyan}║{Reset}");
                sb.AppendLine($"{Bold}{Cyan}║{Reset}  {DarkGray}Video Enc:{Reset}    {Magenta}{videoUsagePct,6:F1}%{Reset}   {RenderBar(videoUsagePct, BarWidth, Magenta)}              {Bold}{Cyan}║{Reset}");
                sb.AppendLine($"{Bold}{Cyan}║{Reset}  {DarkGray}Sparkline (30s):{Reset} {RenderSparkline(GpuUsageHistory, Green)}                           {Bold}{Cyan}║{Reset}");
                sb.AppendLine($"{Bold}{Cyan}╠══════════════════════════════════════════════════════════════════════════════╣{Reset}");

                // ── Temperature Section ────────────────────────
                sb.AppendLine($"{Bold}{Cyan}║{Reset}  {Bold}{Red}🌡️  THERMALS{Reset}                                                               {Bold}{Cyan}║{Reset}");
                var tempMax = powerSpec != null ? 90 : 100; // estimate
                var tempPct = tempMax > 0 ? gpuTempC / tempMax * 100 : 0;
                var tempColor = gpuTempC > 85 ? Red : gpuTempC > 70 ? Yellow : Green;
                sb.AppendLine($"{Bold}{Cyan}║{Reset}  {DarkGray}GPU Temp:{Reset}     {tempColor}{Bold}{gpuTempC,5:F0}°C{Reset}    {RenderBar(tempPct, BarWidth, tempColor)}  {DarkGray}Peak:{Reset} {Red}{_peakTempC:F0}°C{Reset}   {Bold}{Cyan}║{Reset}");
                sb.AppendLine($"{Bold}{Cyan}║{Reset}  {DarkGray}Avg Temp:{Reset}     {Blue}{avgTempC,5:F1}°C{Reset}    {DarkGray}Fan:{Reset} {Cyan}{fanSpeedPct:F0}%{Reset}  {RenderBar(fanSpeedPct, 15, Cyan)}                  {Bold}{Cyan}║{Reset}");
                sb.AppendLine($"{Bold}{Cyan}║{Reset}  {DarkGray}Sparkline (30s):{Reset} {RenderSparkline(TempHistory, Red)}                           {Bold}{Cyan}║{Reset}");
                sb.AppendLine($"{Bold}{Cyan}╠══════════════════════════════════════════════════════════════════════════════╣{Reset}");

                // ── Clocks Section ─────────────────────────────
                sb.AppendLine($"{Bold}{Cyan}║{Reset}  {Bold}{Magenta}🔧 CLOCKS & MEMORY{Reset}                                                        {Bold}{Cyan}║{Reset}");
                var clockMaxEstimate = _peakClockMHz > 0 ? _peakClockMHz * 1.1 : 3000;
                var clockPct = clockMaxEstimate > 0 ? graphicsClockMHz / clockMaxEstimate * 100 : 0;
                sb.AppendLine($"{Bold}{Cyan}║{Reset}  {DarkGray}Graphics:{Reset}     {Yellow}{Bold}{graphicsClockMHz,6:F0} MHz{Reset} {RenderBar(clockPct, BarWidth, Yellow)}  {DarkGray}Peak:{Reset} {Red}{_peakClockMHz:F0}{Reset}  {Bold}{Cyan}║{Reset}");
                sb.AppendLine($"{Bold}{Cyan}║{Reset}  {DarkGray}Memory:{Reset}       {Blue}{memClockMHz,6:F0} MHz{Reset}                                                   {Bold}{Cyan}║{Reset}");
                var vramPct = vramTotalMB > 0 ? vramUsedMB / vramTotalMB * 100 : 0;
                var vramColor = vramPct > 90 ? Red : vramPct > 75 ? Yellow : Green;
                sb.AppendLine($"{Bold}{Cyan}║{Reset}  {DarkGray}VRAM Used:{Reset}    {vramColor}{vramUsedMB,6:F0} MB{Reset} / {vramTotalMB:F0} MB {RenderBar(vramPct, 20, vramColor)}              {Bold}{Cyan}║{Reset}");
                sb.AppendLine($"{Bold}{Cyan}║{Reset}  {DarkGray}Sparkline (30s):{Reset} {RenderSparkline(ClockHistory, Yellow)}                           {Bold}{Cyan}║{Reset}");
                sb.AppendLine($"{Bold}{Cyan}╠══════════════════════════════════════════════════════════════════════════════╣{Reset}");

                // ── Throttle Status ────────────────────────────
                var throttleColor = throttleStatus.Contains("No Throttling") ? Green :
                    throttleStatus.Contains("CRITICAL") ? Red :
                    throttleStatus.Contains("WARNING") ? Yellow : Yellow;
                var throttleIcon = throttleStatus.Contains("No Throttling") ? "✅" :
                    isPowerThrottled && isThermalThrottled ? "🔥" :
                    isPowerThrottled ? "⚡" :
                    isThermalThrottled ? "🌡️" : "⚠️";

                sb.AppendLine($"{Bold}{Cyan}║{Reset}  {throttleIcon} {DarkGray}Throttle:{Reset} {throttleColor}{Bold}{Truncate(throttleStatus, 60),-60}{Reset}       {Bold}{Cyan}║{Reset}");
                sb.AppendLine($"{Bold}{Cyan}╚══════════════════════════════════════════════════════════════════════════════╝{Reset}");
                sb.AppendLine();
                sb.AppendLine($"  {DarkGray}[Q] Quit  [R] Reset Stats{Reset}");

                // Write in one go to minimize flicker
                Console.Write(sb.ToString());

                Thread.Sleep(RefreshIntervalMs);
            }

            // Cleanup
            Console.CursorVisible = true;
            Console.Clear();
            Console.WriteLine("GPU monitoring stopped.");

            return 0;
        }

        // ── Helpers ────────────────────────────────────────────

        private static void PushHistory(Queue<double> history, double value)
        {
            history.Enqueue(value);

            while (history.Count > HistoryLength)
            {
                history.Dequeue();
            }
        }

        private static string RenderBar(double percentage, int width, string color)
        {
            percentage = Math.Max(0, Math.Min(100, percentage));
            var filled = (int)(percentage / 100.0 * width);
            var empty = width - filled;

            return $"{color}{"█".Repeat(filled)}{DarkGray}{"░".Repeat(empty)}{Reset}";
        }

        private static string RenderSparkline(Queue<double> history, string color)
        {
            if (history.Count == 0) return new string(' ', 30);

            var values = history.ToArray();
            var min = values.Min();
            var max = values.Max();
            var range = max - min;

            if (range < 0.01) range = 1; // Prevent division by zero

            var sb = new StringBuilder();
            sb.Append(color);

            // Show last 30 values (or fewer if not enough history)
            var start = Math.Max(0, values.Length - 30);

            for (var i = start; i < values.Length; i++)
            {
                var normalized = (values[i] - min) / range;
                var idx = (int)(normalized * (SparkChars.Length - 1));
                idx = Math.Max(0, Math.Min(SparkChars.Length - 1, idx));
                sb.Append(SparkChars[idx]);
            }

            // Pad if less than 30 chars
            var sparkLen = values.Length - start;

            if (sparkLen < 30)
            {
                sb.Append(new string(' ', 30 - sparkLen));
            }

            sb.Append(Reset);
            return sb.ToString();
        }

        private static string Truncate(string text, int maxLength)
        {
            if (text.Length <= maxLength) return text;
            return text.Substring(0, maxLength - 3) + "...";
        }

        private static void EnableAnsiSupport()
        {
            // On Windows 10+, enable virtual terminal processing
            try
            {
                // Set console output encoding to UTF-8 for unicode chars
                Console.OutputEncoding = Encoding.UTF8;

                // Try enabling ANSI via Windows API
                if (OperatingSystem.IsWindows())
                {
                    var handle = GetStdHandle(-11); // STD_OUTPUT_HANDLE
                    GetConsoleMode(handle, out var mode);
                    SetConsoleMode(handle, mode | 0x0004); // ENABLE_VIRTUAL_TERMINAL_PROCESSING
                }
            }
            catch
            {
                // Fallback: ANSI might already work (Windows Terminal, ConEmu, etc.)
            }
        }

        // P/Invoke for enabling ANSI on older Windows consoles
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
    }

    internal static class StringExtensions
    {
        internal static string Repeat(this string s, int count)
        {
            if (count <= 0) return string.Empty;

            var sb = new StringBuilder(s.Length * count);

            for (var i = 0; i < count; i++)
            {
                sb.Append(s);
            }

            return sb.ToString();
        }
    }
}
