using Newtonsoft.Json.Linq;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using Cysharp.Threading.Tasks;

namespace AccessVR.OrchestrateVR.SDK.Tests
{
    public class Config
    {
        [JsonProperty("environment")] private string _environment;
        [JsonProperty("userId")] public int UserId;
        [JsonProperty("userName")] public string UserName;
        [JsonProperty("authToken")] public string AuthToken;
        [JsonProperty("baseUrl")] public string BaseUrl;
        [JsonProperty("cdnUrl")] public string CdnUrl;
        [JsonProperty("lesson")] public LessonDataLookup Lesson;
        
        public Environment Environment;
        
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            Environment = (Environment) System.Enum.Parse(typeof(Environment), _environment);
        }
    }
    
    public class OrchestrateTests
    {
        private static Config _config;

        private static Config Config => _config ??= JToken.Parse(ReadTestData("test.config.json")).ToObject<Config>();
        
        private static string ReadTestData(string name)
        {
            return File.ReadAllText(Path.Combine(
                Application.dataPath, 
                "Editor", 
                "TestData", 
                "com.accessvr.orchestratevr.sdk", 
                name
            ));
        }

        public OrchestrateTests()
        {
            Orchestrate.Logout();
            Orchestrate.SetEnvironment(Config.Environment); 
            Orchestrate.SetAuthToken(Config.AuthToken);
        }
        
        [Test]
        public void TestConfig()
        {
            Assert.AreEqual(Config.Environment, Orchestrate.GetEnvironment());;
            Assert.AreEqual("Local", Config.Environment.ToString());
            Assert.AreEqual("https://ovr.avr.ngrok.io", Orchestrate.GetBaseUrl());
            Assert.IsNotEmpty(Config.AuthToken);
            Assert.AreEqual(Config.AuthToken, Orchestrate.GetAuthToken());
            Assert.IsNotEmpty(Config.Lesson.Guid);
            Assert.IsNotEmpty(Config.Lesson.EmbedKey);
            Assert.IsTrue(Config.Lesson.Id > 0);
        }

        [UnityTest]
        public IEnumerator TestLoadUser() => UniTask.ToCoroutine(async () =>
        {
            UserData user = await Orchestrate.LoadUser();
            Assert.AreEqual(Config.UserId, user.UserId);
            Assert.AreEqual(Config.UserName, user.UserName);
        });

        private void AssertValidLesson(LessonData lesson)
        {
            Assert.AreEqual(Config.Lesson.Guid, lesson.Guid);
            
            // Scene count
            Assert.AreEqual(5, lesson.Scenes.Count);
            
            // Scene names
            Assert.AreEqual("Start", lesson.InitialScene.Name);
            Assert.AreEqual("Education", lesson.Scenes[1].Name);
            Assert.AreEqual("Law Enforcement", lesson.Scenes[2].Name);
            Assert.AreEqual("Military", lesson.Scenes[3].Name);
            Assert.AreEqual("Healthcare", lesson.Scenes[4].Name);
            
            // Scene end action type
            Assert.AreEqual(ActionType.CompleteLesson, lesson.InitialScene.EndAction.Type);
            
            // Scene initial view
            AssertUtils.AreApproximatelyEqual(new Quaternion(0.01845f, 0.98091f, 0.11783f, -0.15361f), lesson.InitialScene.InitialView.Quaternion);
            
            // Scene skybox
            Assert.IsTrue(lesson.InitialScene.Skybox.IsVideo());
            Assert.AreEqual("1d2ed0cc-8e64-439f-9448-e4cdc8e0b0f2", lesson.InitialScene.Skybox.FileData.Guid);
            Assert.AreEqual(Orchestrate.GetCdnUrl("local/1d2ed0cc-8e64-439f-9448-e4cdc8e0b0f2/Beach-4K.mp4"), lesson.InitialScene.Skybox.FileData.Url);
            Assert.AreEqual(lesson.Guid, lesson.InitialScene.Skybox.FileData.Parent.Guid);
            
            // Timed events
            Assert.AreEqual(5, lesson.InitialScene.TimedEvents.Count);
            
            // Text event
            EventData textEventData = lesson.InitialScene.SortedTimedEvents[0];
            Assert.AreEqual(typeof(InfoEventData), textEventData.GetType());
            Assert.IsTrue(textEventData.Description.Contains("<b>Welcome</b>"));
            Assert.AreEqual("b50b526e-32d5-43fc-ade6-45d3f3f6f576", textEventData.Id);
            Assert.AreEqual(4.0f, textEventData.StartTime);
            Assert.AreEqual(9.0f, textEventData.EndTime);
            Assert.AreEqual(CardSizeOptions.Small, textEventData.CardSize);
            Assert.AreEqual(Positioning.MiddleCenter, textEventData.Position);
            Assert.AreEqual(DisplayTypeOptions.HUD, textEventData.DisplayType);;
            Assert.AreEqual("Continue", textEventData.ButtonText);
            
            // Media event
            EventData mediaEventData = lesson.InitialScene.SortedTimedEvents[1];
            Assert.AreEqual(typeof(MediaEventData), mediaEventData.GetType());
            Assert.IsTrue(mediaEventData.Asset.IsImage());
            Assert.AreEqual("7974964c-adef-4ff5-8096-471c56dffc09", mediaEventData.Asset.FileData.Guid);
            Assert.AreEqual(Orchestrate.GetCdnUrl("local/7974964c-adef-4ff5-8096-471c56dffc09/360-icon_editor.jpg"), mediaEventData.Asset.FileData.Url);
            Assert.AreEqual(lesson.Guid, mediaEventData.Asset.FileData.Parent.Guid);
            Assert.IsFalse(mediaEventData.MuteAudio);
            
            // Hotspot event
            HotspotEventData hotspotEventData = (HotspotEventData) lesson.InitialScene.SortedTimedEvents[3];
            Assert.AreEqual(typeof(HotspotEventData), hotspotEventData.GetType());
            Assert.AreEqual(5, hotspotEventData.Hotspots.Count);
            Assert.IsTrue(hotspotEventData.PausesPlayback());
            
            HotspotData healthcareHotspot = hotspotEventData.Hotspots[0];
            Assert.AreEqual("Healthcare", healthcareHotspot.Name);
            Assert.AreEqual(ActionType.GoToScene, healthcareHotspot.Action.Type);
            Assert.AreEqual(lesson.Scenes[4].Id, healthcareHotspot.Action.SceneId);
            Assert.AreEqual("9beac816-5aba-463c-96d4-1943005cf35c", healthcareHotspot.Id);
            Assert.AreEqual("f82f", healthcareHotspot.Icon?.Unicode);
            AssertUtils.AreApproximatelyEqual(new Vector3(0.23123f, 0.09325f, 3.46305f), healthcareHotspot.Position);

            HotspotData exitHotspot = hotspotEventData.Hotspots[4];
            Assert.AreEqual("Exit Experience", exitHotspot.Name);
            Assert.AreEqual(ActionType.CompleteLesson, exitHotspot.Action.Type);
            Assert.AreEqual("f52b", exitHotspot.Icon?.Unicode);
            
            // Question event
            QuestionEventData questionEventData = (QuestionEventData) lesson.Scenes[1].SortedTimedEvents[4];
            Assert.AreEqual(typeof(QuestionEventData), questionEventData.GetType());
            Assert.AreEqual(1, questionEventData.Questions.Count);
            QuestionData question = questionEventData.Questions[0];
            Assert.AreEqual("84af5141-2eb8-4bc7-b26f-cdcd579a2f77", question.Id);
            Assert.AreEqual(2, question.Answers.Count);
            
            // Question answers
            Assert.AreEqual("5035b7f8-b6f4-41c6-8e77-59084bf3f48d", question.Answers[0].Id);
            Assert.IsTrue(question.Answers[0].IsCorrect);
            Assert.AreEqual(ActionType.GoToScene, question.Answers[0].Action.Type);
            Assert.AreEqual("Yes", question.Answers[0].Text);
            
            Assert.AreEqual("c61b836f-909a-4dda-a97a-3603a5bcf8bd", question.Answers[1].Id);
            Assert.IsFalse(question.Answers[1].IsCorrect);
            Assert.AreEqual(ActionType.CompleteLesson, question.Answers[1].Action.Type);
            Assert.AreEqual("No", question.Answers[1].Text);
        }

        [UnityTest]
        public IEnumerator TestLoadLesson() => UniTask.ToCoroutine(async () =>
        {
            // Test direct download 
            LessonData lesson = await Orchestrate.LoadLesson(Config.Lesson);
            AssertValidLesson(lesson);

            // Test load from cache (file created in direct download)
            lesson = Orchestrate.LoadCachedLesson(Config.Lesson.Guid);
            AssertValidLesson(lesson);
        });

        [UnityTest]
        public IEnumerator TestDownloads() => UniTask.ToCoroutine(async () =>
        {
            Orchestrate.DeleteCachedLesson(Config.Lesson.Guid, true);
            Assert.IsFalse(Orchestrate.CacheContainsLesson(Config.Lesson.Guid));
            LessonData lesson = await Orchestrate.LoadLesson(Config.Lesson);
            DownloadJob job = await Downloader.Download(lesson);
            Assert.IsTrue(job.IsComplete);
        });
    }
} 