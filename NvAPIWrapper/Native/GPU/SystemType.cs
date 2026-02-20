namespace NvAPIWrapper.Native.GPU
{
    /// <summary>
    ///     GPU systems
    /// </summary>
    public enum SystemType
    {
        /// <summary>
        ///     Unknown type
        /// </summary>
        Unknown = 0,

        /// <summary>
        ///     Laptop GPU
        /// </summary>
        Laptop = 1,

        /// <summary>
        ///     Desktop GPU
        /// </summary>
        Desktop = 2,

        /// <summary>
        ///     Professional Workstation (RTX 6000/6000 Ada)
        /// </summary>
        Workstation = 3,

        /// <summary>
        ///     Data Center GPU (Tesla series, H100, H200)
        /// </summary>
        DataCenter = 4,

        /// <summary>
        ///     Hyperscale/Large Model Training (GH200, multi-GPU systems)
        /// </summary>
        Hyperscale = 5,

        /// <summary>
        ///     Edge/Embedded (Orin, AGX Orin)
        /// </summary>
        Edge = 6
    }
}