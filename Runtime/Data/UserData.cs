using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
    
    public class UserData
    {
        [JsonProperty("userId")] public string UserId { get; set; }
        [JsonProperty("userName")] public string UserName { get; set; }
        [JsonProperty("displayName")] public string DisplayName { get; set; }
        [JsonProperty("roles")] public List<string> Roles { get; set; }
        [JsonProperty("permissions")] public List<string> Permissions { get; set; }
        
        public bool IsAnonymous { get; set; }
    }

}