# NvAPI Realtime Power Monitor

A lightweight, real-time GPU monitoring console application built with NvAPIWrapper.

## Features

- **Live Power Draw (Watts)**: Shows real-time board and GPU power usage.
- **Automatic TDP Detection**: Uses the built-in database to resolve watts without manual configuration.
- **Sparkline Graphs**: 30-second history for Power, GPU Usage, Temp, Clocks, and Fan Speed.
- **Throttle Detection**: Instantly shows if performance is limited by Power, Thermal, Voltage, or No Load.
- **Zero-Flicker UI**: Uses ANSI escape codes for smooth 60fps-like updates.

## How to Run

1. Build the solution:
   ```powershell
   dotnet build
   ```

2. Run the monitor from the output directory:
   ```powershell
   ./NvAPIRealtimeMonitor/bin/Debug/net8.0/NvAPIRealtimeMonitor.exe
   ```

## Controls

- **[Q]** or **[Esc]**: Quit the application.
- **[R]**: Reset peak/average statistics and history graphs.
