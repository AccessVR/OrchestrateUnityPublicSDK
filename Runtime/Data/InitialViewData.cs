using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace AccessVR.OrchestrateVR.SDK
{
    public class InitialViewData
    {
        [JsonProperty("quaternion")] private JObject _quaternion;

        [JsonIgnore] public Quaternion Quaternion;
        
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            Quaternion = JsonUtility.ToQuaternion(_quaternion);
        }
    }
}