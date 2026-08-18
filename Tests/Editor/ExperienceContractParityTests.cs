using System.Globalization;
using System.Threading;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

namespace AccessVR.OrchestrateVR.SDK.Tests
{
    /// <summary>
    /// Deserialization contract for the 2026 web experience-layer parity
    /// release: worldspace transforms on Question/Info events, displayType
    /// Hotspots, hidden hotspots, question pause opt-out, preserveView,
    /// spatial tri-state, and CSS color parsing. Every test hand-builds JSON
    /// the way the web editor writes it (MediaEventDataTests pattern).
    /// </summary>
    public class QuestionEventDataPauseTests
    {
        private static QuestionEventData Make(string json, bool isActionEvent = false)
        {
            return (QuestionEventData) EventDataFactory.Make(json, null, isActionEvent);
        }

        [Test]
        public void TestLegacyQuestionAlwaysPauses()
        {
            // Every legacy question stores a meaningless pausePlayback:false.
            // Without the sentinel it must keep pausing.
            var data = Make(@"{""eventType"": 3, ""pausePlayback"": false, ""startTime"": 0, ""endTime"": 10, ""questions"": []}");
            Assert.IsTrue(data.ShouldPauseForAcknowledgement());
        }

        [Test]
        public void TestConfiguredOptOutDoesNotPause()
        {
            var data = Make(@"{""eventType"": 3, ""_pausePlaybackConfigured"": true, ""pausePlayback"": false, ""startTime"": 0, ""endTime"": 10, ""questions"": []}");
            Assert.IsFalse(data.ShouldPauseForAcknowledgement());
        }

        [Test]
        public void TestConfiguredPauseStillPauses()
        {
            var data = Make(@"{""eventType"": 3, ""_pausePlaybackConfigured"": true, ""pausePlayback"": true, ""startTime"": 0, ""endTime"": 10, ""questions"": []}");
            Assert.IsTrue(data.ShouldPauseForAcknowledgement());
        }

        [Test]
        public void TestInvalidTimeWindowForcesPause()
        {
            // EndTime < StartTime forces pausePlayback in the base class:
            // only a valid time window can auto-dismiss a non-pausing event.
            var data = Make(@"{""eventType"": 3, ""_pausePlaybackConfigured"": true, ""pausePlayback"": false, ""startTime"": 5, ""endTime"": 0, ""questions"": []}");
            Assert.IsTrue(data.ShouldPauseForAcknowledgement());
        }

        [Test]
        public void TestActionEventAlwaysPauses()
        {
            var data = Make(@"{""eventType"": 3, ""_pausePlaybackConfigured"": true, ""pausePlayback"": false, ""startTime"": 0, ""endTime"": 10, ""questions"": []}", isActionEvent: true);
            Assert.IsTrue(data.ShouldPauseForAcknowledgement());
        }
    }

    public class QuestionEventDataWorldspaceTests
    {
        [Test]
        public void TestWorldspaceQuestionDeserialize()
        {
            var data = (QuestionEventData) EventDataFactory.Make(@"{
                ""eventType"": 3,
                ""displayType"": 1,
                ""position"": {""X"": 1.5, ""Y"": 0.25, ""Z"": -3.0},
                ""rotation"": {""X"": 0.0, ""Y"": 45.0, ""Z"": 0.0},
                ""scale"": {""X"": 2.0, ""Y"": 2.0, ""Z"": 1.0},
                ""spriteMode"": false,
                ""questions"": []
            }", null);
            Assert.AreEqual(DisplayTypeOptions.WorldSpace, data.DisplayType);
            AssertUtils.AreApproximatelyEqual(new Vector3(1.5f, 0.25f, -3.0f), data.Position.Value);
            AssertUtils.AreApproximatelyEqual(new Vector3(0f, 45f, 0f), data.Rotation);
            AssertUtils.AreApproximatelyEqual(new Vector3(2f, 2f, 1f), data.Scale.Value);
            Assert.IsFalse(data.SpriteMode);
        }

        [Test]
        public void TestDisplayTypeHotspotsDeserialize()
        {
            var data = (QuestionEventData) EventDataFactory.Make(@"{""eventType"": 3, ""displayType"": 3, ""questions"": []}", null);
            Assert.AreEqual(DisplayTypeOptions.Hotspots, data.DisplayType);
        }

        [Test]
        public void TestDisplayTypeAbsentIsHud()
        {
            var data = (QuestionEventData) EventDataFactory.Make(@"{""eventType"": 3, ""questions"": []}", null);
            Assert.AreEqual(DisplayTypeOptions.HUD, data.DisplayType);
            Assert.IsFalse(data.Position.HasValue);
            Assert.IsTrue(data.SpriteMode);
        }

        [Test]
        public void TestAuthoringOnlyKeysAreIgnored()
        {
            // locked and _worldspaceInitialized are editor-only; they must
            // ride through deserialization without error and without mapping.
            var data = (QuestionEventData) EventDataFactory.Make(@"{
                ""eventType"": 3,
                ""locked"": true,
                ""_worldspaceInitialized"": true,
                ""questions"": []
            }", null);
            Assert.IsNotNull(data);
        }
    }

    public class QuestionDataHotspotTests
    {
        [Test]
        public void TestVisibilityAbsentIsVisible()
        {
            var q = JsonConvert.DeserializeObject<QuestionData>(@"{""id"": ""q1"", ""answers"": [{""id"": ""a1""}]}");
            Assert.AreEqual(HotspotVisibilityOptions.Visible, q.HotspotVisibility);
        }

        [Test]
        public void TestVisibilityHidden()
        {
            var q = JsonConvert.DeserializeObject<QuestionData>(@"{""id"": ""q1"", ""hotspotVisibility"": ""hidden"", ""answers"": [{""id"": ""a1""}]}");
            Assert.AreEqual(HotspotVisibilityOptions.Hidden, q.HotspotVisibility);
        }

        [Test]
        public void TestShowLabelsAbsentIsTrue()
        {
            var q = JsonConvert.DeserializeObject<QuestionData>(@"{""id"": ""q1"", ""answers"": [{""id"": ""a1""}]}");
            Assert.IsTrue(q.ShowLabels);
        }

        [Test]
        public void TestShowLabelsFalse()
        {
            var q = JsonConvert.DeserializeObject<QuestionData>(@"{""id"": ""q1"", ""showLabels"": false, ""answers"": [{""id"": ""a1""}]}");
            Assert.IsFalse(q.ShowLabels);
        }
    }

    public class AnswerDataHotspotTests
    {
        [Test]
        public void TestHotspotAnswerDeserialize()
        {
            var a = JsonConvert.DeserializeObject<AnswerData>(@"{
                ""id"": ""a1"",
                ""answerText"": ""The fire extinguisher"",
                ""correctAnswer"": true,
                ""position"": {""X"": -2.0, ""Y"": 1.0, ""Z"": -4.0},
                ""icon"": {""id"": ""check"", ""font"": ""solid"", ""unicode"": ""f00c""},
                ""backgroundColor"": ""#111827"",
                ""iconColor"": ""#FFFFFF""
            }");
            AssertUtils.AreApproximatelyEqual(new Vector3(-2f, 1f, -4f), a.Position.Value);
            Assert.AreEqual("check", a.Icon.Id);
            Assert.AreEqual("solid", a.Icon.Font);
            Assert.AreEqual("f00c", a.Icon.Unicode);
            Assert.IsTrue(a.BackgroundColor.HasValue);
            Assert.IsTrue(a.IconColor.HasValue);
        }

        [Test]
        public void TestLegacyAnswerLeavesHotspotFieldsNull()
        {
            // Renderer owns the fallbacks ({0,0,-3}, check glyph, #111827,
            // #FFFFFF); the data layer must report absence honestly.
            var a = JsonConvert.DeserializeObject<AnswerData>(@"{""id"": ""a1"", ""answerText"": ""Yes"", ""correctAnswer"": false}");
            Assert.IsFalse(a.Position.HasValue);
            Assert.IsNull(a.Icon);
            Assert.IsFalse(a.BackgroundColor.HasValue);
            Assert.IsFalse(a.IconColor.HasValue);
        }
    }

    public class HotspotEventDataVisibilityTests
    {
        private const string HiddenLayerJson = @"{
            ""eventType"": 5,
            ""hotspotVisibility"": ""hidden"",
            ""alwaysShowLabels"": true,
            ""hotspots"": [{""id"": ""h1"", ""name"": ""Valve"", ""position"": {""X"": 1, ""Y"": 0, ""Z"": -2}}]
        }";

        [Test]
        public void TestHiddenPropagatesToChildren()
        {
            var data = (HotspotEventData) EventDataFactory.Make(HiddenLayerJson, null);
            Assert.AreEqual(HotspotVisibilityOptions.Hidden, data.HotspotVisibility);
            Assert.AreEqual(HotspotVisibilityOptions.Hidden, data.Hotspots[0].Visibility);
        }

        [Test]
        public void TestHiddenForcesLabelsOffDespiteAlwaysShowLabels()
        {
            // A stored alwaysShowLabels:true must not leak a hidden hotspot
            // into view (web effectiveAlwaysShowLabels rule).
            var data = (HotspotEventData) EventDataFactory.Make(HiddenLayerJson, null);
            Assert.IsFalse(data.Hotspots[0].AlwaysShowLabel);
        }

        [Test]
        public void TestVisibilityAbsentKeepsLegacyBehavior()
        {
            var data = (HotspotEventData) EventDataFactory.Make(@"{
                ""eventType"": 5,
                ""alwaysShowLabels"": true,
                ""hotspots"": [{""id"": ""h1"", ""name"": ""Valve"", ""position"": {""X"": 1, ""Y"": 0, ""Z"": -2}}]
            }", null);
            Assert.AreEqual(HotspotVisibilityOptions.Visible, data.HotspotVisibility);
            Assert.AreEqual(HotspotVisibilityOptions.Visible, data.Hotspots[0].Visibility);
            Assert.IsTrue(data.Hotspots[0].AlwaysShowLabel);
        }

        [Test]
        public void TestHotspotIconColorParses()
        {
            var data = (HotspotEventData) EventDataFactory.Make(@"{
                ""eventType"": 5,
                ""hotspots"": [{""id"": ""h1"", ""name"": ""Valve"", ""position"": {""X"": 0, ""Y"": 0, ""Z"": -2}, ""iconColor"": ""#FF6600""}]
            }", null);
            Assert.IsTrue(data.Hotspots[0].IconColor.HasValue);
        }
    }

    public class ActionDataPreserveViewTests
    {
        [Test]
        public void TestPreserveViewAbsentIsFalse()
        {
            var action = JsonConvert.DeserializeObject<ActionData>(@"{""type"": 3, ""sceneId"": 42}");
            Assert.IsFalse(action.PreserveView);
            Assert.AreEqual(ActionType.GoToScene, action.Type);
        }

        [Test]
        public void TestPreserveViewTrue()
        {
            var action = JsonConvert.DeserializeObject<ActionData>(@"{""type"": 5, ""preserveView"": true}");
            Assert.IsTrue(action.PreserveView);
            Assert.AreEqual(ActionType.ReplayCurrentScene, action.Type);
        }
    }

    public class MediaEventDataSpatialTests
    {
        [Test]
        public void TestSpatialAbsentIsNull()
        {
            // Legacy payload: renderer falls back to the Position.HasValue proxy.
            var data = JsonConvert.DeserializeObject<MediaEventData>(@"{""displayType"": 2}");
            Assert.IsFalse(data.Spatial.HasValue);
        }

        [Test]
        public void TestSpatialFalseWithStrayPosition()
        {
            // The editor deletes position when spatial is off, but a stray
            // position must still deserialize alongside the authoritative
            // spatial:false — the renderer resolves the conflict.
            var data = JsonConvert.DeserializeObject<MediaEventData>(@"{
                ""displayType"": 2,
                ""spatial"": false,
                ""position"": {""X"": 0, ""Y"": 0, ""Z"": -3}
            }");
            Assert.IsFalse(data.Spatial.Value);
            Assert.IsTrue(data.Position.HasValue);
        }

        [Test]
        public void TestSpatialTrue()
        {
            var data = JsonConvert.DeserializeObject<MediaEventData>(@"{
                ""displayType"": 2,
                ""spatial"": true,
                ""position"": {""X"": 0, ""Y"": 0, ""Z"": -3}
            }");
            Assert.IsTrue(data.Spatial.Value);
        }
    }

    public class EventDataBackdropBlurTests
    {
        [Test]
        public void TestBackdropBlurAbsentIsTrue()
        {
            var data = JsonConvert.DeserializeObject<MediaEventData>(@"{""displayType"": 0}");
            Assert.IsTrue(data.BackdropBlur);
        }

        [Test]
        public void TestBackdropBlurFalse()
        {
            var data = JsonConvert.DeserializeObject<MediaEventData>(@"{""displayType"": 0, ""backdropBlur"": false}");
            Assert.IsFalse(data.BackdropBlur);
        }
    }

    public class StringUtilsColorTests
    {
        [Test]
        public void TestHexForms()
        {
            Assert.IsTrue(StringUtils.ConvertToColor("#FFF").HasValue);
            Assert.IsTrue(StringUtils.ConvertToColor("#FFF8").HasValue);
            Assert.IsTrue(StringUtils.ConvertToColor("#16A34A").HasValue);
        }

        [Test]
        public void TestHexAlphaPreserved()
        {
            // The hidden-hotspot acknowledged disc derives its translucency
            // from the background color's alpha channel.
            Color c = StringUtils.ConvertToColor("#11182780").Value;
            Assert.AreEqual(0x80 / 255f, c.a, 0.01f);
        }

        [Test]
        public void TestRgbForms()
        {
            Color c = StringUtils.ConvertToColor("rgb(255, 0, 0)").Value;
            Assert.AreEqual(1f, c.r, 0.001f);
            Assert.AreEqual(1f, c.a, 0.001f);

            Color halfAlpha = StringUtils.ConvertToColor("rgba(255,0,0,0.5)").Value;
            Assert.AreEqual(0.5f, halfAlpha.a, 0.001f);
        }

        [Test]
        public void TestRgba255StyleAlphaTolerated()
        {
            Color c = StringUtils.ConvertToColor("rgba(255,0,0,128)").Value;
            Assert.AreEqual(128f / 255f, c.a, 0.01f);
        }

        [Test]
        public void TestGarbageReturnsNullNotTransparentBlack()
        {
            Assert.IsFalse(StringUtils.ConvertToColor("not-a-color").HasValue);
            Assert.IsFalse(StringUtils.ConvertToColor("#GGGGGG").HasValue);
            Assert.IsFalse(StringUtils.ConvertToColor("rgb(banana)").HasValue);
            Assert.IsFalse(StringUtils.ConvertToColor("").HasValue);
            Assert.IsFalse(StringUtils.ConvertToColor(null).HasValue);
        }

        [Test]
        public void TestParsesUnderCommaDecimalCulture()
        {
            CultureInfo original = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
                Color c = StringUtils.ConvertToColor("rgba(255,0,0,0.5)").Value;
                Assert.AreEqual(0.5f, c.a, 0.001f);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = original;
            }
        }
    }
}
