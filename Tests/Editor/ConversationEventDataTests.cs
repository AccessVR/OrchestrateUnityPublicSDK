using System;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AccessVR.OrchestrateVR.SDK.Tests
{
    /// <summary>
    /// Deserialization contract for the Conversation layer (eventType 8) and its
    /// launch URL. Every test hand-builds JSON the way the server publishes it.
    /// </summary>
    public class ConversationEventDataTests
    {
        private const string CharacterJson = @"{
            ""id"": ""layer-1"", ""eventType"": 8, ""displayType"": 4, ""pausePlayback"": true,
            ""startTime"": 12, ""endTime"": 12, ""title"": ""Talk to Dana"",
            ""character"": {
                ""id"": 42, ""guid"": ""9b3e"", ""name"": ""Dana Ortiz"", ""description"": ""Shift supervisor"",
                ""thumbnailUrl"": ""https://cdn/dana.jpg"", ""thumbnailVideoUrl"": null,
                ""isActive"": true, ""isPublished"": true, ""publishedVersionId"": 917,
                ""launchPath"": ""/experiences/12/scenes/88/layers/layer-1/conversation""
            }
        }";

        private static EventData Make(string json)
        {
            return EventDataFactory.Make(json, null);
        }

        [Test]
        public void TestEventTypeEightIsACharacterLayer()
        {
            var data = Make(CharacterJson);

            Assert.IsInstanceOf<ConversationEventData>(data);
            Assert.AreEqual(8, data.EventType);
        }

        [Test]
        public void TestCharacterHydrates()
        {
            var data = (ConversationEventData) Make(CharacterJson);

            Assert.AreEqual(42, data.Character.Id);
            Assert.AreEqual("Dana Ortiz", data.Character.Name);
            Assert.AreEqual("/experiences/12/scenes/88/layers/layer-1/conversation", data.Character.LaunchPath);
            Assert.IsTrue(data.IsAvailable);
        }

        [Test]
        public void TestDisplayTypeFourIsTablet()
        {
            var data = Make(CharacterJson);

            Assert.AreEqual(DisplayTypeOptions.Tablet, data.DisplayType);
        }

        [Test]
        public void TestPausesOnlyWhenAuthoredTo()
        {
            var pausing = Make(@"{""eventType"": 8, ""pausePlayback"": true, ""startTime"": 0, ""endTime"": 0, ""character"": {""id"": 1, ""launchPath"": ""/x""}}");
            var windowed = Make(@"{""eventType"": 8, ""pausePlayback"": false, ""startTime"": 0, ""endTime"": 10, ""character"": {""id"": 1, ""launchPath"": ""/x""}}");

            Assert.IsTrue(pausing.ShouldPauseForAcknowledgement());
            Assert.IsFalse(windowed.ShouldPauseForAcknowledgement());
        }

        [Test]
        public void TestAutoStartDefaultsOn()
        {
            var auto = (ConversationEventData) Make(CharacterJson);
            var manual = (ConversationEventData) Make(@"{""eventType"": 8, ""autoStart"": false, ""character"": {""id"": 1, ""launchPath"": ""/x""}}");

            Assert.IsTrue(auto.AutoStart);
            Assert.IsFalse(manual.AutoStart);
        }

        [Test]
        public void TestSkippableFollowsCanBeSkipped()
        {
            var skippable = Make(@"{""eventType"": 8, ""pausePlayback"": true, ""canBeSkipped"": true, ""character"": {""id"": 1, ""launchPath"": ""/x""}}");
            var fixedLayer = Make(@"{""eventType"": 8, ""pausePlayback"": true, ""character"": {""id"": 1, ""launchPath"": ""/x""}}");

            Assert.IsTrue(skippable.Skippable);
            Assert.IsFalse(fixedLayer.Skippable);
        }

        [Test]
        public void TestLessonCarriesConversationsAvailable()
        {
            var withFlag = JsonConvert.DeserializeObject<LessonData>(@"{""id"": 1, ""conversationsAvailable"": false, ""scenes"": []}");
            var without = JsonConvert.DeserializeObject<LessonData>(@"{""id"": 1, ""scenes"": []}");

            Assert.IsFalse(withFlag.ConversationsAvailable);
            Assert.IsNull(without.ConversationsAvailable);
        }

        [Test]
        public void TestMissingOrInactiveCharacterIsUnavailable()
        {
            var missing = (ConversationEventData) Make(@"{""eventType"": 8, ""character"": null}");
            var inactive = (ConversationEventData) Make(@"{""eventType"": 8, ""character"": {""id"": 42, ""isActive"": false, ""launchPath"": ""/x""}}");
            var noPath = (ConversationEventData) Make(@"{""eventType"": 8, ""character"": {""id"": 42, ""isActive"": true, ""launchPath"": null}}");

            Assert.IsFalse(missing.IsAvailable);
            Assert.IsFalse(inactive.IsAvailable);
            Assert.IsFalse(noPath.IsAvailable);
        }

        [Test]
        public void TestNoDownloadableFiles()
        {
            var data = Make(CharacterJson);

            Assert.IsEmpty(data.GetDownloadableFiles());
        }

        [Test]
        public void TestLaunchPathCarriesRunVersionAndAssignment()
        {
            var path = ConversationEventData.BuildLaunchPath("/experiences/12/scenes/88/layers/layer-1/conversation", "run-abc", 917, 5);

            Assert.AreEqual("/experiences/12/scenes/88/layers/layer-1/conversation?run=run-abc&lesson_version_id=917&assignment=5", path);
        }

        [Test]
        public void TestLaunchPathOmitsEmptyValuesAndEscapes()
        {
            var path = ConversationEventData.BuildLaunchPath("/l", "run one&two", null, null, "k=1");

            Assert.AreEqual("/l?run=run+one%26two&unique_key=k%3d1", path.ToLowerInvariant());
        }

        [Test]
        public void TestLaunchPathWithoutQueryIsBare()
        {
            Assert.AreEqual("/l", ConversationEventData.BuildLaunchPath("/l", null, null, null));
        }

        [Test]
        public void TestLaunchPathRequiresAPath()
        {
            Assert.Throws<InvalidOperationException>(() => ConversationEventData.BuildLaunchPath(null, "run", null, null));
        }

        [Test]
        public void TestTabletOnAMediaLayerStillDeserializes()
        {
            // The enum value exists for every layer type; a media layer that
            // somehow carries it simply reads Tablet and falls to the HUD prefab.
            var data = Make(@"{""eventType"": 6, ""displayType"": 4}");

            Assert.AreEqual(DisplayTypeOptions.Tablet, data.DisplayType);
        }
    }
}
