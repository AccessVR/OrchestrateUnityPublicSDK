using Newtonsoft.Json;
using NUnit.Framework;

namespace AccessVR.OrchestrateVR.SDK.Tests
{
    /// <summary>
    /// The app downloads the full-resolution copy when its device can decode
    /// it, and otherwise the sharpest smaller copy the server offers.
    /// </summary>
    public class VideoDownloadChoiceTests
    {
        private const string Original = "org/assets/clip.mp4";
        private const string Download4K = "org/assets/clip_download.mp4";

        private static AssetData Video(string downloadsJson, string sizeJson = @"""originalWidth"": 8192, ""originalHeight"": 4096") =>
            JsonConvert.DeserializeObject<AssetData>(
                @"{""assetTypeId"": 6, ""originalPath"": """ + Original + @""", ""width"": 3840, ""height"": 1920, " + sizeJson + @", ""downloads"": " + downloadsJson + "}");

        private const string OneDownload = @"[{""path"": """ + Download4K + @""", ""width"": 4096, ""height"": 2048, ""codec"": ""hevc""}]";

        private static bool UpTo(long pixels, int width, int height) => (long) width * height <= pixels;

        [Test]
        public void TestDeviceThatDecodesTheOriginalKeepsIt()
        {
            Assert.AreEqual(Original, Video(OneDownload).VideoPathFor((w, h, _) => UpTo(8192L * 4096, w, h)));
        }

        [Test]
        public void TestDeviceThatCannotDecodeTheOriginalGetsTheDownload()
        {
            Assert.AreEqual(Download4K, Video(OneDownload).VideoPathFor((w, h, _) => UpTo(4096L * 2048, w, h)));
        }

        [Test]
        public void TestTheOriginalsOwnSizeWinsOverTheLastRenditionsSize()
        {
            // width/height say 3840 × 1920, which a 4K device decodes; the
            // original is really 8192 × 4096.
            Assert.AreEqual(Download4K, Video(OneDownload).VideoPathFor((w, h, _) => UpTo(4096L * 2048, w, h)));
            Assert.AreEqual(Original, Video(OneDownload, @"""originalWidth"": null").VideoPathFor((w, h, _) => UpTo(4096L * 2048, w, h)));
        }

        [Test]
        public void TestPicksTheSharpestDownloadTheDeviceDecodes()
        {
            AssetData video = Video(@"[
                {""path"": ""small.mp4"", ""width"": 2048, ""height"": 1024, ""codec"": ""hevc""},
                {""path"": ""large.mp4"", ""width"": 5760, ""height"": 2880, ""codec"": ""hevc""},
                {""path"": ""medium.mp4"", ""width"": 4096, ""height"": 2048, ""codec"": ""hevc""}]");

            Assert.AreEqual("medium.mp4", video.VideoPathFor((w, h, _) => UpTo(4096L * 2048, w, h)));
        }

        [Test]
        public void TestFallsBackToTheSmallestWhenNothingDecodes()
        {
            AssetData video = Video(@"[
                {""path"": ""large.mp4"", ""width"": 5760, ""height"": 2880, ""codec"": ""hevc""},
                {""path"": ""small.mp4"", ""width"": 2048, ""height"": 1024, ""codec"": ""hevc""}]");

            Assert.AreEqual("small.mp4", video.VideoPathFor((_, _, _) => false));
        }

        [Test]
        public void TestNoDownloadsKeepsTheOriginal()
        {
            Assert.AreEqual(Original, Video("[]").VideoPathFor((_, _, _) => false));
            Assert.AreEqual(Original, Video("null").VideoPathFor((_, _, _) => false));
        }
    }
}
