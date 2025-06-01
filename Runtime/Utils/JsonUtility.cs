using Newtonsoft.Json.Linq;
using UnityEngine;

namespace AccessVR.OrchestrateVR.SDK
{
    public class JsonUtility
    {
        public static Vector3 ToVector3(JObject json)
        {
            string? X = json["X"]?.ToString();
            string? Y = json["Y"]?.ToString();
            string? Z = json["Z"]?.ToString();
            
            StringUtils.AssertNotNullOrEmpty(X);
            StringUtils.AssertNotNullOrEmpty(Y);
            StringUtils.AssertNotNullOrEmpty(Z);

            return new Vector3(float.Parse(X), float.Parse(Y), float.Parse(Z));
        }
        
        public static Quaternion ToQuaternion(JObject json)
        {
            string? X = json["X"]?.ToString();
            string? Y = json["Y"]?.ToString();
            string? Z = json["Z"]?.ToString();
            string? W = json["W"]?.ToString();
            
            StringUtils.AssertNotNullOrEmpty(X);
            StringUtils.AssertNotNullOrEmpty(Y);
            StringUtils.AssertNotNullOrEmpty(Z);
            StringUtils.AssertNotNullOrEmpty(W);

            return new Quaternion(float.Parse(X), float.Parse(Y), float.Parse(Z), float.Parse(W));
        }
    }
}