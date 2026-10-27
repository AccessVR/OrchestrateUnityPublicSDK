using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AccessVR.OrchestrateVR.SDK.Tests
{
    /// <summary>
    /// Scenes with no skybox, the Panorama display type, and the frame and
    /// clip a media Layer carries, as the web editor and the server's Big
    /// Screen conversion write them.
    /// </summary>
    public class SceneSkyboxTests
    {
        [Test]
        public void TestASceneWithNoSkyboxStillLoads()
        {
            // Builds before this release threw here, failing the whole Experience.
            var scene = JsonConvert.DeserializeObject<SceneData>(@"{""id"": 1, ""noSkybox"": true, ""timedEvents"": []}");

            Assert.IsFalse(scene.HasSkybox);
            Assert.IsNull(scene.Thumbnail);
            Assert.IsEmpty(scene.GetDownloadableFiles());
        }

        [Test]
        public void TestAPlaceholderSkyboxIsNeitherDrawnNorDownloaded()
        {
            var scene = JsonConvert.DeserializeObject<SceneData>(
                @"{""id"": 1, ""noSkybox"": true,
                  ""skyboxAsset"": {""id"": 9, ""assetTypeId"": 5, ""path"": ""dev/7d2f1c0e-0000-4000-8000-000000000009/placeholder.jpg"", ""thumbnailPath"": ""dev/7d2f1c0e-0000-4000-8000-000000000009/placeholder_thumb.jpg""},
                  ""thumbnailAsset"": {""id"": 12, ""assetTypeId"": 6, ""path"": ""dev/7d2f1c0e-0000-4000-8000-000000000012/talk.mp4"", ""thumbnailPath"": ""dev/7d2f1c0e-0000-4000-8000-000000000012/talk_thumb.jpg""},
                  ""timedEvents"": []}");

            Assert.IsFalse(scene.HasSkybox);
            Assert.IsFalse(scene.GetDownloadableFiles().Any(file => file.Url != null && file.Url.Contains("placeholder")));
            StringAssert.Contains("talk_thumb", scene.Thumbnail.path);
        }

        [Test]
        public void TestASceneWithASkyboxWrapsIt()
        {
            var scene = JsonConvert.DeserializeObject<SceneData>(@"{""id"": 1, ""skyboxAsset"": {""assetTypeId"": 5}, ""timedEvents"": []}");

            Assert.IsTrue(scene.HasSkybox);
            Assert.AreEqual(0f, scene.SkyboxRotation);
        }

        [Test]
        public void TestASkyboxTurnsAndItsInitialViewKeepsItsHeading()
        {
            var scene = JsonConvert.DeserializeObject<SceneData>(
                @"{""id"": 1, ""skyboxAsset"": {""assetTypeId"": 5}, ""skyboxRotation"": 135,
                  ""initialView"": {""quaternion"": {""X"": 0, ""Y"": 0, ""Z"": 0, ""W"": 1}, ""longitude"": -45, ""latitude"": 0},
                  ""timedEvents"": []}");

            Assert.AreEqual(135f, scene.SkyboxRotation);
            Assert.AreEqual(-45f, scene.InitialView.Longitude);
        }

        [Test]
        public void TestASceneWithNoSkyboxReadsItsCeilingAndFloorColors()
        {
            var scene = JsonConvert.DeserializeObject<SceneData>(
                @"{""id"": 1, ""noSkybox"": true, ""ceilingColor"": ""#87ceeb"", ""floorColor"": ""#556b2f"", ""timedEvents"": []}");

            Assert.AreEqual("#87ceeb", scene.CeilingColor);
            Assert.AreEqual("#556b2f", scene.FloorColor);
        }
    }

    public class MediaLayerSurfaceTests
    {
        private static MediaEventData Make(string extraJson) =>
            (MediaEventData) EventDataFactory.Make(@"{""eventType"": 6, ""startTime"": 0, ""duration"": 10" + extraJson + "}", null);

        [Test]
        public void TestDisplayTypeFourIsAPanorama()
        {
            var layer = Make(@", ""displayType"": 4");

            Assert.AreEqual(DisplayTypeOptions.Panorama, layer.DisplayType);
            Assert.IsTrue(layer.IsPanorama);
        }

        [TestCase("", 0f)]
        [TestCase(@", ""panoramaPosition"": ""front""", 0f)]
        [TestCase(@", ""panoramaPosition"": ""right""", 90f)]
        [TestCase(@", ""panoramaPosition"": ""back""", 180f)]
        [TestCase(@", ""panoramaPosition"": ""left""", -90f)]
        public void TestAPanoramaTurnsFromTheFrontToItsPosition(string position, float turn)
        {
            var layer = Make(@", ""displayType"": 4" + position);

            Assert.AreEqual(turn, layer.PanoramaTurnDegrees);
        }

        [Test]
        public void TestAWorldspacePictureCanDropItsFrame()
        {
            var layer = Make(@", ""displayType"": 1, ""frame"": ""none""");

            Assert.IsFalse(layer.IsPanorama);
            Assert.IsTrue(layer.IsFrameless);
        }

        [Test]
        public void TestAbsentFieldsDrawAFramedFlatCard()
        {
            var layer = Make(@", ""displayType"": 1");

            Assert.IsFalse(layer.IsPanorama);
            Assert.IsFalse(layer.IsFrameless);
            Assert.IsFalse(layer.Clip.IsTrimmed);
        }

        [Test]
        public void TestAClipPlaysPartOfTheFile()
        {
            MediaClipData clip = Make(@", ""clip"": {""in"": 12, ""out"": 138.4}").Clip;

            Assert.IsTrue(clip.IsTrimmed);
            Assert.AreEqual(12.0, clip.FileTimeAt(0.0), 1e-9);
            Assert.AreEqual(40.0, clip.FileTimeAt(28.0), 1e-9);
            Assert.AreEqual(126.4, clip.Length(150.0), 1e-9);
            Assert.IsFalse(clip.HasReachedOutPoint(138.0));
            Assert.IsTrue(clip.HasReachedOutPoint(138.39));
        }

        [Test]
        public void TestAClipWithNoOutPointRunsToTheEnd()
        {
            MediaClipData clip = Make(@", ""clip"": {""in"": 30, ""out"": null}").Clip;

            Assert.AreEqual(60.0, clip.Length(90.0), 1e-9);
            Assert.IsFalse(clip.HasReachedOutPoint(89.99));
        }

        [Test]
        public void TestAnOutPointBeforeTheInPointIsIgnored()
        {
            MediaClipData clip = Make(@", ""clip"": {""in"": 20, ""out"": 5}").Clip;

            Assert.IsNull(clip.Out);
            Assert.AreEqual(20.0, clip.In, 1e-9);
        }
    }
}
