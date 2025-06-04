using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
    public class QuestionEventData : EventData
    {
        [JsonProperty("questions")] public List<QuestionData> Questions;

        public override bool ShouldPauseForAcknowledgement() => true;

        public override void SetParentScene(SceneData scene)
        {
            base.SetParentScene(scene);
            Questions.ForEach(question => question.SetParentScene(scene));
        }

        public override List<DownloadableFileData> GetDownloadableFiles()
        {
            List<DownloadableFileData> list = base.GetDownloadableFiles();

            list.AddRange(Questions
                .Select(question => question.GetDownloadableFiles())
                .SelectMany(fileList => fileList)
                .ToList());

            return list.Where(file => file != null).Distinct().ToList();
        }
    }
}
