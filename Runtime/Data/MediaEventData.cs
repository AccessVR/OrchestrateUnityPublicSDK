using System;
using Newtonsoft.Json;
using UnityEngine;

namespace AccessVR.OrchestrateVR.SDK
{
    [Serializable]
    public class MediaEventData : EventData
    {
        /// <summary>
        /// Worldspace position relative to the camera, in Three.js right-handed
        /// coordinates. Nullable: an absent value means "no position set" and
        /// signals the renderer to fall back to non-spatial audio (preserves
        /// legacy Hidden-Media behavior for events authored before this field
        /// existed). Set by the editor when the author switches a Media event
        /// to Worldspace or drops a placeable spatial-audio source on Hidden.
        /// </summary>
        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public Vector3? Position;

        /// <summary>
        /// Worldspace rotation in Euler degrees. Only honored when
        /// <see cref="SpriteMode"/> is false and <see cref="EventData.DisplayType"/>
        /// is <see cref="DisplayTypeOptions.WorldSpace"/>; otherwise the renderer
        /// billboards the layer toward the camera.
        /// </summary>
        [JsonProperty("rotation")] public Vector3 Rotation;

        /// <summary>
        /// When true (default), a Worldspace Media layer always faces the camera
        /// (billboard). When false, the layer holds the authored <see cref="Rotation"/>.
        /// Default true is load-bearing: it makes a fresh switch-to-Worldspace
        /// behave like a Hotspot without the author having to choose an orientation.
        /// </summary>
        [JsonProperty("spriteMode")] public bool SpriteMode = true;
    }
}
