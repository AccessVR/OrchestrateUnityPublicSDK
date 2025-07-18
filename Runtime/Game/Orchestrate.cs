using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using JetBrains.Annotations;
using Cysharp.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace AccessVR.OrchestrateVR.SDK
{
	public enum Environment
	{
		Prod,
		Stage,
		Dev,
		Local,
		Custom,
	}
	
    public class Orchestrate : GenericSingleton<Orchestrate>
    {
	    [SerializeField]
	    protected Environment environment = Environment.Prod;

	    public StyleRefSettings colorStyles;
		
        public static StyleRefSettings ColorStyles => Instance.colorStyles;
	    
        private string _customBaseUrl = null;

        private string _customCdnUrl = null;

        private LessonData _activeExperience;
        
        private int _activeAssignmentId;
        
	    private List<LessonData> _cachedLessons;
        
        private UserData _user;

        private string _authToken;

        private string _userCode;

        private List<LessonSummaryData> _cachedLessonSummaries;
        
	    [CanBeNull]
        public static UserData User => GetUser();

        public static void SetEnvironment(Environment name)
        {
	        Instance.environment = name;
        }

        public static Environment GetEnvironment()
        {
	        return Instance.environment;
        }

        public static bool Is(Environment environmentName)
        {
	        return Instance.environment == environmentName;
        }

        public static void SetAuthToken(string token)
        {
	        PlayerPrefs.SetString(GetEnvironmentPrefKey("apiKey"), token);
	        PlayerPrefs.Save();
            Instance._authToken = token;
        }

        public static string GetAuthToken()
        {
	        if (!String.IsNullOrEmpty(Instance._authToken))
	        {
		        return Instance._authToken;
	        }
	        return PlayerPrefs.GetString(GetEnvironmentPrefKey("apiKey")) ?? PlayerPrefs.GetString("apiKey");
        }

        public static void SetBaseUrl(string url)
        {
            Instance._customBaseUrl = url;
        }

        public static void SetActiveExperience(LessonData experience)
        {
            Instance._activeExperience = experience;
        }

        public static void SetActiveAssignmentId(int assignmentId)
        {
	        Instance._activeAssignmentId = assignmentId;
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

        public static int GetActiveAssignmentId()
        {
	        return Instance._activeAssignmentId;
        }

        public static bool HasNativeKeyboard()
        {
	        return Application.platform == RuntimePlatform.IPhonePlayer;
        }
        
        public static bool IsTouchscreenPlayer() 
        {
	        bool assumeIPhonePlayer = false;
	        #if UNITY_IOS
				assumeIPhonePlayer = true;
			#endif
	        return assumeIPhonePlayer || Application.platform == RuntimePlatform.IPhonePlayer;
        }

        public static string GetBaseUrl([CanBeNull] string path = null)
        {
	        if (Is(Environment.Custom) && !String.IsNullOrEmpty(Instance._customBaseUrl))
	        {
		        return Instance._customBaseUrl;
	        }

	        string baseUrl = Instance.environment switch
            {
                Environment.Prod => "https://app.orchestratevr.com",
                Environment.Stage => "https://orchestrate-stage.accessvr.com",
                Environment.Dev => "https://orchestrate-dev.accessvr.com",
                Environment.Local => "https://ovr.avr.ngrok.io",
                _ => "http://localhost"
            };
	        
	        return baseUrl + (path != null ? "/" + path.TrimStart('/') : "");
        }

        public static string ProfileUrl(int userId)
		{
			return ProfileUrl(userId.ToString());
		}
		
		public static string ProfileUrl()
		{
			return GetUrl("/profile");
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
	        string cdnBaseUrl = Instance.environment switch
            {
                Environment.Prod => "https://files.accessvr.com",
                _ => "https://dev.files.ovr.accessvr.com"
            };
            if (Is(Environment.Custom) && !String.IsNullOrEmpty(Instance._customCdnUrl))
            {
                cdnBaseUrl = Instance._customCdnUrl;
            }
            return cdnBaseUrl + (path != null ? "/" + path.TrimStart('/') : "");
        }

        public static string GetUrl(string path = null)
        {
            return GetBaseUrl(path);
        }

        public static HttpClient CreateClient()
        {
            return HttpClient.Create(GetBaseUrl(), GetAuthToken());
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
        
        private float _testOfflineStateFrequency = 1.0f; // seconds
        
        private float _timeSinceLastOfflineStateTest = 0.0f;
        
        private bool _lastIsOfflineValue;

        public static string Version => "v" + Application.version + " / " + Application.unityVersion;

        public override void Awake()
        {
            base.Awake();
            _lastIsOfflineValue = IsOffline;
        }

        private void Update()
        {
            _timeSinceLastOfflineStateTest += Time.deltaTime;
            if (_timeSinceLastOfflineStateTest > _testOfflineStateFrequency)
            {
                _timeSinceLastOfflineStateTest = 0;
                if (_lastIsOfflineValue != IsOffline)
                {
                    offlineStateListeners.ForEach((listener) => listener.OnOfflineStateChanged(IsOffline, _lastIsOfflineValue));
                }
                _lastIsOfflineValue = IsOffline;
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
	            bool lastOfflineValue = IsOffline;
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
        
        public static bool IsOffline
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
	        return _user != null || PlayerPrefs.HasKey(GetEnvironmentPrefKey("apiKey")) || PlayerPrefs.HasKey("apiKey");
        }

        public static string GetEnvironmentPrefKey(string key)
        {
	        return $"{GetEnvironment().ToString()}.{key}";
        }

        public static async UniTask<UserData> LoadUser([CanBeNull] Action<UserData> onUserDataLoaded = null)
        {
	        if (String.IsNullOrEmpty(GetAuthToken()))
	        {
		        if (!IsOffline)
		        {
			        FireError(Error.NotAuthenticated);
		        }
		        else
		        {
			        FireError(Error.InternetRequired);
		        }

		        return null;
	        }

	        if (PlayerPrefs.HasKey(GetEnvironmentPrefKey("user")))
	        {
				SetUser(JsonConvert.DeserializeObject<UserData>(PlayerPrefs.GetString(GetEnvironmentPrefKey("user"))));;
				
			// Legacy support for userid storage	
	        } else if (PlayerPrefs.HasKey("userid"))
			{
				UserData user = new UserData();
				user.UserId = int.Parse(PlayerPrefs.GetString("userid"));
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
					PlayerPrefs.SetString(GetEnvironmentPrefKey("user"), JsonConvert.SerializeObject(user));;
					PlayerPrefs.Save();
					
					SetUser(user);
				}
				catch (HttpRequestException e)
				{
					FireError(new Error(e));
					return null;
				}
			}
			
			if (!IsOffline)
			{
				if (!await Instance.IsLoggedIn())
				{
					FireError(Error.NotAuthenticated);
					return null;
				}
			}

			onUserDataLoaded?.Invoke(GetUser());
			Instance.sessionListeners.ForEach((handler) => handler.OnUserData(GetUser()));
			return GetUser();
		}
		
		public async UniTask<bool> IsLoggedIn()
		{
			return await CreateClient().IsLoggedIn();
		}

		public static void Logout()
		{
			SetAuthToken(null);
			Debug.Log("Logout / SethAuthToken(null)");
			SetUser(null);
			Debug.Log("Logout / SetUser(null)");
			PlayerPrefs.DeleteAll();
			Debug.Log("Logout / PlayerPrefs.DeleteAll()");
			PlayerPrefs.Save();
			Debug.Log("Logout / PlayerPrefs.Save()");
			Instance.sessionListeners.ForEach((handler) => handler.OnLogout());
			Debug.Log("Logout / sessionListeners.OnLogOut()");
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
        
		public static async UniTask<LessonData> LoadLesson(LessonDataLookup lookup)
		{
			NumberUtils.AssertNotNullOrEmpty(lookup.Id);
			LessonData lesson = await CreateClient().GetLesson(lookup);
			StringUtils.AssertNotNullOrEmpty(lesson.Guid);
			WriteCache(lesson.FileData, JsonConvert.SerializeObject(lesson));
			Debug.Log("Clearing stored list of cached lessons");
			Instance._cachedLessonSummaries = null;
			return lesson;
		}

		public static bool CacheContainsLesson(string guid)
		{
			LessonData lesson = LessonData.Make(StringUtils.AssertNotNullOrEmpty(guid));
			if (File.Exists(GetLegacyCachePath(lesson)))
			{
				return true;
			}
			return CacheContains(lesson.FileData);
		}

		private static string GetLegacyCachePath(LessonData lesson)
		{
			// Legacy cache path for Lesson: {lesson.Guid}/content.json
			StringUtils.AssertNotNullOrEmpty(lesson.Guid);
			return Path.Combine(Application.persistentDataPath, lesson.Guid, "content.json");
		}

		public static bool CacheContainsLessonAndDownloadables(string guid)
		{
			if (!CacheContainsLesson(guid))
			{
				return false;
			}
			LessonData lesson = LessonData.Make(StringUtils.AssertNotNullOrEmpty(guid));
			foreach (DownloadableFileData file in lesson.GetDownloadableFiles())
			{
				if (!CacheContains(file))
				{
					return false;
				}
			}
			return true;
		}

		public static bool CacheContains(LessonData lesson)
		{
			StringUtils.AssertNotNullOrEmpty(lesson.Guid);
			return CacheContainsLessonAndDownloadables(lesson.Guid);
		}

		public static bool CacheContains(AssetData asset)
		{
			return CacheContains(asset.FileData) && (!asset.HasThumbnail() || CacheContains(asset.Thumbnail.FileData));
		}
		
		public static bool CacheContains(FileData file)
		{
			// TODO: in a future release, when we stop caring about Legacy, just move to GetCachePath()
			return File.Exists(GetNewCachePath(file)) || File.Exists(GetLegacyCachePath(file));
		}

		public static LessonData LoadCachedLesson(string guid)
		{
			LessonData lesson = LessonData.Make(StringUtils.AssertNotNullOrEmpty(guid));
			string legacyCachePath = GetLegacyCachePath(lesson);
			if (File.Exists(legacyCachePath))
			{
				return JsonConvert.DeserializeObject<LessonData>(ReadCache(legacyCachePath));
			}
			if (!CacheContains(lesson.FileData))
			{
				throw new InvalidOperationException("Could not find lesson in cache: " + guid);
			}
			return JsonConvert.DeserializeObject<LessonData>(ReadCache(lesson.FileData));
		}

		public static LessonData LoadCachedLesson(LessonDataLookup lookup)
		{
			return LoadCachedLesson(lookup.Guid);
		}

		public static LessonData LoadCachedLesson(FileData file)
		{
			return LoadCachedLesson(file.Guid);
		}

		public static void DeleteCachedLesson(LessonDataLookup lookup, bool removeContentAndDownloadables = false)
		{
			DeleteCachedLesson(lookup.Guid, removeContentAndDownloadables);
		}

		public static void DeleteCachedLesson(string guid, bool removeContentAndDownloadables = false)
		{
			LessonData lesson = LessonData.Make(StringUtils.AssertNotNullOrEmpty(guid));
			if (CacheContainsLesson(lesson.Guid))
			{
				lesson = LoadCachedLesson(lesson.Guid);
			}
			if (removeContentAndDownloadables)
			{
				lesson.GetDownloadableFiles().ForEach(DeleteCache);
			}
			string legacyCachePath = GetLegacyCachePath(lesson);
			if (File.Exists(legacyCachePath))
			{
				File.Delete(legacyCachePath);
			}
			DeleteCache(lesson.FileData);
			if (Instance._cachedLessonSummaries != null)
			{
				Instance._cachedLessonSummaries.RemoveAll((summary) => summary.Guid == lesson.Guid);
			}
		}

		public static void DeleteCache(FileData file)
		{
			string legacyCachePath = GetLegacyCachePath(file);
			if (File.Exists(legacyCachePath))
			{
				File.Delete(legacyCachePath);
			}
			string cachePath = GetNewCachePath(file);
			if (File.Exists(cachePath))
			{
				File.Delete(cachePath);
			}
		}
		
		protected static string GetGuidFromDirectoryPath(string path)
		{
			return path.Split('/').Last();
		}

		private static List<LessonSummaryData> GetLegacyCachedLessons()
		{
			List<string> envList = new List<string>
			{
				"local",
				"dev",
				"stage",
				"prod",
				"custom"
			};

			return Directory.GetDirectories(Application.persistentDataPath)
				.Where(dir => envList.Find(env => env == dir) == null)
				.Select(dir =>
				{
					if (File.Exists(Path.Combine(dir, "content.json")))
					{
						return LoadCachedLesson(GetGuidFromDirectoryPath(dir));
					}
					return null;
				})
				.Where(lesson => lesson != null)
				.OrderBy(lesson => lesson.Name)
				.Select(lesson => lesson.Summary)
				.ToList();
		}

		private static List<LessonSummaryData> GetCachedLessonsForEnvironment(Environment environment)
		{
			string path = Path.Combine(Application.persistentDataPath, environment.ToString().ToLower(), typeof(LessonData).ToString());
			if (!Directory.Exists(path))
			{
				return new();
			}
			
			return Directory.GetDirectories(path)
				.Select(dir =>
				{
					if (File.Exists(Path.Combine(dir, "content.json")))
					{
						return LoadCachedLesson(GetGuidFromDirectoryPath(dir));
					}
					return null;
				})
				.Where(lesson => lesson != null)
				.OrderBy(lesson => lesson.Name)
				.Select(lesson => lesson.Summary)
				.ToList();
		}
		
		public static List<LessonSummaryData> GetCachedLessonList()
		{
			if (Instance._cachedLessonSummaries == null)
			{
				Debug.Log("GetCachedLessonList: building list");
				List<LessonSummaryData> lessons = new ();
				lessons.AddRange(GetCachedLessonsForEnvironment(GetEnvironment()));
				lessons.AddRange(GetLegacyCachedLessons());
				Instance._cachedLessonSummaries = lessons;	
			}
			return Instance._cachedLessonSummaries;
		}

		public async UniTaskVoid LoadSkybox()
		{
			string path = await CreateClient().GetSkyboxPath();
			string url = GetCdnUrl(path);
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
                if (IsOffline)
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

		public static async UniTask<SubmissionData> Submit(SubmissionData submission)
		{
			SubmissionData output = await CreateClient().Submit(submission);
			Instance.sessionListeners.ForEach((handler) => handler.OnSubmission(output));
			return output;
		}

		public static async UniTask<Texture2D> LoadTexture2D(FileData file)
		{
			if (!CacheContains(file))
			{
				throw new Exception($"File is not cached, can't load texture: ${file.Name}");
			}
			return await TextureUtils.LoadTexture2D(GetCachePath(file));
		}

		public static string GetCachePath(FileData file)
		{
			// If a file exists in the new path, return new path
			string newCachePath = GetNewCachePath(file);
			if (File.Exists(newCachePath))
			{
				return newCachePath;
			}
			// Look for legacy cache
			string legacyCachePath = GetLegacyCachePath(file);
			if (File.Exists(legacyCachePath))
			{
				return legacyCachePath;
			}
			// Look for web-formatted legacy cache
			string legacyWebCachePath = GetLegacyCachePath(file.LegacyWebFileData);
			if (File.Exists(legacyWebCachePath))
			{
				return legacyWebCachePath;
			}
			// Fall back on new cache path
			return newCachePath;
		}

		private static string GetNewCachePath(FileData file)
		{
			// New Cache path is {file.Env}/{file.Type}/{file.Guid}/{file.Name}
			return Path.Combine(Application.persistentDataPath, StringUtils.ReplaceSpaces(file.Env.ToLower()), file.Type.ToString(), file.Guid, StringUtils.ReplaceSpaces(file.Name));
		}

		private static string GetLegacyCachePath(FileData file)
		{
			// Legacy Cache path is {parentLesson.Guid}/{file.Env}/{file.Guid}/{file.Name}
			if (file.Parent != null)
			{
				return Path.Combine(Application.persistentDataPath, file.Parent.Guid, StringUtils.ReplaceSpaces(file.Env.ToLower()), file.Guid, StringUtils.ReplaceSpaces(file.Name));
			}
			return GetNewCachePath(file);
		}

		public static string GetCachePath(AssetData asset)
		{
			return GetCachePath(asset.FileData);
		}

		public static string GetTempCachePath(FileData file)
		{
			return GetNewCachePath(file) + ".tmp";
		}

		public static UnityWebRequest MakeCacheRequest(DownloadableFileData file)
		{
			UnityWebRequest request = new UnityWebRequest(file.Url, UnityWebRequest.kHttpVerbGET);
			CreateCacheDirectory(file);
			string tempCachePath = GetTempCachePath(file);
			if (File.Exists(tempCachePath))
			{
				File.Delete(tempCachePath);
			}
			request.downloadHandler = new DownloadHandlerFile(tempCachePath);
			return request;
		}

		protected static void CreateCacheDirectory(FileData file, bool forceUseNewCachePath = false)
		{
			string cachePath = forceUseNewCachePath ? GetNewCachePath(file) : GetCachePath(file);
			Directory.CreateDirectory(cachePath.Substring(0, cachePath.LastIndexOf("/") + 1));
		}

		public static void WriteCache(FileData file, string contents)
		{
			CreateCacheDirectory(file, true);
			File.WriteAllText(GetNewCachePath(file), contents);
		}

		public static string EncodeCachedBytes([CanBeNull] FileData file)
		{
			if (file == null)
			{
				return null;
			}
			return Convert.ToBase64String(ReadCachedBytes(file));
		}

		public static byte[] ReadCachedBytes(FileData file)
		{
			return File.ReadAllBytes(GetCachePath(file));
		}
		
		public static string ReadCache(FileData file)
		{
			return ReadCache(GetCachePath(file));
		}

		private static string ReadCache(string path)
		{
			return File.ReadAllText(path);
		}

		public static void FinalizeTempCache(FileData file)
		{
			string cachePath = GetNewCachePath(file);
			string tempCachePath = GetTempCachePath(file);
			if (!File.Exists(tempCachePath))
			{
				throw new InvalidOperationException("File has not be temporarily stored yet and cache does not exist.");
			}
			if (File.Exists(cachePath))
			{
				File.Delete(cachePath);
			}
			File.Move(tempCachePath, cachePath);
		}

    }
}