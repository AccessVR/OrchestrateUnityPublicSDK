using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
    [Serializable]
    public class QuestionData: Data
    {
        public enum QuestionTypeOptions
        {
            Single,
            Multiple
        }

        [JsonProperty("id")] public string Id = "0";
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
            AddPlaceholderAnswers();
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
        
        public override void SetParentScene(SceneData scene)
        {
            base.SetParentScene(scene);
            Answers.ForEach(answer => answer.SetParentScene(scene));
        }
    }
    
    public class QuestionEventData : EventData
    {
        [JsonProperty("questions")] public List<QuestionData> Questions;

        public override bool ShouldPauseForAcknowledgement() => true;

        public override void SetParentScene(SceneData scene)
        {
            base.SetParentScene(scene);
            Questions.ForEach(question => question.SetParentScene(scene));
        }
    }
}
