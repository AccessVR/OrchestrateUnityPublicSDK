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
        /// <summary>
        /// "Loop Until Acknowledged": replay this layer's own time window
        /// instead of letting it end, until the learner deals with the
        /// question. Only meaningful for a non-pausing question that shows
        /// correct answers — a pausing question already holds the scene.
        /// </summary>
        [JsonProperty("loopUntilAcknowledged")] public bool LoopUntilAcknowledged = false;

        /// <summary>
        /// Fires when the layer is acknowledged, whatever the learner answered
        /// — distinct from the per-answer actions on <see cref="AnswerData"/>.
        /// A layer whose window elapses before the learner answers is never
        /// acknowledged, so this deliberately does not fire for it.
        /// </summary>
        [JsonProperty("acknowledgeAction")] public ActionData AcknowledgeAction = ActionData.NoAction;

        public override bool ShouldPauseForAcknowledgement()
        {
            if (IsActionEvent())
            {
                return true;
            }
            return !(_pausePlaybackConfigured && !PausesPlayback());
        }

        /// <summary>
        /// Whether the learner is shown the correct answer after submitting.
        /// Per-question and absent-means-on, matching the web: the field
        /// postdates the questions that do not carry it, and those have always
        /// remediated.
        /// </summary>
        public bool ShowsCorrectAnswer()
        {
            return Questions != null && Questions.Exists(question => question.Remediate);
        }

        /// <summary>
        /// Whether this layer gives the learner something to acknowledge — the
        /// correct/incorrect banner and a Continue button — after Submit.
        ///
        /// A pausing question always does. A non-pausing one does too once the
        /// author turned on Show Correct Answer: there is feedback on screen to
        /// read, and the learner needs a way to say they have read it. Without
        /// this the layer simply vanished when its window ended, taking any
        /// attached action with it.
        /// </summary>
        public bool ShowsAcknowledgement()
        {
            return ShouldPauseForAcknowledgement() || ShowsCorrectAnswer();
        }

        /// <summary>
        /// Whether this layer loops its own window until acknowledged. Offered
        /// only where it means something, mirroring the authoring toggle.
        /// </summary>
        public bool LoopsUntilAcknowledged()
        {
            return LoopUntilAcknowledged
                && !ShouldPauseForAcknowledgement()
                && ShowsCorrectAnswer()
                && EndTime > StartTime;
        }

        public override void SetParentScene(SceneData scene)
        {
            base.SetParentScene(scene);
            Questions.ForEach(question => question.SetParentScene(scene));
            AcknowledgeAction?.SetParentScene(scene);
        }

        public override List<DownloadableFileData> GetDownloadableFiles()
        {
            List<DownloadableFileData> list = base.GetDownloadableFiles();

            list.AddRange(Questions
                .Select(question => question.GetDownloadableFiles())
                .SelectMany(fileList => fileList)
                .ToList());

            // A ShowCard acknowledge action carries its own layer, whose media
            // has to be cached with everything else or the card is empty
            // offline.
            list.AddRange(AcknowledgeAction?.GetDownloadableFiles() ?? new());

            return list.Where(file => file != null).Distinct().ToList();
        }
    }
}
