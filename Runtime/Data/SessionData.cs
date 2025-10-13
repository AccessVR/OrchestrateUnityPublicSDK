using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
    public class SessionData
    {
        [JsonProperty("authToken")]
        public string AuthToken { get; set; }

        [JsonProperty("user")]
        public UserData User { get; set; }

        [JsonProperty("userCode")]
        public string UserCode { get; set; }

        [JsonProperty("offlineState")]
        public string OfflineState { get; set; }

        [JsonProperty("defaultSkybox")]
        public string DefaultSkybox { get; set; }

        public SessionData()
        {
            AuthToken = null;
            User = null;
            UserCode = null;
            OfflineState = null;
            DefaultSkybox = null;
        }
    }
}
