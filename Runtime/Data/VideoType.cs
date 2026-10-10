namespace AccessVR.OrchestrateVR.SDK
{
    /// <summary>
    /// Mirrors the server's VideoType table (Asset.videoTypeId 1-9). An id this
    /// build doesn't know reads as Unknown, so the player falls back to the
    /// file's own metadata and treats the picture as mono.
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
        Video180Stereo = 7,
        Video180StereoTab = 8,
        Video360StereoSbs = 9
    }

    public enum StereoLayout
    {
        None,
        SideBySide,
        TopBottom
    }
}
