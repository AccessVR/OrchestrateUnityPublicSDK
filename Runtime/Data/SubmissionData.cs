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

        /// <summary>
        /// The run id this playthrough's ABXR telemetry was emitted under.
        /// Ties the LearningSession to its analytics rows; the server is
        /// idempotent on it, which is what makes submission retries safe.
        /// Null when the run was untracked.
        /// </summary>
        [JsonProperty("abxrRunId", NullValueHandling = NullValueHandling.Ignore)]
        public string AbxrRunId;

        /// <summary>
        /// Contact-capture session to complete instead of opening a new one.
        /// Always null until the VR contact-capture flow ships.
        /// </summary>
        [JsonProperty("sessionId")] public int? SessionId;

        /// <summary>
        /// The version row id actually played (LessonData.PublishedLessonId).
        /// Client-reported because the server cannot infer it after a
        /// republish; validated server-side against the lesson.
        /// </summary>
        [JsonProperty("lessonVersionId")] public int? LessonVersionId;

        public static SubmissionData Make(LessonData lesson, UserData user, DateTime startedOn, float score)
        {
            return Make(lesson, 0, user, startedOn, score);
        }

        public static SubmissionData Make(LessonData lesson, int assignmentId, UserData user, DateTime startedOn, float score, string abxrRunId = null)
        {
            return new()
            {
                LessonId = lesson.Id,
                AssignmentId = assignmentId,
                LearnerId = user.UserId,
                Score = score,
                StartedOn = startedOn,
                AbxrRunId = abxrRunId,
                SessionId = null,
                LessonVersionId = lesson.PublishedLessonId,
            };
        }
    }
}