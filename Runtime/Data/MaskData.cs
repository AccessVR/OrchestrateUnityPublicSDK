using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace AccessVR.OrchestrateVR.SDK
{
    /// <summary>
    /// The 180-simulation mask on a 360 scene: an angular window centered on a
    /// focal point, fading to black outside. Longitude/latitude use the web
    /// player's camera convention (longitude -90 = scene front); Size is the
    /// angular diameter of the fully visible window in degrees. The feather
    /// width is a client constant, never on the wire.
    /// </summary>
    public class MaskData
    {
        public const float MinSize = 30f;
        public const float MaxSize = 270f;

        [JsonProperty("enabled")] public bool Enabled;
        [JsonProperty("longitude")] public float Longitude;
        [JsonProperty("latitude")] public float Latitude;
        [JsonProperty("size")] public float Size = 180f;

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            Size = Mathf.Clamp(Size, MinSize, MaxSize);
            Latitude = Mathf.Clamp(Latitude, -90f, 90f);
        }
    }
}
