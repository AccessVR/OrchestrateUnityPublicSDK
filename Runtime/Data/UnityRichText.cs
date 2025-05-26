using Newtonsoft.Json.Linq;

namespace AccessVR.OrchestrateVR.SDK
{
    public class UnityRichText
    {
        public string Content;
        
        public UnityRichText(JObject json)
        {
            Content = (string) json["content"];
        }
    }
}