using System;
using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
    [Serializable]
    public class AnswerData : Data
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("answerText")] public string Text;
        [JsonProperty("correctAnswer")] public bool IsCorrect = false;
        [JsonProperty("action")] public ActionData Action;
    }
}