using System.Collections.Generic;
using System;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace AccessVR.OrchestrateVR.SDK
{
	public class LessonData : AbstractData
	{
		[JsonProperty("scenes")] public List<SceneData> Scenes;
		[JsonProperty("id")] public int Id;
		[JsonProperty("embedKey")] public string EmbedKey;
		[JsonProperty("name")] public string Name;
		[JsonProperty("description")] public string Description;
		[JsonProperty("createdBy")] public string CreatedBy;
		[JsonProperty("guid")] public string Guid;
		[JsonProperty("initialScene")] public int InitialSceneId;
		[JsonProperty("existsInManifest")] public bool ExistsInManifest = true;
		[JsonProperty("startDate")] public DateTime StartDate;
		[JsonProperty("completeDate")] public DateTime CompleteDate;
		[JsonProperty("publishedDate")] public DateTime PublishedDate;
		[JsonProperty("score")] public float Score = -1.0f;
		[JsonProperty("previewImage")] public Sprite previewImage;
		[JsonProperty("thumbnailEncoded")] public string ThumbnailEncoded;
		[JsonProperty("completeMessage")] public string completeMessage;
		[JsonProperty("showSceneList")] public bool showSceneList = false;
		[JsonProperty("isLocked")] public bool isLocked = false;
		private bool updateIsAvailable = false;
		private bool _isDownloaded = false;
		private int? _downloadableAssetCount = null;
		private List<AssetData> downloadList;
		private bool IsSummary = true;
		private string DateFormatString = "M/d/yyyy h:mm:ss tt";

		public bool Preview { get; set; }

		public bool IsDownloaded => _isDownloaded;
		public int DownloadableAssetCount
		{
			get
			{
				if (_downloadableAssetCount == null)
				{
					_downloadableAssetCount = 0;
					if (Scenes?.Count > 0)
					{
						foreach (SceneData sceneData in Scenes)
						{
							_downloadableAssetCount += sceneData.Assets.Count;
						}
					}
				}
				return (int)_downloadableAssetCount;
			}
		}
		public bool UpdateIsAvailable
		{
			get { return updateIsAvailable; }
			set
			{
				updateIsAvailable = value;
				OnUpdateStatusChange?.Invoke(this, new EventArgs());
			}
		}
		public event EventHandler<EventArgs> OnUpdateStatusChange;

		public SceneData InitialScene()
		{
			foreach (var scene in Scenes)
			{
				if (scene.Id == InitialSceneId)
					return scene;
			}
			return null;
		}
		public SceneData SceneForId(int id)
		{
			foreach (var scene in Scenes)
			{
				if (scene.Id == id)
					return scene;
			}
			return null;
		}

		public LessonData(JObject jsonObject)
		{
			if (jsonObject["guid"] != null)
			{
				Guid = (string)jsonObject["guid"];
			}

			string contentPath = Path.Combine(Application.persistentDataPath, Guid, "content.json");
			if (File.Exists(contentPath))
            {
				string content = File.ReadAllText(contentPath);
				LoadFromJson(JObject.Parse(content));
				IsSummary = false;
			} else
            {
				LoadFromJson(jsonObject);
			}

			if (IsSummary)
			{
				DownloadPublishedLessonContent();
			}
		}

		public LessonData(string guid)
		{
			string contentPath = Path.Combine(Application.persistentDataPath, guid, "content.json");
			if (File.Exists(contentPath))
			{
				string content = File.ReadAllText(contentPath);
				LoadFromJson(JObject.Parse(content));
			}
		}

		public LessonData(int id, string embedKey)
		{
			Id = id;
			EmbedKey = embedKey;
		}

		public LessonData(int id, bool preview = false)
		{
			Id = id;
			Preview = preview;
		}

		public LessonData()
		{
			//
		}
		
		private void ResetIsDownloaded()
		{
			string dir = Path.Combine(Application.persistentDataPath, Guid);
			
			if (Directory.Exists(dir))
			{
				string contentPath = Path.Combine(dir, "content.json");
				if (File.Exists(contentPath))
				{
					bool allScenesDownloaded = true;
					foreach (SceneData scene in Scenes)
					{
						if (!scene.IsCached)
						{
							allScenesDownloaded = false;
						}
					}
					_isDownloaded = allScenesDownloaded;
				}
				else
                {
					_isDownloaded = false;
                }
			}
			else
			{
				_isDownloaded = false;
			}
		}

		private void LoadFromJson(JObject jsonObject)
		{
			#if UNITY_EDITOR
				Debug.Log("LessonData.LoadFromJson: " + jsonObject.ToString());
			#endif
			
			_downloadableAssetCount = null;
            
			Scenes = new List<SceneData>();

			if (jsonObject["publishedDate"] != null)
			{
				string dateString = jsonObject["publishedDate"].ToString();
				DateTime.TryParseExact(dateString, DateFormatString, null, System.Globalization.DateTimeStyles.AllowWhiteSpaces |
								   System.Globalization.DateTimeStyles.AdjustToUniversal,
								   out PublishedDate);
			}

			if (jsonObject["name"] != null)
				Name = jsonObject["name"].ToString();
			if (jsonObject["description"] != null)
				Description = jsonObject["description"].ToString();
			if (jsonObject["id"] != null)
				Id = (int)jsonObject["id"];
			if (jsonObject["guid"] != null)
			{
				Guid = (string)jsonObject["guid"];
			}

			if (jsonObject["unity_completeMessage"] != null)
			{
				UnityRichText unityCompleteMessage = new UnityRichText((JObject)jsonObject["unity_completeMessage"]);
				if (!String.IsNullOrEmpty(unityCompleteMessage.Content))
				{ 
					completeMessage = unityCompleteMessage.Content;
				}
			}

			if (jsonObject["showSceneList"] != null)
				showSceneList = (bool) jsonObject["showSceneList"];
			
			if (jsonObject["createdBy"] != null)
				CreatedBy = jsonObject["createdBy"].ToString();

			if (jsonObject["initialScene"] != null)
				InitialSceneId = (int) jsonObject["initialScene"];
			
			if (jsonObject["thumbnailUrl"] != null)
			{
				string thumbnailUrl = jsonObject["thumbnailUrl"].ToString();
				if (thumbnailUrl.Length > 0)
				{
					LoadPreviewImage(jsonObject["thumbnailUrl"].ToString());
				}
			}

			if (jsonObject["scenes"] != null)
			{
				foreach (JObject token in jsonObject["scenes"])
				{
					var scene = token.Value<SceneData>();
					Scenes.Add(scene);
				}
			}

			ResetIsDownloaded();
		}

		private void LoadPreviewImage(string path)
		{
			throw new Exception("We don't use LessonData.LoadPreviewImage anymore");

			// Texture2D tex = null;
			// path = OrchestrateEnvironment.CdnUrl(path);

			// if (!IsDownloaded)
			// {
			// 	Debug.Log(path);
			// 	TextureProccessFunctions.DownloadImage(path, (string error) =>
			// 	{
			// 		//Error downloading
			// 		Debug.LogError("Failed to load Lesson preview image " + path + ": " + error);
			// 	},
			// 	(Texture2D texture2D) =>
			// 	{
			// 		//Successfully Downloaded the texture
			// 		tex = texture2D;
			// 		previewImage = TextureProccessFunctions.ConvertTextureToSprite(tex);
			// 	});
			// }
			// else
			// {
			// 	tex = TextureProccessFunctions.LoadTextureLocalFile(path);
			// 	if (tex != null)
			// 	{
			// 		previewImage = TextureProccessFunctions.ConvertTextureToSprite(tex);
			// 	}
			// }
		}
		

		public async Task<bool> DownloadPublishedLessonContent()
		{
			string path = "/api/rest/published-lesson/" + Id;
			
			if (!String.IsNullOrEmpty(EmbedKey))
			{
				path += "/" + EmbedKey;
			}
			
			if (Preview)
			{
				path += "?preview=1";
			}
			
			string url = OrchestrateEnvironment.GetUrl(path);
			
			var client = OrchestrateEnvironment.CreateClient();

			if (Preview)
			{
				Debug.Log("Loading preview Lesson data: " + url);
			}
			else
			{
				Debug.Log("Loading published Lesson data: " + url);
			}

			try
			{
				HttpResponseMessage response = await client.GetAsync(url);
				string responseBody = await HttpUtils.AssertSuccessfulResponse(response);
				
				// Debug.Log(responseBody);
				
				JObject rootObject = JObject.Parse(responseBody);
				
				if (rootObject["result"] != null)
				{
					JObject root = rootObject["result"].ToObject<JObject>();
					Guid = root["guid"].ToString();
					
					if (root["content"] != null)
					{
						if (!Preview)
						{
							string content = root["content"].ToString();
							Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, Guid));
							File.WriteAllText(Path.Combine(Application.persistentDataPath, Guid, "content.json"),
								content);
							LoadFromJson(JObject.Parse(content));
						}
						else
						{
							LoadFromJson(root["content"].ToObject<JObject>());
						}
						
						IsSummary = false;
						return true;
					}
					else
                    {
						Debug.Log("Error reading JSON, no content");

						return false;
                    }
				}
				else
				{
					Debug.Log("Error reading JSON");
					Debug.Log(response);
					
					return false;
				}
			}
			catch (HttpRequestException e)
			{
				Debug.Log("Load Course Data: " + e.Message + "\nURL that was requested: " + url);
				
				return false;
			}
		}

		public async void StartDownload([CanBeNull] Action callOnStarted = null)
		{
			if (IsSummary || String.IsNullOrEmpty(Guid))
			{
				if (await DownloadPublishedLessonContent())
				{
					StartDownloadingAssets();	
					ResetIsDownloaded();
					callOnStarted?.Invoke();
				}
				else
				{
					throw new Exception("Failed to Start Downloading Lesson");
				}
			}
			else
			{
				StartDownloadingAssets();
				ResetIsDownloaded();
				callOnStarted?.Invoke();
			}
		}

		private void StartDownloadingAssets()
		{
			LessonDownloadJob job = new LessonDownloadJob();
			downloadList = new List<AssetData>();

			foreach (SceneData scene in Scenes)
			{
				foreach (AssetData asset in scene.Assets)
				{
					if (!asset.IsCached())
					{
						downloadList.Add(asset);
					}
				}
			}

			job.assets = downloadList;
			job.guid = Guid;
			DownloadHandler.Instance.StartJob(job);
		}

		public void RemoveCachedContent()
		{
			try
			{
				File.Delete(Path.Combine(Application.persistentDataPath, Guid, "content.json"));
				_isDownloaded = false;
				IsSummary = true;
			}
			catch (IOException e)
			{
				Debug.LogError("Error deleting Lesson content: " + e.Message);
			}
		}
            
		public void CancelDownload()
		{
			DownloadHandler.Instance.CancelJob(Guid);
			RemoveCachedContent();
		}

		public void RemoveCachedContentAndAssets()
		{
			try
			{
				Directory.Delete(Path.Combine(Application.persistentDataPath, Guid), true);
				_isDownloaded = false;
				IsSummary = true;
			}
			catch (IOException ex)
			{
				Debug.Log("Error deleting directory: " + ex.Message);
			}
		}
		
	}
}
