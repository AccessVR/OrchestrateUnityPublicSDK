using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
    /// <summary>
    /// A smaller copy of a video, for a device that can't decode the
    /// full-resolution one (the server makes one for large 360° and 180° video).
    /// </summary>
    public class VideoDownloadData
    {
        [JsonProperty("path")] public string Path;
        [JsonProperty("width")] public int Width;
        [JsonProperty("height")] public int Height;
        [JsonProperty("codec")] public string Codec;

        [JsonIgnore] public long Pixels => (long) Width * Height;
    }
}
