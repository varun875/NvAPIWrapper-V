using NvAPIWrapper.Native.GPU.Structures;

namespace NvAPIWrapper.Native.GPU
{
    /// <summary>
    ///     Contains the list of clocks available to public
    /// </summary>
    public enum PublicClockDomain
    {
        /// <summary>
        ///     Undefined
        /// </summary>
        Undefined = ClockFrequenciesV1.MaxClocksPerGPU,

        /// <summary>
        ///     3D graphics clock
        /// </summary>
        Graphics = 0,

        /// <summary>
        ///     Memory clock
        /// </summary>
        Memory = 4,

        /// <summary>
        ///     Processor clock
        /// </summary>
        Processor = 7,

        /// <summary>
        ///     Video decoding clock
        /// </summary>
        Video = 8,

        /// <summary>
        ///     Base/Core clock (Ada+ architectures)
        /// </summary>
        BaseClock = 10,

        /// <summary>
        ///     Video encoding clock (NVENC)
        /// </summary>
        VideoEncode = 11,

        /// <summary>
        ///     Tensor clock (separate from graphics on H100+)
        /// </summary>
        Tensor = 12,

        /// <summary>
        ///     Display clock
        /// </summary>
        Display = 13
    }
}