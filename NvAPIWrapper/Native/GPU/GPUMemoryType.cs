namespace NvAPIWrapper.Native.GPU
{
    /// <summary>
    ///     Holds a list of known memory types
    /// </summary>
    public enum GPUMemoryType : uint
    {
        /// <summary>
        ///     Unknown memory type
        /// </summary>
        Unknown = 0,

        /// <summary>
        ///     Synchronous dynamic random-access memory
        /// </summary>
        SDRAM,

        /// <summary>
        ///     Double Data Rate Synchronous Dynamic Random-Access Memory
        /// </summary>
        DDR1,

        /// <summary>
        ///     Double Data Rate 2 Synchronous Dynamic Random-Access Memory
        /// </summary>
        DDR2,

        /// <summary>
        ///     Graphics Double Data Rate 2 Synchronous Dynamic Random-Access Memory
        /// </summary>
        GDDR2,

        /// <summary>
        ///     Graphics Double Data Rate 3 Synchronous Dynamic Random-Access Memory
        /// </summary>
        GDDR3,

        /// <summary>
        ///     Graphics Double Data Rate 4 Synchronous Dynamic Random-Access Memory
        /// </summary>
        GDDR4,

        /// <summary>
        ///     Double Data Rate 3 Synchronous Dynamic Random-Access Memory
        /// </summary>
        DDR3,

        /// <summary>
        ///     Graphics Double Data Rate 5 Synchronous Dynamic Random-Access Memory
        /// </summary>
        GDDR5,

        /// <summary>
        ///     Lowe Power Double Data Rate 2 Synchronous Dynamic Random-Access Memory
        /// </summary>
        LPDDR2,

        /// <summary>
        ///     Graphics Double Data Rate 5X Synchronous Dynamic Random-Access Memory
        /// </summary>
        GDDR5X,

        /// <summary>
        ///     Graphics Double Data Rate 6 Synchronous Dynamic Random-Access Memory (RTX 40 series)
        /// </summary>
        GDDR6,

        /// <summary>
        ///     Graphics Double Data Rate 6X Synchronous Dynamic Random-Access Memory (RTX 4090, RTX 4080)
        /// </summary>
        GDDR6X,

        /// <summary>
        ///     High Bandwidth Memory 2nd Generation (RTX 6000 Ada)
        /// </summary>
        HBM2,

        /// <summary>
        ///     High Bandwidth Memory 2nd Generation Enhanced (RTX 6000 Ada, L40)
        /// </summary>
        HBM2e,

        /// <summary>
        ///     Graphics Double Data Rate 7 Synchronous Dynamic Random-Access Memory (RTX 50 series)
        /// </summary>
        GDDR7,

        /// <summary>
        ///     High Bandwidth Memory 3rd Generation (H100, H200)
        /// </summary>
        HBM3,

        /// <summary>
        ///     High Bandwidth Memory 3rd Generation Enhanced (H200, GH200)
        /// </summary>
        HBM3e,

        /// <summary>
        ///     Low Power Double Data Rate 5 (mobile/edge GPUs like Orin)
        /// </summary>
        LPDDR5,

        /// <summary>
        ///     Double Data Rate 5 Synchronous Dynamic Random-Access Memory (some embedded solutions)
        /// </summary>
        DDR5
    }
}