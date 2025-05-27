using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Cysharp.Threading.Tasks;
using System.Net.Http;
using UnityEngine.Windows;
using UnityEngine.Networking;

namespace AccessVR.OrchestrateVR.SDK
{
	public enum Environment
	{
		Production,
		Staging,
		Development,
		Local,
		Custom,
	}
	
    public class Orchestrate : GenericSingleton<Orchestrate>
    {
	    [SerializeField]
	    protected Environment environmentName = Environment.Production;

	    public StyleRefSettings colorStyles;
		
        public static StyleRefSettings ColorStyles => Instance.colorStyles;
	    
        private string _customBaseUrl = null;

        private string _customCdnUrl = null;

        private LessonData _activeExperience;
        
        private UserData _user;

        private string _authToken;

        private string _userCode;
        
	    [CanBeNull]
        public static UserData User => GetUser();

        public static void Set(Environment name)
        {
	        Instance.environmentName = name;
        }

        public static Environment Get()
        {
	        return Instance.environmentName;
        }

        public static bool Is(Environment environmentName)
        {
	        return Instance.environmentName == environmentName;
        }

        public static void SetAuthToken(string token)
        {
	        PlayerPrefs.SetString("apiKey", token);
	        PlayerPrefs.Save();
            Instance._authToken = token;
        }

        public static string GetAuthToken()
        {
            return String.IsNullOrEmpty(Instance._authToken) ? PlayerPrefs.GetString("apiKey") : Instance._authToken;
        }

        public static void SetBaseUrl(string url)
        {
            Instance._customBaseUrl = url;
        }

        public static void SetActiveExperience(LessonData experience)
        {
            Instance._activeExperience = experience;
        }

        public static void SetUser(UserData user)
        {
            Instance._user = user;
        }

        public static UserData GetUser()
        {
	        return Instance._user;
        }

        public static LessonData GetActiveExperience()
        {
            return Instance._activeExperience;
        }

        public static string GetBaseUrl()
        {
	        if (Is(Environment.Custom) && !String.IsNullOrEmpty(Instance._customBaseUrl))
	        {
		        return Instance._customBaseUrl;
	        }

	        return Instance.environmentName switch
            {
                Environment.Production => "https://app.orchestratevr.com",
                Environment.Staging => "https://orchestrate-stage.accessvr.com",
                Environment.Development => "https://orchestrate-dev.accessvr.com",
                Environment.Local => "https://ovr.avr.ngrok.io",
                _ => "http://localhost"
            };
        }

        public static string ProfileUrl(int userId)
		{
			return ProfileUrl(userId.ToString());
		}
		
		public static string ProfileUrl()
		{
			return ProfileUrl(User.UserId);
		}

		public static string ProfileUrl(string userId)
        {
            return GetUrl("/resources/users/" + userId);
        }
        
        public static void SetCdnUrl(string url)
        {
            Instance._customCdnUrl = url;
        }

        public static string GetCdnUrl([CanBeNull] string path = null)
        {
            if (Is(Environment.Custom) && !String.IsNullOrEmpty(Instance._customCdnUrl))
            {
                return Instance._customCdnUrl;
            }

            string cdnBaseURL = Instance.environmentName switch
            {
                Environment.Production => "https://files.accessvr.com",
                _ => "https://dev.files.ovr.accessvr.com"
            };

            return cdnBaseURL + (path != null ? "/" + path.TrimStart('/') : "");
        }

        public static string GetUrl(string path = null)
        {
            return GetBaseUrl() + (path != null ? "/" + path.TrimStart('/') : "");
        }

        public static string CdnUrl(string path)
        {
            return GetCdnUrl() + (path != null ? "/" + path.TrimStart('/') : "");
        }

        public static HttpClient CreateClient()
        {
            return HttpClient.Create(GetAuthToken());
        }

        public bool forceOffline = false;
        
        public enum OfflineStates {
            Offline,
            Online,
            Unknown
        }

        public static bool IsQuestRuntime => 
	        IsQuest2Runtime || IsQuest3Runtime;
        
        public static bool IsQuest2Runtime =>
            Application.platform == RuntimePlatform.Android && 
            UnityEngine.Device.SystemInfo.deviceName.Contains("Quest 2");
        
        public static bool IsQuest3Runtime =>
            Application.platform == RuntimePlatform.Android && 
            UnityEngine.Device.SystemInfo.deviceName.Contains("Quest 3");

        public static bool IsViveRuntime => 
	        IsViveFocus3Runtime || IsViveFocusVisionRuntime;
        
        public static bool IsViveFocus3Runtime =>
            Application.platform == RuntimePlatform.Android &&
            UnityEngine.Device.SystemInfo.deviceName.Contains("VIVE Focus 3");
        
        public static bool IsViveFocusVisionRuntime =>
            Application.platform == RuntimePlatform.Android &&
            UnityEngine.Device.SystemInfo.deviceName.Contains("VIVE Focus Vision");

        public static bool IsUnknownRuntime => !IsQuestRuntime && !IsViveRuntime;

        private NetworkReachability networkStatus;
        
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

        private List<ISessionListener> sessionListeners = new List<ISessionListener>();
        
        public static void addSessionListener(ISessionListener listener)
        {
            Instance.sessionListeners.Add(listener);
        }
        
        public static void removeSessionListener(ISessionListener listener)
        {
            Instance.sessionListeners.Remove(listener);
        }
        
        private List<IOfflineStateListener> offlineStateListeners = new List<IOfflineStateListener>();

        public static void addOfflineStateListener(IOfflineStateListener listener)
        {
            Instance.offlineStateListeners.Add(listener);
        }

        public static void removeOfflineStateListener(IOfflineStateListener listener)
        {
            Instance.offlineStateListeners.Remove(listener);
        }
        
        private static List<IErrorHandler> errorHandlers = new List<IErrorHandler>();

        public static void addErrorHandler(IErrorHandler handler)
        {
            errorHandlers.Add(handler);
        }

        public static void removeErrorHandler(IErrorHandler handler)
        {
            errorHandlers.Remove(handler);
        }

        private static List<IDownloadJobListener> downloadProgressListeners = new();

        public static void FireError(Error error)
        {
            errorHandlers.ForEach((handler) => handler.HandleError(error));
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
                    PlayerPrefs.Save();
                } else if (value == OfflineStates.Offline)
                {
                    PlayerPrefs.SetString("OfflineState", "offline");
                    PlayerPrefs.Save();
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

        public bool HasUser()
        {
	        return _user != null || PlayerPrefs.HasKey("apiKey");
        }
		
		public static async UniTask<UserData> LoadUser(Action onUserDataLoaded)
		{
			if (String.IsNullOrEmpty(GetAuthToken()))
			{
				if (!Offline)
				{	
					FireError(Error.NotAuthenticated);
				}
				else
				{
					FireError(Error.InternetRequired);
				}
				return null;
			}
			
			if (PlayerPrefs.HasKey("userid"))
			{
				UserData user = new UserData();
				user.UserId = PlayerPrefs.GetString("userid");
				user.DisplayName = PlayerPrefs.GetString("displayname");
				user.UserName = PlayerPrefs.GetString("username");
				user.Roles = PlayerPrefs.GetString("userroles").Split(',').ToList();
				user.Permissions = PlayerPrefs.GetString("userpermissions").Split(',').ToList();

				SetUser(user);
			}
			else
			{
				try
				{
					UserData user = await CreateClient().GetUser();

					PlayerPrefs.SetString("username", user.UserName);
					PlayerPrefs.SetString("displayname", user.DisplayName);
					PlayerPrefs.SetString("userid", user.UserId);
					PlayerPrefs.SetString("userroles", String.Join(",", user.Roles));
					PlayerPrefs.SetString("userpermissions", String.Join(",", user.Permissions));
					PlayerPrefs.Save();
					
					SetUser(user);
				}
				catch (HttpRequestException e)
				{
					FireError(new Error(e));
					return null;
				}
			}
			
			if (!Offline)
			{
				if (!await Instance.IsLoggedIn())
				{
					FireError(Error.NotAuthenticated);
					return null;
				}
			}

			onUserDataLoaded?.Invoke();
			Instance.sessionListeners.ForEach((handler) => handler.OnUserData(GetUser()));
			return GetUser();
		}
		
		public async UniTask<bool> IsLoggedIn()
		{
			return await CreateClient().IsLoggedIn();
		}

		public void Logout()
		{
			SetAuthToken(null);
			SetUser(null);
			PlayerPrefs.DeleteAll();
			PlayerPrefs.Save();
			sessionListeners.ForEach((handler) => handler.OnLogout());
		}
        
        public static Dictionary<string, string> GetSettings()
		{
			Dictionary<string, string> settings = new Dictionary<string, string>()
			{
				{"offlineState", OfflineState switch {
	                OfflineStates.Unknown => "unknown",
	                OfflineStates.Offline => "offline",
	                OfflineStates.Online => "online",
	                _ => "unknown",
				}}
			};

			return settings;
		}

		public static async UniTask<LessonData> GetOrLoadLesson(LessonDataLookup lookup)
		{
			if (!lookup.Preview && HasCachedLesson(lookup.Guid))
			{
				return LoadCachedLesson(lookup.Guid);
			}
			else
			{
				return await LoadLesson(lookup);
			}
		}

		public static async UniTask<LessonData> LoadLesson(LessonDataLookup lookup)
		{
			NumberUtils.AssertNotNullOrEmpty(lookup.Id);
			StringUtils.AssertNotNullOrEmpty(lookup.Guid);
			
			return new LessonData();
		}

		public static bool HasCachedLesson(string guid)
		{
			StringUtils.AssertNotNullOrEmpty(guid);
			
			return false;
		}

		public static LessonData LoadCachedLesson(string guid)
		{
			StringUtils.AssertNotNullOrEmpty(guid);
			
			return new LessonData();
		}

		private static void CacheLesson(LessonData lessonData)
		{
			StringUtils.AssertNotNullOrEmpty(lessonData.Guid);
		}

		public static void RemoveCachedLesson(string guid, bool removeContentAndAssets = false)
		{
			StringUtils.AssertNotNullOrEmpty(guid);		
		}
		
		public static List<LessonData> GetCachedLessonList()
		{
			return new List<LessonData>();
			
			// List<LessonData> cachedLessons = new List<LessonData>();
			// string path = Application.persistentDataPath;
			//
			// foreach (string dir in Directory.GetDirectories(path))
			// {
			// 	string guid = dir.Split('/').Last();
			// 	string contentFile = Path.Combine(dir, "content.json");
			// 	if (File.Exists(contentFile))
			// 	{
			// 		try
			// 		{
			// 			string json = File.ReadAllText(contentFile);
			// 			JObject content = JObject.Parse(json);
			// 			if (content["name"] != null)
			// 			{
			// 				LessonData lesson = new LessonData()
			// 				{
			// 					Id = (int) content["id"],
			// 					Name = content["name"].ToString(),
			// 					Guid = guid,
			// 				};
			// 				
			// 				string thumbnailPath = content["scenes"]?[0]?["thumbnailAsset"]?["thumbnailPath"].ToString();
			// 				if (thumbnailPath == null)
			// 				{
			// 					thumbnailPath = content["scenes"]?[0]?["skyboxAsset"]?["thumbnailPath"].ToString();
			// 				}
			// 				
			// 				if (thumbnailPath != null)
			// 				{
			// 					string thumbnailFile = Path.Combine(dir, thumbnailPath);
			// 					if (File.Exists(StringFormattingFunctions.ReplaceSpaces(thumbnailFile)))
			// 					{
			// 						try
			// 						{
			// 							byte[] imageBytes = File.ReadAllBytes(StringFormattingFunctions.ReplaceSpaces(thumbnailFile));
			// 							lesson.ThumbnailEncoded = Convert.ToBase64String(imageBytes);
			// 						}
			// 						catch (Exception e)
			// 						{
			// 							Debug.LogError($"Failed to read thumbnail file {thumbnailFile}: {e}");
			// 						}
			// 					}
			// 				}
			// 				
			// 				cachedLessons.Add(lesson);
			// 			}
			// 		}
			// 		catch (Exception e)
			// 		{
			// 			// Skip invalid files
			// 			Debug.LogError(e);
			// 		}
			// 	}
			// }
			//
			// return cachedLessons.OrderBy(cachedLesson => cachedLesson.Name).ToList();
		}

		public async UniTaskVoid LoadSkybox()
		{
			string path = await CreateClient().GetSkyboxPath();
			string url = CdnUrl(path);
			PlayerPrefs.SetString("defaultSkybox", url);
			PlayerPrefs.Save();
			sessionListeners.ForEach((listener) => listener.OnSkybox(url));
		}
		
		public async UniTaskVoid LoadUserCode()
		{
			try
			{
				Debug.Log("Requesting user code for Device: " + SystemInfo.deviceUniqueIdentifier);
				_userCode = await CreateClient().GetUserCode(SystemInfo.deviceUniqueIdentifier);;
				PlayerPrefs.SetString("userCode", _userCode);
				PlayerPrefs.Save();
				sessionListeners.ForEach((handler) => handler.OnUserCode(_userCode));
			} catch (HttpRequestException e)
            {
                if (Offline)
                {
	                FireError(Error.InternetRequired);
                }
	            else
	            {
					FireError(Error.FailedToLoadUserCode);    
	            }
            }
		}
		
		public static async UniTask<string> LoadAuthToken(string newUserCode = null)
		{
			if (newUserCode != null)
			{
				Instance._userCode = newUserCode;
			}

			try
			{
				string apiKey = await CreateClient().GetAuthToken(Instance._userCode);
				if (apiKey != null)
				{
					SetAuthToken(apiKey);
				}
				return apiKey;
			}
			catch (Exception e)
			{
				FireError(new Error(e));
				return null;
			}
		}

		public static async UniTask<SubmissionData> Submit(LessonData lesson)
		{
			SubmissionData submission = await CreateClient().Submit(lesson);
			Instance.sessionListeners.ForEach((handler) => handler.onSubmission(submission));
			return submission;
		}

		public static async UniTask<Texture2D> LoadTexture2D(FileData file)
		{
			return await TextureUtils.LoadTexture2D(file);
		}

		public static string GetCachePath(FileData file)
		{
			return file.Parent != null ? 
                System.IO.Path.Combine(Application.persistentDataPath, file.Parent.Guid, file.Guid, StringUtils.ReplaceSpaces(file.Name)) 
                : System.IO.Path.Combine(Application.persistentDataPath, file.Guid, StringUtils.ReplaceSpaces(file.Name));
		}

		public static bool IsCached(FileData file)
		{
			return System.IO.File.Exists(file.CachePath);
		}

		public static UnityWebRequest MakeCacheRequest(DownloadableFileData file)
		{
			UnityWebRequest request = new UnityWebRequest(file.Url, UnityWebRequest.kHttpVerbGET);
			System.IO.Directory.CreateDirectory(file.CachePath.Substring(0, file.CachePath.LastIndexOf("/") + 1));
			if (System.IO.File.Exists(file.TempCachePath))
			{
				System.IO.File.Delete(file.TempCachePath);
			}
			request.downloadHandler = new DownloadHandlerFile(file.TempCachePath);
			return request;
		}

		public static void FinalizeCache(FileData file)
		{
			if (file.TempCachePath == null || !File.Exists(file.TempCachePath))
			{
				// nothing to do
				return;
			}
			if (System.IO.File.Exists(file.CachePath))
			{
				System.IO.File.Delete(file.CachePath);
			}
			System.IO.File.Move(file.TempCachePath, file.CachePath);
		}

    }
}