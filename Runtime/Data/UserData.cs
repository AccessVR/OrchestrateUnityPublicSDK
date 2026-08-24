using System.Collections.Generic;
using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
    
    public class UserData
    {
        [JsonProperty("userId")] public int UserId;
        [JsonProperty("userName")] public string UserName;
        [JsonProperty("displayName")] public string DisplayName;
        [JsonProperty("roles")] public List<string> Roles;
        [JsonProperty("permissions")] public List<string> Permissions;
        [JsonProperty("userCode")] public string UserCode;

        /// <summary>
        /// The user's active organization, from the player manifest. Persists
        /// through the cached session file, so an offline launch still knows
        /// which tenant its analytics belong to.
        /// </summary>
        [JsonProperty("organizationId")] public int? OrganizationId;
        [JsonProperty("organizationName")] public string OrganizationName;

        /// <summary>
        /// ABXR collector credentials, also session-cached for offline runs.
        /// </summary>
        [JsonProperty("abxr")] public AbxrConfigData Abxr;

        [JsonIgnore] public bool IsAnonymous;
    }

}