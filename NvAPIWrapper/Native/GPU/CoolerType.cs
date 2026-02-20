namespace NvAPIWrapper.Native.GPU
{
    /// <summary>
    ///     Holds a list of possible cooler types
    /// </summary>
    public enum CoolerType : uint
    {
        /// <summary>
        ///     No cooler type
        /// </summary>
        None,

        /// <summary>
        ///     Air cooling (axial/blower fans)
        /// </summary>
        Fan,

        /// <summary>
        ///     Water cooling (custom loop)
        /// </summary>
        Water,

        /// <summary>
        ///     Liquid nitrogen cooling (extreme overclocking)
        /// </summary>
        LiquidNitrogen,

        /// <summary>
        ///     AIO liquid cooling (all-in-one closed loop, common on RTX 40/50 hybrid cards)
        /// </summary>
        AIOLiquid,

        /// <summary>
        ///     Passive cooling (no active cooling, vapor chamber heatsink only)
        /// </summary>
        Passive,

        /// <summary>
        ///     Immersion cooling (data center / enterprise)
        /// </summary>
        Immersion
    }
}