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

        /// <summary>
        /// "left" or "right": the hand a Conversation layer's virtual tablet
        /// follows. Null until the learner chooses; right is assumed.
        /// </summary>
        [JsonProperty("dominantHand")]
        public string DominantHand { get; set; }

        public SessionData()
        {
            AuthToken = null;
            User = null;
            UserCode = null;
            OfflineState = null;
            DefaultSkybox = null;
            DominantHand = null;
        }
    }
}
