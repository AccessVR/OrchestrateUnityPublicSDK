using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace AccessVR.OrchestrateVR.SDK
{
    [Serializable]
    public class HotspotData: Data
    {
        [FormerlySerializedAs("name")] [JsonProperty("name")] public string Name;
        [FormerlySerializedAs("hPos")] [JsonProperty("position")] public Vector3 Position;
        [FormerlySerializedAs("scale")] [JsonProperty("scale")] public float Scale;
        [JsonProperty("font")] public string Font;
        [JsonProperty("id")] public string Id;
        [JsonProperty("unicode")] public string Character;
        [JsonProperty("action")] public ActionData Action;
        [FormerlySerializedAs("acknowledged")] [JsonProperty("acknowledged")] public bool Acknowledged = false;
        [FormerlySerializedAs("confirm")] [JsonProperty("confirm")] public bool Confirm = true;
        [FormerlySerializedAs("alwaysShowLabel")] [JsonProperty("alwaysShowLabel")] public bool AlwaysShowLabel = true;
        [JsonProperty("backgroundColor")] public string _backgroundColor;

        [JsonIgnore] public Color? BackgroundColor;

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            BackgroundColor = StringUtils.ConvertToColor(_backgroundColor);
        }

        public override void SetParentScene(SceneData scene)
        {
            base.SetParentScene(scene);
            Action?.SetParentScene(scene);
        }
        
    }

}