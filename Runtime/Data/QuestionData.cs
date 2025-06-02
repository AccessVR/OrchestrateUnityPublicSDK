using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
    [Serializable]
    public class QuestionData: Data, IDownloadable
    {
        public enum QuestionTypeOptions
        {
            Single,
            Multiple
        }

        [JsonProperty("id")] public string Id;
        [JsonProperty("text")] public string Text;
        [JsonProperty("assessable")] public bool Assessable = true;
        [JsonProperty("remediate")] public bool Remediate = true;
        [JsonProperty("answers")] public List<AnswerData> Answers;
        [JsonProperty("questionType")] private int _questionType;

        [JsonIgnore] public QuestionTypeOptions QuestionType;
        
        public QuestionData()
        {
            Answers = new List<AnswerData>();
            QuestionType = QuestionTypeOptions.Single;
            Text = "This is a placeholder question.";
        }

        private void AddPlaceholderAnswers()
        {
            Answers.Add(new AnswerData
            {
                Text = "Temporary Answer 1 (Correct)",
                IsCorrect = true
            });
            
            Answers.Add(new AnswerData
            {
                Text = "Temporary Answer 2",
                IsCorrect = false
            });
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            QuestionType = _questionType switch
            {
                1 => QuestionTypeOptions.Multiple,
                _ => QuestionTypeOptions.Single
            };

            if (Answers.Count <= 0)
            {
                AddPlaceholderAnswers();
            }
        }

        public List<DownloadableFileData> GetDownloadableFiles()
        {
            List<DownloadableFileData> list = new();
            
            list.AddRange(Answers
                .Select(answer => answer.GetDownloadableFiles())
                .SelectMany(fileList => fileList)
                .ToList());

            return list.Where(file => file != null).Distinct().ToList();
        }

        public override void SetParentScene(SceneData scene)
        {
            base.SetParentScene(scene);
            Answers.ForEach(answer => answer.SetParentScene(scene));
        }
    }
}