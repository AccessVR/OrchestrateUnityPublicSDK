using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
    [Serializable]
    public class AnswerData : Data, IDownloadable
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("answerText")] public string Text;
        [JsonProperty("correctAnswer")] public bool IsCorrect = false;
        [JsonProperty("action")] public ActionData Action;

        public List<DownloadableFileData> GetDownloadableFiles()
        {
            return Action?.GetDownloadableFiles() ?? new();
        }
    }
}