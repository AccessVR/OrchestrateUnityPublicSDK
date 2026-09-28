using Cysharp.Threading.Tasks;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace AccessVR.OrchestrateVR.SDK
{
    /// <summary>
    /// The microphone permission a WebRTC conversation needs. Android requires a
    /// runtime prompt in addition to the manifest entry; every other platform
    /// either prompts from inside the WebView or needs nothing, so the request
    /// resolves true there.
    /// </summary>
    public static class MicrophonePermission
    {
        public static bool IsGranted()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return Permission.HasUserAuthorizedPermission(Permission.Microphone);
#else
            return true;
#endif
        }

        /// <summary>
        /// Prompts when not yet granted. Resolves with the outcome; a denial (or
        /// a "don't ask again") resolves false so callers can fall back rather
        /// than wait on a prompt that will never appear.
        /// </summary>
        public static UniTask<bool> Request()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (IsGranted())
            {
                return UniTask.FromResult(true);
            }

            var completion = new UniTaskCompletionSource<bool>();
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += _ => completion.TrySetResult(true);
            callbacks.PermissionDenied += _ => completion.TrySetResult(false);
            callbacks.PermissionDeniedAndDontAskAgain += _ => completion.TrySetResult(false);

            Debug.Log("[MicrophonePermission] requesting RECORD_AUDIO");
            Permission.RequestUserPermission(Permission.Microphone, callbacks);

            return completion.Task;
#else
            return UniTask.FromResult(true);
#endif
        }
    }
}
