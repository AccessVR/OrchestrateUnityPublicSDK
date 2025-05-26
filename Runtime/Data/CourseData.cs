using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AccessVR.OrchestrateVR.SDK
{
	public class CourseData : AbstractData
	{
		[JsonProperty("lessons")] public List<LessonData> lessons;
		[JsonProperty("lastModified")] public DateTime lastModified;
		//public DateTime assignedDate;
		//public DateTime completedDate;
		public string DateFormatString = "M/d/yyyy h:mm:ss tt";
		[JsonProperty("currentProg")] public int currentProg;
		[JsonProperty("maxProg")] public int maxProg = 1;
		//public string createdBy;
		[JsonProperty("title")] public string title;
		[JsonProperty("description")] public string description;
		[JsonProperty("previewImage")] public Sprite previewImage;
		[JsonProperty("id")] public int Id; //id of the user, to know which course to download
		[JsonProperty("guid")] public string Guid; //unique number representing the course file being downloaded, used as a savename to find it later
		

		private WebClient webClient;

		private bool isValid = false;
		public bool IsValid { get { return isValid; } }
		private bool isLocal = false;
		public bool IsLocal { get { return isLocal; } }
		private bool updateIsAvailable = false;
		public bool UpdateIsAvailable
		{
			get { return updateIsAvailable; }
			set { updateIsAvailable = value; }
		}


		public event EventHandler<EventArgs> OnComplete;
		//public event EventHandler<DownloadProgressEventArgs> OnProgress;
		//public event EventHandler<EventArgs> OnUpdateStatusChange;
		
		
		//Create and Initialize a course data object
		public CourseData(JToken jsonToken)
		{
			//webClient = new WebClient();
			JObject courseObject = jsonToken.ToObject<JObject>();
			LoadFromJson(courseObject);
		}

		//Constructor
		//attempts to load a course from local files
		//currently unused, shall be deleted
		public CourseData(string guid)
		{
			if (Directory.Exists(Path.Combine(Application.persistentDataPath, guid)))
			{
				string filePath = Path.Combine(Application.persistentDataPath, guid + "/course.json");
				JObject storedJson;
				if (!File.Exists(filePath))
				{
					Debug.Log("Could not find local json: " + filePath);
					return;
				}
				try
				{
					storedJson = JObject.Parse(File.ReadAllText(filePath));
					//IsSummary = false;
					isLocal = true;
					LoadFromJson(storedJson);

				}
				catch (JsonReaderException ex)
				{
					Debug.Log("Error reading local json: " + ex.Message);
				}
			}
		}

		public void LoadFromJson(JObject jsonObject)
		{
			if (jsonObject["name"] != null && jsonObject["name"].Type != JTokenType.Null)
				title = jsonObject["name"].ToString();
			
			if (jsonObject["description"] != null && jsonObject["description"].Type != JTokenType.Null)
				description = jsonObject["description"].ToString();
			
			if (jsonObject["id"] != null && jsonObject["id"].Type != JTokenType.Null)
				Id = (int)jsonObject["id"];
			
			if (jsonObject["guid"] != null && jsonObject["guid"].Type != JTokenType.Null)
				Guid = (string)jsonObject["guid"];
			
			if (jsonObject["thumbnailUrl"] != null && jsonObject["thumbnailUrl"].Type != JTokenType.Null)
			{
				string thumbnailUrl = jsonObject["thumbnailUrl"].ToString();
				if (thumbnailUrl.Length > 0)
				{
					LoadPreviewImage(jsonObject["thumbnailUrl"].ToString());
				}
			}

			//Grab Lesson Data
			LoadLessons(jsonObject);

			//sets this data to be marked as valid, aka was read correctly
			isValid = true;
		}
		
		
		private void LoadPreviewImage(string path)
		{
			path = OrchestrateEnvironment.CdnUrl(path);
			Texture2D tex = null;

			if (!isLocal)
			{
 				TextureProccessFunctions.DownloadImage(path, (string error) => 
				{
					//Error downloading
					Debug.LogError("Failed to load Course preview image " + path + ": " + error);
				}, 
				(Texture2D texture2D) => 
				{
					//Successfully Downloaded the texture
					tex = texture2D;
					previewImage = TextureProccessFunctions.ConvertTextureToSprite(tex);
				});
			}
			else
			{
				path = StringFormattingFunctions.ReplaceSpaces(path);
				tex = TextureProccessFunctions.LoadTextureLocalFile(path);

				if (tex != null)
					previewImage = TextureProccessFunctions.ConvertTextureToSprite(tex);
			}
		}
		
		
		//loads lessonData objects into a list to be passed into the CourseMenu later
		private void LoadLessons(JObject jsonObject)
		{
			lessons = new List<LessonData>();

			if (jsonObject["lessons"] != null)
			{
				JArray jLessons = (JArray)jsonObject["lessons"];

				foreach (JToken token in jLessons)
				{
					JObject lessonObj = token.ToObject<JObject>();
					lessons.Add(new LessonData(lessonObj));
				}
			}
		}
	}
}
