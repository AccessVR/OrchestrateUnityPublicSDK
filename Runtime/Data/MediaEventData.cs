using System;
using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
    /// <summary>
    /// Media (image / audio / video) timed event. The worldspace transform
    /// fields (position / rotation / spriteMode / scale) live on
    /// <see cref="EventData"/> — the editor writes them on any
    /// worldspace-capable event type, not just Media.
    /// </summary>
    [Serializable]
    public class MediaEventData : EventData
    {
        /// <summary>
        /// Hidden-media spatial-audio opt-in. Null = legacy payload (fall back
        /// to the Position.HasValue proxy: a placed position implies spatial).
        /// False = authoritatively non-spatial even if a stray position
        /// exists — the editor deletes the position when the toggle is turned
        /// off, but a defensive read costs nothing. True = spatial; the editor
        /// seeds position {0,0,-3} when the toggle is turned on.
        /// </summary>
        [JsonProperty("spatial")] public bool? Spatial;

        /// <summary>
        /// The part of the file the Layer plays. Absent plays the whole file.
        /// </summary>
        [JsonProperty("clip")] private MediaClipData _clip;

        [JsonIgnore] public MediaClipData Clip => _clip ?? MediaClipData.Whole;

        /// <summary>"card" (absent) frames the picture; "none" shows only the picture.</summary>
        [JsonProperty("frame")] public string Frame;

        /// <summary>
        /// Where round the learner a Panorama stands: "front" (absent), "back",
        /// "left" or "right" of the heading the Scene's initial view opens on.
        /// </summary>
        [JsonProperty("panoramaPosition")] public string PanoramaPosition;

        /// <summary>
        /// How far a Panorama's position turns from the Scene's front, in the
        /// web player's camera longitude, which grows to the right.
        /// </summary>
        [JsonIgnore] public float PanoramaTurnDegrees => PanoramaPosition switch
        {
            "back" => 180f,
            "left" => -90f,
            "right" => 90f,
            _ => 0f,
        };

        [JsonIgnore] public bool IsPanorama => DisplayType == DisplayTypeOptions.Panorama;

        [JsonIgnore] public bool IsFrameless => Frame == "none";
    }
}
