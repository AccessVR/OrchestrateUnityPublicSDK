using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace AccessVR.OrchestrateVR.SDK
{
	public interface DownloadHandlerListener
	{
		public void OnDownloadProgress(object downloadHandler, DownloadProgressEventArgs args);

		public void OnDownloadFailure();

		public void OnDownloadComplete();
		
	}
	
	public class LessonDownloadJob
	{
		public string guid;
		public List<AssetData> assets = new List<AssetData>();
		public int filesDownloaded = 0;
		public bool isComplete = false;
		public int retries = 0;
		public bool isCanceled = false;
		public UnityWebRequest currentRequest;
		
		public Action OnFailure;
		public Action OnComplete;
		public event EventHandler<DownloadProgressEventArgs> OnProgress;

		public void ReportProgress()
		{
			float progress = (currentRequest != null ? currentRequest.downloadProgress * 100 : 0) + (filesDownloaded * 100f);
			OnProgress?.Invoke(this, new DownloadProgressEventArgs(progress));
		}
	}

	public class DownloadProgressEventArgs : EventArgs
	{
		public DownloadProgressEventArgs(float progress)
		{
			Progress = progress;
		}

		public float Progress { get; set; }
	}


	public class DownloadHandler : GenericSingleton<DownloadHandler>
	{
		private const int MAX_RETRIES = 5;
		
		private List<LessonDownloadJob> queue = new List<LessonDownloadJob>();

		private UnityWebRequest request;

		public void StartJob(LessonDownloadJob job)
		{
			queue.Add(job);

			StartCoroutine(StartNextDownload(job));
		}

		private void Update()
		{
			foreach (LessonDownloadJob job in queue)
			{
				if (!job.isCanceled)
				{
					job.ReportProgress();
				}
			}
		}

		private IEnumerator StartNextDownload(LessonDownloadJob job)
		{
			if (OrchestrateEnvironment.Offline)
			{
				// XXX: This needs to bubble up somehow
				// OrchestrateController.Instance.ShowError("Please connect to the Internet to download new content. (4)");
				
				OnJobFailure(job);
			}
			
			if (!job.isCanceled)
			{
				if (job.assets.Count > 0)
				{
					bool error = false;
					
					AssetData asset = job.assets.First();
					
					// XXX: require authentication for CDN access
					string assetUrl = OrchestrateEnvironment.GetCdnUrl(asset.path);

					if (asset.IsVideo() && !String.IsNullOrEmpty(asset.originalPath))
					{
						assetUrl = OrchestrateEnvironment.GetCdnUrl(asset.originalPath);
					}
					
					job.currentRequest = new UnityWebRequest(assetUrl, UnityWebRequest.kHttpVerbGET);
					Directory.CreateDirectory(asset.localPath.Substring(0, asset.localPath.LastIndexOf("/") + 1));
					if (File.Exists(asset.tempLocalPath))
					{
						File.Delete(asset.tempLocalPath);
					}

					job.currentRequest.downloadHandler = new DownloadHandlerFile(asset.tempLocalPath);
					
					yield return job.currentRequest.SendWebRequest();
					if (job.currentRequest.result != UnityWebRequest.Result.Success)
					{
						Debug.LogError($"Failed to download Asset {assetUrl}");
						OnDownloadError(job);
						error = true;
					}

					if (asset.HasThumbnail())
					{
						// XXX: require authentication for CDN access
						string assetThumbnailUrl = OrchestrateEnvironment.GetCdnUrl(asset.thumbnailPath);
						job.currentRequest = new UnityWebRequest(assetThumbnailUrl, UnityWebRequest.kHttpVerbGET);
						Directory.CreateDirectory(asset.thumbnailLocalPath.Substring(0, asset.thumbnailLocalPath.LastIndexOf("/") + 1));
						if (File.Exists(asset.tempThumbnailLocalPath))
						{
							File.Delete(asset.tempThumbnailLocalPath);
						}

						job.currentRequest.downloadHandler = new DownloadHandlerFile(asset.tempThumbnailLocalPath);
						
						yield return job.currentRequest.SendWebRequest();
						if (job.currentRequest.result != UnityWebRequest.Result.Success)
						{
							Debug.LogError($"Failed to download Asset {assetThumbnailUrl}");
							OnDownloadError(job);
							error = true;
						}
					}
					
					
					if (!error)
					{
						OnDownloadComplete(job);
					}

				}
				else
				{
					OnJobComplete(job);
				}
			}
		}

		public void CancelJob(string lessonGuid)
		{
			GetJobs(lessonGuid).ForEach((job =>
			{
				job.isCanceled = true;
				job.isComplete = false;
				queue.Remove(job);
			}));
			// TODO: RemoveCorruptedDownload(downloadInfo);
		}
		
		private void OnDownloadComplete(LessonDownloadJob job)
		{
			try
			{
				if (File.Exists(job.assets[0].localPath))
				{
					File.Delete(job.assets[0].localPath);
				}

				File.Move(job.assets[0].tempLocalPath, job.assets[0].localPath);

				Debug.Log("Downloaded " + job.assets[0]);
			}
			catch (IOException e)
			{
				OnJobFailure(job, e);
			}

			if (job.assets[0].HasThumbnail())
			{
				try
				{
					if (File.Exists(job.assets[0].thumbnailLocalPath))
					{
						File.Delete(job.assets[0].thumbnailLocalPath);
					}

					File.Move(job.assets[0].tempThumbnailLocalPath, job.assets[0].thumbnailLocalPath);

					Debug.Log("Downloaded " + job.assets[0] + " Thumbnail");
				}
				catch (IOException e)
				{
					OnJobFailure(job, e);
				}
			}

			job.assets.RemoveAt(0);
			job.filesDownloaded++;
			job.retries = 0;
			
			if (job.assets.Count > 0 && !job.isCanceled)
			{
				StartCoroutine(StartNextDownload(job));
			}
			else if (!job.isCanceled)
			{
				OnJobComplete(job);
			}
			else
			{
				// TODO: cancel event invocation?
			}
		}

		private void OnDownloadError(LessonDownloadJob job)
		{
			if (job.retries < MAX_RETRIES)
			{
				job.retries++;
				Debug.LogError("Retrying " + job.assets[0]);
				StartCoroutine(StartNextDownload(job));
			}
			else
			{
				OnJobFailure(job);
			}
		}
		
		private void OnJobComplete(LessonDownloadJob job)
		{
			job.isComplete = true;
			job.OnComplete?.Invoke();
			queue.Remove(job);
		}

		private void OnJobFailure(LessonDownloadJob job, [CanBeNull] Exception e = null)
		{
			job.isComplete = false;
			job.OnFailure?.Invoke();
			queue.Remove(job);
		}

		public static bool Subscribe(string lessonGuid, DownloadHandlerListener listener)
		{
			return Instance.SubToOnComplete(lessonGuid, listener.OnDownloadComplete)
				&& Instance.SubToOnProgress(lessonGuid, listener.OnDownloadProgress)
				&& Instance.SubToOnFailure(lessonGuid, listener.OnDownloadFailure);
		}
		
		public static void Unsubscribe(string lessonGuid, DownloadHandlerListener listener)
		{
			Instance.SubToOnComplete(lessonGuid, listener.OnDownloadComplete);
            Instance.SubToOnProgress(lessonGuid, listener.OnDownloadProgress);
            Instance.SubToOnFailure(lessonGuid, listener.OnDownloadFailure);
		}
		
		public bool SubToOnFailure(string lessonGuid, Action action)
		{
			LessonDownloadJob job = GetJob(lessonGuid);
			if (job != null)
			{
				job.OnFailure += action;
				return true;
			}
			return false;
		}

		public void UnSubFromOnFailure(string lessonGuid, Action action)
		{
			LessonDownloadJob job = GetJob(lessonGuid);

			if (job != null)
			{
				job.OnFailure -= action;
			}
		}


		public bool SubToOnComplete(string lessonGuid, Action action)
		{
			LessonDownloadJob job = GetJob(lessonGuid);
			if (job != null)
			{
				job.OnComplete += action;
				return true;
			}
			return false;
		}

		public void UnSubFromOnComplete(string lessonGuid, Action action)
		{
			LessonDownloadJob job = GetJob(lessonGuid);
			if (job != null)
			{
				job.OnComplete -= action;
			}
		}
		
		public bool SubToOnProgress(string lessonGuid, EventHandler<DownloadProgressEventArgs> eventHandler)
		{
			LessonDownloadJob job = GetJob(lessonGuid);
			if (job != null)
			{
				job.OnProgress += eventHandler;
				return true;
			}
			return false;
		}

		public void UnSubFromOnProgress(string lessonGuid, EventHandler<DownloadProgressEventArgs> eventHandler)
		{
			LessonDownloadJob job = GetJob(lessonGuid);
			if (job != null)
			{
				job.OnProgress -= eventHandler;
			}
		}

		private LessonDownloadJob GetJob(string lessonGuid)
		{
			return queue.Find(item => item.guid == lessonGuid);
		}

		private List<LessonDownloadJob> GetJobs(string lessonGuid)
		{
			return queue.FindAll(item => item.guid == lessonGuid);
		}
	}
}


