using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Runtime.Serialization;

namespace AccessVR.OrchestrateVR.SDK
{
    /// <summary>
    /// Layer-level hotspot visibility. Hidden hotspots are invisible until the
    /// learner's pointer finds them (pulsing ring), then render a persistent
    /// acknowledged disc once clicked.
    /// </summary>
    public enum HotspotVisibilityOptions
    {
        Visible,
        Hidden,
    }

    public class HotspotEventData : EventData
    {
        [JsonProperty("hotspots")] public List<HotspotData> Hotspots;
        [JsonProperty("alwaysShowLabels")] public bool AlwaysShowLabels;

        /// <summary>
        /// Event-level visibility: "visible" | "hidden". Absent means visible.
        /// </summary>
        [JsonProperty("hotspotVisibility")] private string _hotspotVisibility;

        [JsonIgnore] public HotspotVisibilityOptions HotspotVisibility = HotspotVisibilityOptions.Visible;

        public override void SetParentScene(SceneData scene)
        {
            base.SetParentScene(scene);
            Hotspots.ForEach(hotspot => hotspot.SetParentScene(scene));
        }

        public override void AfterDeserialized(StreamingContext context)
        {
            base.AfterDeserialized(context);

            HotspotVisibility = _hotspotVisibility == "hidden"
                ? HotspotVisibilityOptions.Hidden
                : HotspotVisibilityOptions.Visible;

            Hotspots.ForEach(hotspot =>
            {
                hotspot.Visibility = HotspotVisibility;
                // A hidden hotspot must never be revealed by its label: a
                // stored alwaysShowLabels=true is ignored in Hidden mode (web
                // effectiveAlwaysShowLabels rule).
                hotspot.AlwaysShowLabel = HotspotVisibility == HotspotVisibilityOptions.Hidden
                    ? false
                    : AlwaysShowLabels;
            });
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
