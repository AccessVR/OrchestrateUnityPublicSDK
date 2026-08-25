namespace AccessVR.OrchestrateVR.SDK
{
    /// <summary>
    /// Mirrors the server's VideoType table (Asset.videoTypeId 1-7).
    /// </summary>
    public enum VideoType
    {
        Unknown = 0,
        Video360 = 1,
        Video2D = 2,
        Video360Stereo = 3,
        VideoStereoSbs = 4,
        VideoStereoTab = 5,
        Video180 = 6,
        Video180Stereo = 7
    }

    public enum StereoLayout
    {
        None,
        SideBySide,
        TopBottom
    }
}
