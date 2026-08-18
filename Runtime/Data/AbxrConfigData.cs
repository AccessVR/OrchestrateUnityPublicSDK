using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
    /// <summary>
    /// ABXR collector credentials delivered by the player manifest (the native
    /// analogue of the props the server injects into the web viewer's page).
    /// When <see cref="Enabled"/> is false, or any credential is missing,
    /// analytics no-ops for the session.
    /// </summary>
    public class AbxrConfigData
    {
        [JsonProperty("enabled")] public bool Enabled;
        [JsonProperty("appId")] public string AppId;
        [JsonProperty("orgId")] public string OrgId;
        [JsonProperty("authSecret")] public string AuthSecret;

        /// <summary>
        /// Host root (scheme + host), not a path under /api — the AbxrLib SDK
        /// keeps only scheme+host and calls its own /v1/* routes.
        /// </summary>
        [JsonProperty("baseUrl")] public string BaseUrl;

        [JsonIgnore]
        public bool IsUsable =>
            Enabled
            && !string.IsNullOrEmpty(AppId)
            && !string.IsNullOrEmpty(OrgId)
            && !string.IsNullOrEmpty(AuthSecret);
    }
}
