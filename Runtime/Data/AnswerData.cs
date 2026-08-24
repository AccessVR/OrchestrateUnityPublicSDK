using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace AccessVR.OrchestrateVR.SDK
{
    [Serializable]
    public class AnswerData : Data, IDownloadable
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("answerText")] public string Text;
        [JsonProperty("correctAnswer")] public bool IsCorrect = false;
        [JsonProperty("action")] public ActionData Action = ActionData.NoAction;

        /// <summary>
        /// Hotspot-mode (displayType Hotspots) worldspace position for this
        /// answer's hotspot, Three.js right-handed coords. Nullable: the
        /// renderer falls back to {0,0,-3} when absent.
        /// </summary>
        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public Vector3? Position;

        /// <summary>
        /// Hotspot-mode icon. Null means the renderer draws the default
        /// check glyph (FontAwesome solid f00c).
        /// </summary>
        [JsonProperty("icon")] public IconData Icon;

        [JsonProperty("backgroundColor")] private string _backgroundColor;
        [JsonProperty("iconColor")] private string _iconColor;

        /// <summary>Null means renderer fallback #111827.</summary>
        [JsonIgnore] public Color? BackgroundColor;

        /// <summary>Null means renderer fallback #FFFFFF.</summary>
        [JsonIgnore] public Color? IconColor;

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            BackgroundColor = StringUtils.ConvertToColor(_backgroundColor);
            IconColor = StringUtils.ConvertToColor(_iconColor);
        }

        public List<DownloadableFileData> GetDownloadableFiles()
        {
            return Action?.GetDownloadableFiles() ?? new();
        }
    }
}
