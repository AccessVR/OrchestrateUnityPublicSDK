using Newtonsoft.Json.Linq;
using UnityEngine;

namespace AccessVR.OrchestrateVR.SDK
{
    public class JsonUtility
    {
        public static Vector3 ToVector3(JObject json)
        { 
            var x = json["X"]?.ToString();
            var y = json["Y"]?.ToString();
            var z = json["Z"]?.ToString();
            
            StringUtils.AssertNotNullOrEmpty(x);
            StringUtils.AssertNotNullOrEmpty(y);
            StringUtils.AssertNotNullOrEmpty(z);

            return new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
        }
        
        public static Quaternion ToQuaternion(JObject json)
        {
            var x = json["X"]?.ToString();
            var y = json["Y"]?.ToString();
            var z = json["Z"]?.ToString();
            var w = json["W"]?.ToString();
            
            StringUtils.AssertNotNullOrEmpty(x);
            StringUtils.AssertNotNullOrEmpty(y);
            StringUtils.AssertNotNullOrEmpty(z);
            StringUtils.AssertNotNullOrEmpty(w);

            return new Quaternion(float.Parse(x), float.Parse(y), float.Parse(z), float.Parse(w));
        }
    }
}