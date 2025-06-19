using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Runtime.Serialization;

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

        public override void AfterDeserialized(StreamingContext context)
        {
            base.AfterDeserialized(context);
            Hotspots.ForEach(hotspot => hotspot.AlwaysShowLabel = AlwaysShowLabels);
        }
        
        public override List<DownloadableFileData> GetDownloadableFiles()
        {
            List<DownloadableFileData> list = base.GetDownloadableFiles();

            list.AddRange(Hotspots
                .Select(hotspot => hotspot.GetDownloadableFiles())
                .SelectMany(fileList => fileList)
                .ToList());

            return list.Where(file => file != null).Distinct().ToList();
        }
    }
    
    
}
