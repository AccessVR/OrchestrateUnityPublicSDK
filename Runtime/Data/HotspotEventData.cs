using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
    [Serializable]
    public class Hotspot
    {
        [JsonProperty("name")] public string name;
        [JsonProperty("position")] public Vector3 hPos;
        [JsonProperty("scale")] public float scale;
        [JsonProperty("font")] public string Font;
        [JsonProperty("id")] public string Id;
        [JsonProperty("unicode")] public string Character;
        [JsonProperty("backgroundColor")] public Color? BackgroundColor;
        [JsonProperty("action")] public ActionData action;
        [JsonProperty("acknowledged")] public bool acknowledged = false;
        [JsonProperty("confirm")] public bool confirm = true;
        [JsonProperty("alwaysShowLabel")] public bool alwaysShowLabel = true;
    }

    public class HotspotEventData : AbstractEventData
    {
        [JsonProperty("hotspots")] public List<Hotspot> hotspots;
        [JsonProperty("alwaysShowLabels")] public bool AlwaysShowLabels = false;

    }
}
