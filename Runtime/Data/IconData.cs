using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
    public class IconData: Data
    {
        [JsonProperty("unicode")] public string Unicode;
    }
}