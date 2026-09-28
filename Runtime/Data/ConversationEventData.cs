using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace AccessVR.OrchestrateVR.SDK
{
    /// <summary>
    /// A Conversation layer (eventType 8): a live conversation with a Character at a
    /// moment in the scene. Pausing follows the author's "Pause Scene Playback"
    /// choice like a media layer; a non-pausing layer lives for its time window.
    /// The layer is acknowledged when the web conversation reports it ended (or
    /// could not start), or when a skippable layer is skipped while connecting.
    /// </summary>
    [Serializable]
    public class ConversationEventData : EventData
    {
        public const int EventTypeCharacter = 8;

        [JsonProperty("character")] public CharacterData Character;

        /// <summary>
        /// On (the default, and what an absent key means): the conversation
        /// begins the moment the layer appears. Off: the learner presses the
        /// layer's button first.
        /// </summary>
        [JsonProperty("autoStart")] public bool AutoStart = true;

        /// <summary>
        /// Whether a conversation can be attempted at all. A layer whose
        /// Character was removed or deactivated after publish still arrives in
        /// the payload (so the timeline keeps its shape) but must not launch.
        /// </summary>
        public bool IsAvailable =>
            Character != null && Character.Id > 0 && Character.IsActive && !string.IsNullOrEmpty(Character.LaunchPath);


        /// <summary>
        /// No asset to cache: the conversation is fetched live.
        /// </summary>
        public override List<DownloadableFileData> GetDownloadableFiles() => new();

        /// <summary>
        /// The absolute URL the WebView navigates to for this layer's
        /// conversation, carrying what the server needs to attribute it to this
        /// play-through: the run id the analytics mint, the version played, and
        /// the Course context when there is one.
        /// </summary>
        public string GetLaunchUrl(string runId, int? lessonVersionId, int? assignmentId, string uniqueKey = null)
        {
            return Orchestrate.GetUrl(BuildLaunchPath(Character?.LaunchPath, runId, lessonVersionId, assignmentId, uniqueKey));
        }

        /// <summary>
        /// Pure so it can be tested without a scene: the relative launch path plus
        /// its query string, values URL-escaped, empty values omitted.
        /// </summary>
        public static string BuildLaunchPath(string launchPath, string runId, int? lessonVersionId, int? assignmentId, string uniqueKey = null)
        {
            if (string.IsNullOrEmpty(launchPath))
            {
                throw new InvalidOperationException("This Conversation layer has no launch path.");
            }

            var query = new List<string>();

            if (!string.IsNullOrEmpty(runId))
            {
                query.Add("run=" + UnityWebRequest.EscapeURL(runId));
            }

            if (lessonVersionId.HasValue && lessonVersionId.Value > 0)
            {
                query.Add("lesson_version_id=" + lessonVersionId.Value);
            }

            if (assignmentId.HasValue && assignmentId.Value > 0)
            {
                query.Add("assignment=" + assignmentId.Value);
            }

            if (!string.IsNullOrEmpty(uniqueKey))
            {
                query.Add("unique_key=" + UnityWebRequest.EscapeURL(uniqueKey));
            }

            return query.Count == 0 ? launchPath : launchPath + "?" + string.Join("&", query);
        }
    }
}
