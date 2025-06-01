using System;

namespace AccessVR.OrchestrateVR.SDK
{
    [Serializable]
    public class PromptEventData : EventData
    {
        public override bool ShouldPauseForAcknowledgement() => true;
    }
}
