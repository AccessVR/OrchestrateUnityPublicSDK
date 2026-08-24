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
    }
}
