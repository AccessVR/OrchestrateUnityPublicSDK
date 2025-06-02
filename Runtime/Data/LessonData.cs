using System.Collections.Generic;
using System;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace AccessVR.OrchestrateVR.SDK
{
	public class LessonData : Data, IDownloadable
	{
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
		[JsonProperty("unity_completeMessage")] private UnityRichText _unityCompleteMessage;
		[JsonProperty("completeMessage")] public string completeMessage;
		[JsonProperty("showSceneList")] public bool showSceneList = false;
		[JsonProperty("isLocked")] public bool IsLocked = false;
		[JsonProperty("scenes")] public List<SceneData> Scenes = new();

		[JsonIgnore] public SceneData InitialScene => Scenes.First(scene => scene.Id == InitialSceneId);
		
		public List<DownloadableFileData> GetDownloadableFiles()
		{
			return Scenes.Select(scene => scene.GetDownloadableFiles())
				.SelectMany(fileList => fileList)
				.Where(file => file != null)
				.Distinct()
				.ToList();
		}

		public SceneData SceneForId(int id) => Scenes.First(scene => scene.Id == id);
		
		[JsonIgnore] public FileData FileData => new (Orchestrate.GetEnvironment(), Guid, "content.json");
		
		[JsonIgnore] public FileData PreviewFileData => new (Orchestrate.GetEnvironment(), Guid, "content-preview.json");

		public static LessonData Make(string guid)
		{
			return new LessonData()
			{
				Guid = guid
			};
		}

		[OnDeserialized]
		internal void OnDeserialized(StreamingContext context)
		{
			Scenes.ForEach(scene => scene.SetParentLesson(this));
			if (_unityCompleteMessage != null)
			{
				completeMessage = _unityCompleteMessage.Content;
			}
		}
	}
}
