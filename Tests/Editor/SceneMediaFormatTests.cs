using Newtonsoft.Json;
using NUnit.Framework;

namespace AccessVR.OrchestrateVR.SDK.Tests
{
    public class ScreenTypeMappingTests
    {
        // SceneData.OnDeserialized throws without timedEvents, and the
        // Thumbnail getter dereferences Skybox; every scene fixture needs both.
        private static SceneData MakeScene(string extraJson = "") =>
            JsonConvert.DeserializeObject<SceneData>(
                @"{""id"": 1, ""skyboxAsset"": {""assetTypeId"": 5}, ""timedEvents"": []" + extraJson + "}");

        [Test]
        public void TestScreenTypeIntsMapToEnum()
        {
            Assert.AreEqual(ScreenType.Sphere360, MakeScene(@", ""screenType"": 0").ScreenType);
            Assert.AreEqual(ScreenType.Sphere180, MakeScene(@", ""screenType"": 1").ScreenType);
            Assert.AreEqual(ScreenType.Big, MakeScene(@", ""screenType"": 2").ScreenType);
            Assert.AreEqual(ScreenType.Pano, MakeScene(@", ""screenType"": 3").ScreenType);
        }

        [Test]
        public void TestUnknownAndAbsentScreenTypeDefaultTo360()
        {
            Assert.AreEqual(ScreenType.Sphere360, MakeScene(@", ""screenType"": 99").ScreenType);
            Assert.AreEqual(ScreenType.Sphere360, MakeScene().ScreenType);
        }
    }

    public class MaskDataTests
    {
        private static SceneData MakeScene(string extraJson = "") =>
            JsonConvert.DeserializeObject<SceneData>(
                @"{""id"": 1, ""skyboxAsset"": {""assetTypeId"": 5}, ""timedEvents"": []" + extraJson + "}");

        [Test]
        public void TestMaskRoundTrips()
        {
            var scene = MakeScene(
                @", ""mask"": {""enabled"": true, ""longitude"": -90, ""latitude"": 15, ""size"": 200}");

            Assert.IsNotNull(scene.Mask);
            Assert.IsTrue(scene.Mask.Enabled);
            Assert.AreEqual(-90f, scene.Mask.Longitude);
            Assert.AreEqual(15f, scene.Mask.Latitude);
            Assert.AreEqual(200f, scene.Mask.Size);
        }

        [Test]
        public void TestAbsentMaskIsNull()
        {
            Assert.IsNull(MakeScene().Mask);
        }

        [Test]
        public void TestOutOfRangeValuesAreClamped()
        {
            var mask = JsonConvert.DeserializeObject<MaskData>(
                @"{""enabled"": true, ""longitude"": 10, ""latitude"": 120, ""size"": 500}");
            Assert.AreEqual(90f, mask.Latitude);
            Assert.AreEqual(MaskData.MaxSize, mask.Size);

            var tiny = JsonConvert.DeserializeObject<MaskData>(
                @"{""enabled"": true, ""size"": 5}");
            Assert.AreEqual(MaskData.MinSize, tiny.Size);
        }

        [Test]
        public void TestMaskDefaultsWhenKeysOmitted()
        {
            var mask = JsonConvert.DeserializeObject<MaskData>(@"{""enabled"": true}");
            Assert.AreEqual(180f, mask.Size);
            Assert.AreEqual(0f, mask.Longitude);
            Assert.AreEqual(0f, mask.Latitude);
        }
    }

    public class VideoTypeTests
    {
        private static AssetData MakeVideo(string videoTypeJson) =>
            JsonConvert.DeserializeObject<AssetData>(
                @"{""assetTypeId"": 6" + videoTypeJson + "}");

        [Test]
        public void TestVideoTypeIdsMapToEnum()
        {
            Assert.AreEqual(VideoType.Video360, MakeVideo(@", ""videoTypeId"": 1").VideoType);
            Assert.AreEqual(VideoType.Video2D, MakeVideo(@", ""videoTypeId"": 2").VideoType);
            Assert.AreEqual(VideoType.Video360Stereo, MakeVideo(@", ""videoTypeId"": 3").VideoType);
            Assert.AreEqual(VideoType.VideoStereoSbs, MakeVideo(@", ""videoTypeId"": 4").VideoType);
            Assert.AreEqual(VideoType.VideoStereoTab, MakeVideo(@", ""videoTypeId"": 5").VideoType);
            Assert.AreEqual(VideoType.Video180, MakeVideo(@", ""videoTypeId"": 6").VideoType);
            Assert.AreEqual(VideoType.Video180Stereo, MakeVideo(@", ""videoTypeId"": 7").VideoType);
        }

        [Test]
        public void TestMissingZeroAndUnknownIdsAreUnknown()
        {
            Assert.AreEqual(VideoType.Unknown, MakeVideo("").VideoType);
            Assert.IsFalse(MakeVideo("").HasVideoType());
            Assert.AreEqual(VideoType.Unknown, MakeVideo(@", ""videoTypeId"": 0").VideoType);
            Assert.AreEqual(VideoType.Unknown, MakeVideo(@", ""videoTypeId"": 99").VideoType);
        }

        [Test]
        public void TestStereoLayoutConventions()
        {
            // VR180 stereo = side-by-side; 360 stereo = top-bottom.
            Assert.AreEqual(StereoLayout.SideBySide, MakeVideo(@", ""videoTypeId"": 7").StereoLayout);
            Assert.AreEqual(StereoLayout.TopBottom, MakeVideo(@", ""videoTypeId"": 3").StereoLayout);
            Assert.AreEqual(StereoLayout.SideBySide, MakeVideo(@", ""videoTypeId"": 4").StereoLayout);
            Assert.AreEqual(StereoLayout.TopBottom, MakeVideo(@", ""videoTypeId"": 5").StereoLayout);
            Assert.AreEqual(StereoLayout.None, MakeVideo(@", ""videoTypeId"": 1").StereoLayout);
            Assert.AreEqual(StereoLayout.None, MakeVideo(@", ""videoTypeId"": 6").StereoLayout);
        }

        [Test]
        public void Test180Detection()
        {
            Assert.IsTrue(MakeVideo(@", ""videoTypeId"": 6").Is180Video());
            Assert.IsTrue(MakeVideo(@", ""videoTypeId"": 7").Is180Video());
            Assert.IsFalse(MakeVideo(@", ""videoTypeId"": 1").Is180Video());
            Assert.IsTrue(MakeVideo(@", ""videoTypeId"": 7").IsStereoVideo());
            Assert.IsFalse(MakeVideo(@", ""videoTypeId"": 6").IsStereoVideo());
        }

        [Test]
        public void TestImagesNeverReportVideoFormats()
        {
            // A stray videoTypeId on a non-video asset must not claim 180/stereo.
            var image = JsonConvert.DeserializeObject<AssetData>(
                @"{""assetTypeId"": 5, ""videoTypeId"": 7}");
            Assert.IsFalse(image.Is180Video());
            Assert.IsFalse(image.IsStereoVideo());
            Assert.AreEqual(StereoLayout.None, image.StereoLayout);
        }
    }
}
