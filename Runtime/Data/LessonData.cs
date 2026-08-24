using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;
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
		// Nullable, all three: a draft previewed before its first publish has
		// no publishedDate (and may have no schedule window), and Json.NET
		// throws converting null into a bare DateTime — which killed the
		// whole lesson load for preview launch codes.
		[JsonProperty("startDate")] public DateTime? StartDate;
		[JsonProperty("completeDate")] public DateTime? CompleteDate;
		[JsonProperty("publishedDate")] public DateTime? PublishedDate;
		[JsonProperty("unity_completeMessage")] private UnityRichText _unityCompleteMessage;
		[JsonProperty("completeMessage")] public string CompleteMessage;
		[JsonProperty("showSceneList")] public bool ShowSceneList = false;
		[JsonProperty("isLocked")] public bool IsLocked = false;

		/// <summary>
		/// Version identity of the published content this payload represents.
		/// Stamped by HttpClient.GetLesson from the Lesson resource (they sit
		/// beside `content`, like guid) and round-tripped through the lesson
		/// cache so an offline run still knows what it played. The row id goes
		/// back on the submission (lessonVersionId, server-validated); the
		/// human-meaningful number rides analytics context meta
		/// (lessonVersionNumber) so a reader outside our database can tell
		/// which content was on screen.
		/// </summary>
		[JsonProperty("publishedLessonId")] public int? PublishedLessonId;
		[JsonProperty("publishedVersionNumber")] public int? PublishedVersionNumber;

		/// <summary>
		/// True when this payload was fetched through a preview launch code
		/// (?preview=hash). Preview runs are not tracked — the native analogue
		/// of the web viewer's edit/preview gating. Serialized so a cached
		/// preview payload stays recognizable.
		/// </summary>
		[JsonProperty("isPreview")] public bool IsPreview = false;
		[JsonProperty("scenes")] public List<SceneData> Scenes = new();
		
		[JsonIgnore]
		public LessonSummaryData Summary
		{
			get
			{
				
				LessonSummaryData summary = new LessonSummaryData
				{
					Id = Id,
					Name = Name,
					Guid = Guid,
				};
				try
				{

					if (Thumbnail != null)
					{
						string thumbnailPath = Orchestrate.GetCachePath(Thumbnail.FileData);
						FileInfo info = new FileInfo(thumbnailPath);
						Debug.Log($"Thumbnail {Thumbnail.FileData.Name} {info.Length} bytes ({thumbnailPath})");
						if (info.Length <= 1048576)
						{
							summary.ThumbnailEncoded = Orchestrate.EncodeCachedBytes(Thumbnail.FileData);	
						}
					}
				}
				catch (IOException e)
				{
					// ignore
				}
				return summary;
			}
		}

		[JsonIgnore] 
		[CanBeNull] 
		public SceneData InitialScene => Scenes.First(scene => scene.Id == InitialSceneId);

		[JsonIgnore] 
		[CanBeNull] 
		private AssetData Thumbnail => InitialScene?.Thumbnail;
		
		public List<DownloadableFileData> GetDownloadableFiles()
		{
			return Scenes.Select(scene => scene.GetDownloadableFiles())
				.SelectMany(fileList => fileList)
				.Where(file => file != null)
				.Distinct()
				.ToList();
		}

		public SceneData SceneForId(int id) => Scenes.First(scene => scene.Id == id);
		
		[JsonIgnore] public FileData FileData => new (Orchestrate.GetEnvironment(), GetType(), Guid, "content.json");
		
		[JsonIgnore] public FileData PreviewFileData => new (Orchestrate.GetEnvironment(), GetType(), Guid, "content-preview.json");

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
				CompleteMessage = _unityCompleteMessage.Content;
			}
		}
	}
}
