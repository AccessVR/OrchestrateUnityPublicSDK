using Newtonsoft.Json;
using System;

namespace AccessVR.OrchestrateVR.SDK
{
    public class SubmissionData
    {
        [JsonProperty("lessonId")] public int LessonId;
        [JsonProperty("startedOn")] public DateTime StartedOn;
        [JsonProperty("learnerId")] public int LearnerId;
        [JsonProperty("assignmentId")] public int? AssignmentId;
        [JsonProperty("score")] public float Score;
        
        [JsonProperty("id")] public string Id;
        [JsonProperty("completedOn")] public DateTime CompletedOn;
        
        public static SubmissionData Make(LessonData lesson, UserData user, DateTime startedOn, float score)
        {
            return Make(lesson, 0, user, startedOn, score);
        }
        
        public static SubmissionData Make(LessonData lesson, int assignmentId, UserData user, DateTime startedOn, float score)
        {
            return new()
            {
                LessonId = lesson.Id,
                AssignmentId = assignmentId,
                LearnerId = user.UserId,
                Score = score,
                StartedOn = startedOn,
            };
        }
    }
}