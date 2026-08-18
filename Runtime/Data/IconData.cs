using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
    public class IconData: Data
    {
        /// <summary>Icon slug, e.g. "check".</summary>
        [JsonProperty("id")] public string Id;

        /// <summary>FontAwesome style, e.g. "solid".</summary>
        [JsonProperty("font")] public string Font;

        [JsonProperty("unicode")] public string Unicode;
    }
}
