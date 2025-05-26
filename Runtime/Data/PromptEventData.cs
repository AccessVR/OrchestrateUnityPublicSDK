using System;
using UnityEngine;
using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
    [Serializable]
    public class PromptEventData : AbstractEventData
    {
        [JsonProperty("buttonText")] public string ButtonText = "OK";

        public override bool ShouldPauseForAcknowledgement() => true;
    }
}
