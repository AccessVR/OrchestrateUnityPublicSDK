using UnityEngine;
using UnityEngine.Serialization;
using System;
using System.Collections.Generic;

namespace AccessVR.OrchestrateVR.SDK
{
    public class OrchestrateEnvironment : GenericSingleton<OrchestrateEnvironment>
    {
        public static string Environment;

        private static string? authToken;

        public static void SetEnvironment(string environment)
        {
            Environment = environment;
        }

        public static string GetEnvironment()
        {
            return Environment ?? "local";
        }

        public static void SetAuthToken(string token)
        {
            authToken = token;
        }

        public static string? GetAuthToken()
        {
            return String.IsNullOrEmpty(authToken) ? PlayerPrefs.GetString("apiKey") : authToken;
        }

        public static string GetBaseURL()
        {
            return Environment switch
            {
                "production" => "https://app.orchestratevr.com",
                "stage" => "https://orchestrate-stage.accessvr.com",
                "dev" => "https://orchestrate-dev.accessvr.com",
                "local" => "https://ovr.avr.ngrok.io",
                _ => "http://localhost"
            };
        }

        public static string GetCdnUrl(string? path = null)
        {
            string CdnBaseURL = Environment switch
            {
                "production" => "https://files.accessvr.com",
                _ => "https://dev.files.ovr.accessvr.com"
            };

            return CdnBaseURL + (path != null ? "/" + path.TrimStart('/') : "");
        }

        public static string GetUrl(string? path = null)
        {
            return GetBaseURL() + (path != null ? "/" + path.TrimStart('/') : "");
        }

        public static string CdnUrl(string? path)
        {
            return GetCdnUrl() + (path != null ? "/" + path.TrimStart('/') : "");
        }

        public static HttpClient CreateClient()
        {
            return HttpClient.Create();
        }

        public StyleRefSettings colorStyles;
        [FormerlySerializedAs("standInPreviewImage")] public Sprite placeholderPreviewImage;
        
        public static StyleRefSettings ColorStyles => Instance.colorStyles;

        public UserData currentUser = new UserData();
        public AssignmentData currentAssignment;

        public bool forceOffline = false;
        
        public enum OfflineStates {
            Offline,
            Online,
            Unknown
        }

        public static bool IsQuestRuntime => IsQuest2Runtime || IsQuest3Runtime;
        public static bool IsQuest2Runtime =>
            Application.platform == RuntimePlatform.Android && 
            UnityEngine.Device.SystemInfo.deviceName.Contains("Quest 2");
        
        public static bool IsQuest3Runtime =>
            Application.platform == RuntimePlatform.Android && 
            UnityEngine.Device.SystemInfo.deviceName.Contains("Quest 3");

        public static bool IsViveRuntime => IsViveFocus3Runtime || IsViveFocusVisionRuntime;
        
        public static bool IsViveFocus3Runtime =>
            Application.platform == RuntimePlatform.Android &&
            UnityEngine.Device.SystemInfo.deviceName.Contains("VIVE Focus 3");
        
        public static bool IsViveFocusVisionRuntime =>
            Application.platform == RuntimePlatform.Android &&
            UnityEngine.Device.SystemInfo.deviceName.Contains("VIVE Focus Vision");

        public static bool IsUnknownRuntime => !IsQuestRuntime && !IsViveRuntime;

        private float testOfflineStateFrequency = 1.0f; // seconds
        
        private float timeSinceLastOfflineStateTest = 0.0f;
        
        private bool lastOfflineValue;

        public static string Version => "v" + Application.version + " / " + Application.unityVersion;

        public override void Awake()
        {
            base.Awake();
            lastOfflineValue = Offline;
        }

        private void Update()
        {
            timeSinceLastOfflineStateTest += Time.deltaTime;
            if (timeSinceLastOfflineStateTest > testOfflineStateFrequency)
            {
                timeSinceLastOfflineStateTest = 0;
                if (lastOfflineValue != Offline)
                {
                    offlineStateListeners.ForEach((listener) => listener.OnOfflineStateChanged(Offline, lastOfflineValue));
                }
                lastOfflineValue = Offline;
            }
        }

        private List<OfflineStateListener> offlineStateListeners = new List<OfflineStateListener>();

        public static void addOfflineStateListener(OfflineStateListener listener)
        {
            Instance.offlineStateListeners.Add(listener);
        }
        
        public static void removeOfflineStateListener(OfflineStateListener listener)
        {
            Instance.offlineStateListeners.Remove(listener);
        }

        public static OfflineStates getOfflineStateForString(string value)
        {
            return value switch
            {
                null => OfflineStates.Unknown,
                "offline" => OfflineStates.Offline,
                "online" => OfflineStates.Online,
                _ => OfflineStates.Unknown
            };
        }

        public static OfflineStates OfflineState
        {
            get => getOfflineStateForString(PlayerPrefs.GetString("OfflineState"));
            set
            {
                if (value == OfflineStates.Online)
                {
                    PlayerPrefs.SetString("OfflineState", "online");
                } else if (value == OfflineStates.Offline)
                {
                    PlayerPrefs.SetString("OfflineState", "offline");
                }
                else
                {
                    throw new Exception("Only allowed to set OfflineState to Online or Offline");
                }
            }
        }
        
        public static bool Offline
        {
            get
            {
                #if UNITY_EDITOR
                    if (Instance.forceOffline)
                    {
                        return true;
                    }
                #endif
                
                if (OfflineState == OfflineStates.Unknown)
                {
                    return Application.internetReachability == NetworkReachability.NotReachable;
                }
                else if (OfflineState == OfflineStates.Offline || Application.internetReachability == NetworkReachability.NotReachable)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}