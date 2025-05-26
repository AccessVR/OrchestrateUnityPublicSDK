using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AccessVR.OrchestrateVR.SDK
{
    [Serializable]
    public class AnswerData
    {
        [JsonProperty("id")] public string Id = "0";
        [JsonProperty("answerText")] public string Text;
        [JsonProperty("correctAnswer")] public bool IsCorrect = false;
        [JsonProperty("action")] public ActionData action;

        public AnswerData(string text, bool isCorrect)
        {
            action = new ActionData();
            Text = text;
            IsCorrect = isCorrect;
        }
        public AnswerData(JObject json, SceneData parentScene)
        {
            action = new ActionData();

            if (json["id"] != null)
                Id = (string)json["id"];
            if (json["answerText"] != null)
                Text = (string)json["answerText"];
            if (json["correctAnswer"] != null)
                IsCorrect = (bool)json["correctAnswer"];


            if (json["action"] != null)
                action = new ActionData(json["action"].ToObject<JObject>(), parentScene);
        }
    }

    [Serializable]
    public class QuestionData
    {
        public enum QuestionTypeOptions
        {
            Single,
            Multiple
        }

        [JsonProperty("id")] public string Id = "0";
        [JsonProperty("questionType")] public QuestionTypeOptions QuestionType;
        [JsonProperty("text")] public string Text;
        [JsonProperty("assessable")] public bool Assessable = true;
        [JsonProperty("remediate")] public bool Remediate = true;
        [JsonProperty("answers")] public List<AnswerData> Answers;

        public QuestionData()
        {
            Answers = new List<AnswerData>();
            QuestionType = QuestionTypeOptions.Single;
            Text = "This is a placeholder question.";

            Answers.Add(new AnswerData("Temporary Answer 1 (Correct)", true));
            Answers.Add(new AnswerData("Temporary Answer 2", false));
        }

        public QuestionData(JObject json, SceneData parentScene)
        {
            Answers = new List<AnswerData>();
            if (json["id"] != null)
                Id = (string)json["id"];
            if (json["questionType"] != null)
            {
                switch ((int)json["questionType"])
                {
                    default:
                    case 0:
                        QuestionType = QuestionTypeOptions.Single;
                        break;
                    case 1:
                        QuestionType = QuestionTypeOptions.Multiple;
                        break;
                }
            }


            if (json["text"] != null)
                Text = (string)json["text"];

            if (json["remediate"] != null)
                Remediate = (bool)json["remediate"];

            if (json["assessable"] != null)
                Assessable = (bool)json["assessable"];


            if (json["answers"] != null)
            {
                foreach (JToken token in (JArray)json["answers"])
                {
                    Answers.Add(new AnswerData((JObject)token, parentScene));
                }
            }

            if (Answers.Count <= 0)
            {
                Answers.Add(new AnswerData("Temporary Answer 1 (Correct)", true));
                Answers.Add(new AnswerData("Temporary Answer 2", false));
            }
        }
    }
    
    public class QuestionEventData : AbstractEventData
    {
        [JsonProperty("questions")] public List<QuestionData> Questions;

        public override bool ShouldPauseForAcknowledgement() => true;
    }
}
