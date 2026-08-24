using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace AccessVR.OrchestrateVR.SDK.Tests
{
    public class UserDataAnalyticsTests
    {
        [Test]
        public void TestManifestIdentityDeserializes()
        {
            var user = JsonConvert.DeserializeObject<UserData>(@"{
                ""userId"": 42,
                ""userName"": ""learner"",
                ""organizationId"": 7,
                ""organizationName"": ""Acme Training"",
                ""abxr"": {
                    ""enabled"": true,
                    ""appId"": ""app-1"",
                    ""orgId"": ""org-1"",
                    ""authSecret"": ""secret"",
                    ""baseUrl"": ""https://app.orchestratevr.com""
                }
            }");
            Assert.AreEqual(7, user.OrganizationId);
            Assert.AreEqual("Acme Training", user.OrganizationName);
            Assert.IsTrue(user.Abxr.IsUsable);
            Assert.AreEqual("https://app.orchestratevr.com", user.Abxr.BaseUrl);
        }

        [Test]
        public void TestLegacyManifestLeavesAnalyticsIdentityNull()
        {
            var user = JsonConvert.DeserializeObject<UserData>(@"{""userId"": 42, ""userName"": ""learner""}");
            Assert.IsFalse(user.OrganizationId.HasValue);
            Assert.IsNull(user.Abxr);
        }

        [Test]
        public void TestDisabledOrIncompleteCredentialsAreNotUsable()
        {
            var disabled = JsonConvert.DeserializeObject<AbxrConfigData>(@"{""enabled"": false, ""appId"": ""a"", ""orgId"": ""o"", ""authSecret"": ""s""}");
            Assert.IsFalse(disabled.IsUsable);

            var missingSecret = JsonConvert.DeserializeObject<AbxrConfigData>(@"{""enabled"": true, ""appId"": ""a"", ""orgId"": ""o""}");
            Assert.IsFalse(missingSecret.IsUsable);
        }
    }

    public class EventTypeAndActionRawTests
    {
        [Test]
        public void TestRawEventTypeIsMapped()
        {
            var media = (MediaEventData) EventDataFactory.Make(@"{""eventType"": 6}", null);
            Assert.AreEqual(6, media.EventType);

            var legacyVideo = (MediaEventData) EventDataFactory.Make(@"{""eventType"": 4}", null);
            Assert.AreEqual(4, legacyVideo.EventType);
        }

        [Test]
        public void TestActionRawTypeExposed()
        {
            var action = JsonConvert.DeserializeObject<ActionData>(@"{""type"": 3, ""sceneId"": 9}");
            Assert.AreEqual(3, action.RawType);
            Assert.AreEqual(ActionType.GoToScene, action.Type);
        }

        [Test]
        public void TestAssetIdMapped()
        {
            var asset = JsonConvert.DeserializeObject<AssetData>(@"{""id"": 314, ""assetTypeId"": 5}");
            Assert.AreEqual(314, asset.Id);

            var legacy = JsonConvert.DeserializeObject<AssetData>(@"{""assetTypeId"": 5}");
            Assert.IsFalse(legacy.Id.HasValue);
        }
    }

    public class LessonVersionIdentityTests
    {
        [Test]
        public void TestVersionIdentityRoundTripsThroughSerialization()
        {
            // The lesson cache is the re-serialized LessonData; version
            // identity and the preview flag must survive it so an offline
            // launch still knows what it is playing.
            var lesson = new LessonData
            {
                Id = 12,
                Guid = "abc",
                PublishedLessonId = 88,
                PublishedVersionNumber = 7,
                IsPreview = true,
            };

            var restored = JsonConvert.DeserializeObject<LessonData>(JsonConvert.SerializeObject(lesson));
            Assert.AreEqual(88, restored.PublishedLessonId);
            Assert.AreEqual(7, restored.PublishedVersionNumber);
            Assert.IsTrue(restored.IsPreview);
        }

        [Test]
        public void TestLegacyCachePayloadLeavesVersionNull()
        {
            var restored = JsonConvert.DeserializeObject<LessonData>(@"{""id"": 12, ""guid"": ""abc""}");
            Assert.IsFalse(restored.PublishedLessonId.HasValue);
            Assert.IsFalse(restored.PublishedVersionNumber.HasValue);
            Assert.IsFalse(restored.IsPreview);
        }

        [Test]
        public void TestDraftPreviewWithNullDatesDeserializes()
        {
            // A draft previewed before its first publish sends explicit nulls
            // for its dates; this exact payload shape used to throw and kill
            // the whole lesson load for preview launch codes.
            var restored = JsonConvert.DeserializeObject<LessonData>(
                @"{""id"": 483, ""guid"": ""abc"", ""publishedDate"": null, ""startDate"": null, ""completeDate"": null}");

            Assert.AreEqual(483, restored.Id);
            Assert.IsFalse(restored.PublishedDate.HasValue);
            Assert.IsFalse(restored.StartDate.HasValue);
            Assert.IsFalse(restored.CompleteDate.HasValue);
        }
    }

    public class SubmissionDataAnalyticsTests
    {
        [Test]
        public void TestSubmissionCarriesRunAndVersion()
        {
            var lesson = new LessonData { Id = 12, PublishedLessonId = 88 };
            var user = new UserData { UserId = 42 };

            var submission = SubmissionData.Make(lesson, 5, user, System.DateTime.UtcNow, 0.85f, "run-uuid");
            string json = JsonConvert.SerializeObject(submission);
            var wire = JObject.Parse(json);

            Assert.AreEqual("run-uuid", wire["abxrRunId"]?.ToString());
            Assert.AreEqual(88, wire["lessonVersionId"]?.Value<int>());
            Assert.AreEqual(JTokenType.Null, wire["sessionId"].Type);
            Assert.IsFalse(wire["exited"].Value<bool>());
        }

        [Test]
        public void TestUnlistedLessonKeyRidesTheSubmission()
        {
            // The server authorizes play on unlisted content by the share
            // key; a submission without it is refused (403) and the run's
            // session never records.
            var user = new UserData { UserId = 42 };

            var keyed = SubmissionData.Make(new LessonData { Id = 12, UniqueKey = "share-key" }, 0, user,
                System.DateTime.UtcNow, 0.5f);
            var wire = JObject.Parse(JsonConvert.SerializeObject(keyed));
            Assert.AreEqual("share-key", wire["uniqueKey"]?.ToString());

            var unkeyed = SubmissionData.Make(new LessonData { Id = 12 }, 0, user, System.DateTime.UtcNow, 0.5f);
            Assert.IsNull(JObject.Parse(JsonConvert.SerializeObject(unkeyed))["uniqueKey"]);
        }

        [Test]
        public void TestExitedRunMarksSubmission()
        {
            var lesson = new LessonData { Id = 12 };
            var user = new UserData { UserId = 42 };

            var submission = SubmissionData.Make(lesson, 0, user, System.DateTime.UtcNow, 0.5f, "run-uuid");
            submission.Exited = true;
            var wire = JObject.Parse(JsonConvert.SerializeObject(submission));

            Assert.IsTrue(wire["exited"].Value<bool>());
        }

        [Test]
        public void TestUntrackedRunOmitsRunId()
        {
            var lesson = new LessonData { Id = 12 };
            var user = new UserData { UserId = 42 };

            var submission = SubmissionData.Make(lesson, 0, user, System.DateTime.UtcNow, -1f);
            var wire = JObject.Parse(JsonConvert.SerializeObject(submission));

            Assert.IsNull(wire["abxrRunId"]);
            Assert.AreEqual(JTokenType.Null, wire["lessonVersionId"].Type);
        }
    }

    public class OXRDurableQueueTests
    {
        private string _dir;
        private string _path;

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), "oxr-queue-tests-" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
            _path = Path.Combine(_dir, "test.queue.jsonl");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_dir))
            {
                Directory.Delete(_dir, recursive: true);
            }
        }

        [Test]
        public void TestAppendAndReadBack()
        {
            var queue = new OXRDurableQueue(_path);
            queue.Append(@"{""seq"":1,""kind"":""event""}");
            queue.Append(@"{""seq"":2,""kind"":""event""}");

            var records = queue.ReadAll();
            Assert.AreEqual(2, records.Count);
            Assert.AreEqual(1, JObject.Parse(records[0])["seq"].Value<int>());
            Assert.AreEqual(2, JObject.Parse(records[1])["seq"].Value<int>());
        }

        [Test]
        public void TestSurvivesReopen()
        {
            // The whole point: a new instance over the same file — an app
            // restart — sees everything.
            new OXRDurableQueue(_path).Append(@"{""seq"":1}");
            Assert.AreEqual(1, new OXRDurableQueue(_path).Count());
        }

        [Test]
        public void TestCorruptedLineIsSkippedNotFatal()
        {
            var queue = new OXRDurableQueue(_path);
            queue.Append(@"{""seq"":1}");
            // Simulate a torn write from a crash mid-append.
            File.AppendAllText(_path, "{\"seq\":2,\"kin");
            queue.Append(@"{""seq"":3}");

            var records = queue.ReadAll();
            Assert.AreEqual(2, records.Count);
            Assert.AreEqual(1, JObject.Parse(records[0])["seq"].Value<int>());
            Assert.AreEqual(3, JObject.Parse(records[1])["seq"].Value<int>());
        }

        [Test]
        public void TestRemoveFirstKeepsTheRest()
        {
            var queue = new OXRDurableQueue(_path);
            for (int i = 1; i <= 5; i++)
            {
                queue.Append($@"{{""seq"":{i}}}");
            }
            queue.RemoveFirst(3);

            var records = queue.ReadAll();
            Assert.AreEqual(2, records.Count);
            Assert.AreEqual(4, JObject.Parse(records[0])["seq"].Value<int>());
        }

        [Test]
        public void TestRemoveAllDeletesFile()
        {
            var queue = new OXRDurableQueue(_path);
            queue.Append(@"{""seq"":1}");
            queue.RemoveFirst(99);
            Assert.IsFalse(File.Exists(_path));
        }

        [Test]
        public void TestBoundsDropOldest()
        {
            var queue = new OXRDurableQueue(_path, maxEntries: 3);
            for (int i = 1; i <= 5; i++)
            {
                queue.Append($@"{{""seq"":{i}}}");
            }

            var records = queue.ReadAll();
            Assert.AreEqual(3, records.Count);
            Assert.AreEqual(3, JObject.Parse(records[0])["seq"].Value<int>());
            Assert.AreEqual(5, JObject.Parse(records[2])["seq"].Value<int>());
        }

        [Test]
        public void TestEmbeddedNewlinesAreFlattened()
        {
            var queue = new OXRDurableQueue(_path);
            queue.Append("{\"text\":\"line one\nline two\"}");
            Assert.AreEqual(1, queue.ReadAll().Count);
        }
    }
}
