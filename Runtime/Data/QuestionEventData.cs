using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
    public class QuestionEventData : EventData
    {
        [JsonProperty("questions")] public List<QuestionData> Questions;

        /// <summary>
        /// Editor sentinel: true once the author has explicitly used the
        /// "Pause Scene Playback" toggle on this question. Required because
        /// every legacy question already stores a meaningless
        /// <c>pausePlayback: false</c> — without the sentinel, old content
        /// would silently stop pausing.
        /// </summary>
        [JsonProperty("_pausePlaybackConfigured")] private bool _pausePlaybackConfigured = false;

        /// <summary>
        /// A question pauses the scene unless the author explicitly opted out
        /// (sentinel set AND pausePlayback false). Action-triggered questions
        /// always pause, mirroring the web player. Note the base class forces
        /// pausePlayback true when EndTime &lt; StartTime, so a "non-pausing"
        /// question without a valid time window still pauses — by design, since
        /// only a time window can auto-dismiss a non-pausing event.
        /// </summary>
        public override bool ShouldPauseForAcknowledgement()
        {
            if (IsActionEvent())
            {
                return true;
            }
            return !(_pausePlaybackConfigured && !PausesPlayback());
        }

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
