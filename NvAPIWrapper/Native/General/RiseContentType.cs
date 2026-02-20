namespace NvAPIWrapper.Native.General
{
    /// <summary>
    ///     RISE content type values defined by NVAPI.
    /// </summary>
    public enum RiseContentType : uint
    {
        /// <summary>
        ///     Invalid/unspecified content type.
        /// </summary>
        Invalid = 0,

        /// <summary>
        ///     Textual content.
        /// </summary>
        Text = 1,

        /// <summary>
        ///     Graph content.
        /// </summary>
        Graph = 2,

        /// <summary>
        ///     Custom behavior request.
        /// </summary>
        CustomBehavior = 3,

        /// <summary>
        ///     Custom behavior result.
        /// </summary>
        CustomBehaviorResult = 4,

        /// <summary>
        ///     Installation status.
        /// </summary>
        Installing = 5,

        /// <summary>
        ///     Progress update.
        /// </summary>
        ProgressUpdate = 6,

        /// <summary>
        ///     Ready notification.
        /// </summary>
        Ready = 7,

        /// <summary>
        ///     Download request.
        /// </summary>
        DownloadRequest = 8,

        /// <summary>
        ///     Update information.
        /// </summary>
        UpdateInfo = 9
    }
}
