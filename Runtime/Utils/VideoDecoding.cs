using System;
using System.Collections.Generic;
using UnityEngine;

namespace AccessVR.OrchestrateVR.SDK
{
    /// <summary>
    /// Whether this device plays a video of a given size smoothly, at 60 fps.
    /// </summary>
    public static class VideoDecoding
    {
        public const string Hevc = "hevc";

        /// <summary>
        /// 5% under 60 fps. Decoders publish their 8K rating (7680 × 4320 at
        /// 60) as an exact ceiling on 16×16 blocks a second, and VR180's
        /// 8192 × 4096 is 1% more pixels than that. A Quest 3 plays it
        /// smoothly but would refuse it at 60; a decoder rated for 8K at 30
        /// still refuses it here.
        /// </summary>
        private const int FramesPerSecond = 57;

        /// <summary>
        /// iOS has no way to ask its decoder. An M1 iPad Pro plays 8192 × 4096
        /// HEVC at a few frames a second, and Apple rates its video playback at
        /// 4K, so every iPad is held to the size the server's download copy is
        /// made at.
        /// </summary>
        private const long TouchscreenPixels = 4096L * 2048;

        private static readonly Dictionary<(string Codec, int Width, int Height), bool> AndroidAnswers = new();

        public static bool CanDecode(int width, int height, string codec)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            lock (AndroidAnswers)
            {
                if (!AndroidAnswers.TryGetValue((codec, width, height), out bool answer))
                {
                    answer = AndroidCanDecode(width, height, codec);
                    AndroidAnswers[(codec, width, height)] = answer;
                }

                return answer;
            }
#else
            return !Orchestrate.IsTouchscreenPlayer() || (long) width * height <= TouchscreenPixels;
#endif
        }

        /// <summary>
        /// Asks Android for a decoder that takes this size at about 60 fps.
        /// Quest 3 answers yes at 8192 × 4096; headsets with smaller decoders
        /// say no, and get the download copy. A failed question keeps the
        /// full-resolution copy, as before there was a choice.
        /// </summary>
        private static bool AndroidCanDecode(int width, int height, string codec)
        {
            try
            {
                // Downloads are chosen off the main thread too; attaching an
                // attached thread is a no-op.
                AndroidJNI.AttachCurrentThread();

                using AndroidJavaClass mediaFormat = new("android.media.MediaFormat");
                using AndroidJavaObject format = mediaFormat.CallStatic<AndroidJavaObject>("createVideoFormat", MimeType(codec), width, height);
                format.Call("setInteger", "frame-rate", FramesPerSecond);

                // 0 is MediaCodecList.REGULAR_CODECS.
                using AndroidJavaObject codecs = new("android.media.MediaCodecList", 0);
                return !string.IsNullOrEmpty(codecs.Call<string>("findDecoderForFormat", format));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Couldn't ask for a {codec} decoder at {width}×{height}; keeping the full-resolution video. {e.Message}");
                return true;
            }
        }

        private static string MimeType(string codec) => codec == Hevc ? "video/hevc" : "video/avc";
    }
}
