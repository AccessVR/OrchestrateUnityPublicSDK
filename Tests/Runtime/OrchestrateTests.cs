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
        [JsonProperty("environmentName")] private string _env;
        [JsonProperty("userId")] public int UserId;
        [JsonProperty("userName")] public string UserName;
        [JsonProperty("authToken")] public string AuthToken;
        [JsonProperty("baseUrl")] public string BaseUrl;
        [JsonProperty("cdnUrl")] public string CdnUrl;
        [JsonProperty("lesson")] public LessonDataLookup Lesson;
        
        public Environment EnvironmentName;
        
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            EnvironmentName = (Environment) System.Enum.Parse(typeof(Environment), _env);
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
            Orchestrate.SetEnvironmentName(Config.EnvironmentName); 
            Orchestrate.SetAuthToken(Config.AuthToken);
        }
        
        [Test]
        public void TestConfig()
        {
            Assert.AreEqual("Local", Config.EnvironmentName.ToString());
            Assert.AreEqual("https://ovr.avr.ngrok.io", Orchestrate.GetBaseUrl());
            Assert.IsNotEmpty(Config.AuthToken);
            Assert.AreEqual(Config.AuthToken, Orchestrate.GetAuthToken());
            Assert.IsNotEmpty(Config.Lesson.Guid);
            Assert.IsNotEmpty(Config.Lesson.EmbedKey);
            Assert.IsTrue(Config.Lesson.Id > 0);
        }

        [UnityTest]
        public IEnumerator TestGetUser() => UniTask.ToCoroutine(async () =>
        {
            UserData user = await Orchestrate.CreateClient().GetUser();
            Assert.AreEqual(Config.UserId, user.UserId);
            Assert.AreEqual(Config.UserName, user.UserName);
        });

        private void AssertValidLesson(LessonData lesson)
        {
            Assert.AreEqual(Config.Lesson.Guid, lesson.Guid);
            
            // Scene count
            Assert.AreEqual(lesson.Scenes.Count, 5);
            
            SceneData scene1 = lesson.Scenes[0];
            
            // Scene names
            Assert.AreEqual("Start", scene1.Name);
            Assert.AreEqual("Education", lesson.Scenes[1].Name);
            Assert.AreEqual("Law Enforcement", lesson.Scenes[2].Name);
            Assert.AreEqual("Military", lesson.Scenes[3].Name);
            Assert.AreEqual("Healthcare", lesson.Scenes[4].Name);
            
            // Scene end action type
            Assert.AreEqual(ActionType.CompleteLesson, scene1.EndAction.Type);
            
            // Scene initial view
            AssertUtils.AreApproximatelyEqual(new Quaternion(0.01845f, 0.98091f, 0.11783f, -0.15361f), scene1.InitialView.Quaternion);
            
            // Scene skybox
            Assert.IsTrue(scene1.Skybox.IsVideo());
            Assert.AreEqual("1d2ed0cc-8e64-439f-9448-e4cdc8e0b0f2", scene1.Skybox.FileData.Guid);
            Assert.AreEqual(Orchestrate.CdnUrl("local/1d2ed0cc-8e64-439f-9448-e4cdc8e0b0f2/Beach-4K.mp4"), scene1.Skybox.FileData.Url);
            Assert.AreEqual(lesson.Guid, scene1.Skybox.FileData.Parent.Guid);
            
            // Timed events
            Assert.AreEqual(5, scene1.TimedEvents.Count);
            
            // Text event
            EventData textEventData = scene1.SortedTimedEvents[0];
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
            EventData mediaEventData = scene1.SortedTimedEvents[1];
            Assert.AreEqual(typeof(MediaEventData), mediaEventData.GetType());
            Assert.IsTrue(mediaEventData.Asset.IsImage());
            Assert.AreEqual("7974964c-adef-4ff5-8096-471c56dffc09", mediaEventData.Asset.FileData.Guid);
            Assert.AreEqual(Orchestrate.CdnUrl("local/7974964c-adef-4ff5-8096-471c56dffc09/360-icon_editor.jpg"), mediaEventData.Asset.FileData.Url);
            Assert.AreEqual(Path.Combine(Application.persistentDataPath, lesson.Guid, mediaEventData.Asset.FileData.Guid, mediaEventData.Asset.FileData.Name), Orchestrate.GetCachePath( mediaEventData.Asset.FileData));
            Assert.AreEqual(lesson.Guid, mediaEventData.Asset.FileData.Parent.Guid);
            Assert.IsFalse(mediaEventData.MuteAudio);
            
            // Hotspot event
            HotspotEventData hotspotEventData = (HotspotEventData) scene1.SortedTimedEvents[3];
            Assert.AreEqual(typeof(HotspotEventData), hotspotEventData.GetType());
            Assert.AreEqual(5, hotspotEventData.Hotspots.Count);
            HotspotData healthcareHotspot = hotspotEventData.Hotspots[0];
            Assert.AreEqual("Healthcare", healthcareHotspot.Name);
            Assert.AreEqual(ActionType.GoToScene, healthcareHotspot.Action.Type);
            Assert.AreEqual(lesson.Scenes[4].Id, healthcareHotspot.Action.SceneId);
        }

        [UnityTest]
        public IEnumerator TestGetLesson() => UniTask.ToCoroutine(async () =>
        {
            LessonData lesson = await Orchestrate.LoadLesson(Config.Lesson);
            AssertValidLesson(lesson);

            lesson = Orchestrate.LoadCachedLesson(Config.Lesson.Guid);
            AssertValidLesson(lesson);
        });
    }
} 