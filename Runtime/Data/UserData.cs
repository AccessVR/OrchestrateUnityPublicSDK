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
        
        [JsonIgnore] public bool IsAnonymous;
    }

}