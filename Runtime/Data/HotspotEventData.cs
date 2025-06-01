using System.Collections.Generic;
using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
    public class HotspotEventData : EventData
    {
        [JsonProperty("hotspots")] public List<HotspotData> Hotspots;
        [JsonProperty("alwaysShowLabels")] public bool AlwaysShowLabels;
        
        public override void SetParentScene(SceneData scene)
        {
            base.SetParentScene(scene);
            Hotspots.ForEach(hotspot => hotspot.SetParentScene(scene));
        }
    }
    
    
}
