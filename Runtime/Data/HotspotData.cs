using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace AccessVR.OrchestrateVR.SDK
{
    [Serializable]
    public class HotspotData: Data, IDownloadable
    {
        [FormerlySerializedAs("name")] [JsonProperty("name")] public string Name;
        [FormerlySerializedAs("hPos")] [JsonProperty("position")] public Vector3 Position;
        [FormerlySerializedAs("scale")] [JsonProperty("scale")] public float Scale;
        [JsonProperty("font")] public string Font;
        [JsonProperty("id")] public string Id;
        [JsonProperty("icon")] public IconData Icon;
        [JsonProperty("action")] public ActionData Action = ActionData.NoAction;
        [FormerlySerializedAs("acknowledged")] [JsonProperty("acknowledged")] public bool Acknowledged = false;
        [FormerlySerializedAs("confirm")] [JsonProperty("confirm")] public bool Confirm = true;
        [JsonProperty("backgroundColor")] public string _backgroundColor;
        [JsonProperty("iconColor")] private string _iconColor;
        public bool AlwaysShowLabel = false;

        /// <summary>
        /// Stamped by the parent <see cref="HotspotEventData"/> after
        /// deserialization, mirroring the AlwaysShowLabel pattern.
        /// </summary>
        [JsonIgnore] public HotspotVisibilityOptions Visibility = HotspotVisibilityOptions.Visible;

        [JsonIgnore] public Color? BackgroundColor;
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

        public override void SetParentScene(SceneData scene)
        {
            base.SetParentScene(scene);
            Action?.SetParentScene(scene);
        }
        
    }

}