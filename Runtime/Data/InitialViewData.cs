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

        /// <summary>
        /// The heading the view opens on, in the web player's camera
        /// longitude, degrees. The Scene's front, back, left and right (a
        /// Panorama's position) are measured from it.
        /// </summary>
        [JsonProperty("longitude")] public float? Longitude;
        
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            Quaternion = JsonUtility.ToQuaternion(_quaternion);
        }
    }
}