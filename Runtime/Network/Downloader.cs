using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace AccessVR.OrchestrateVR.SDK
{
	
	public class Downloader : GenericSingleton<Downloader>
	{
		private const int MAX_RETRIES = 3;
		
		private List<DownloadJob> queue = new();

		private UnityWebRequest request;

		public void Download(DownloadJob job)
		{
			queue.Add(job);
			StartCoroutine(StartNextDownload(job));
		}

		private void Update()
		{
			foreach (DownloadJob job in queue)
			{
				if (!job.IsCanceled)
				{
					job.ReportProgress();
				}
			}
		}

		private IEnumerator StartNextDownload(DownloadJob job)
		{
			if (Orchestrate.Offline)
			{
				FireJobFailure(job, Error.InternetRequired);
			}

			if (job.IsCanceled) yield break;
			
			DownloadableFileData file = job.NextFileData;
			
			if (file != null)
			{
				bool error = false;
				
				job.CurrentRequest = Orchestrate.MakeCacheRequest(file);
					
				yield return job.CurrentRequest.SendWebRequest();
					
				if (job.CurrentRequest.result != UnityWebRequest.Result.Success)
				{
					OnDownloadError(job, file, new Error($"Failed to download File {file.Url}"));
					error = true;
				}
					
				if (!error)
				{
					try
					{
						Orchestrate.FinalizeCache(file);
						Debug.Log("Downloaded " + file);
					}
					catch (IOException e)
					{
						FireJobFailure(job, new Error(e));
					}
					
					if (!job.IsComplete && !job.IsCanceled)
					{
						StartCoroutine(StartNextDownload(job));
					}
					else if (!job.IsCanceled)
					{
						job.OnComplete?.Invoke(job);
						queue.Remove(job);
					}
					else
					{
						job.OnCancelled?.Invoke(job);
					}
				}
			}
			else
			{
				job.OnComplete?.Invoke(job);
				queue.Remove(job);
			}
		}

		public void CancelJob(string guid)
		{
			GetJobs(guid).ForEach(job =>
			{
				job.IsCanceled = true;
				job.OnCancelled?.Invoke(job);
				queue.Remove(job);
			});
			
			// TODO: RemoveCorruptedDownload(downloadInfo);
		}

		private void OnDownloadError(DownloadJob job, FileData file, Error error)
		{
			if (file.Retries < MAX_RETRIES)
			{
				file.Retries++;
				Debug.LogError("Retrying " + file);
				StartCoroutine(StartNextDownload(job));
			}
			else
			{
				// TODO: log the file
				
				FireJobFailure(job, new Error(ErrorType.TooManyRetries));
			}
		}
		

		private void FireJobFailure(DownloadJob job, Error error)
		{
			job.OnFailure?.Invoke(job, error);
			queue.Remove(job);
		}

		public static bool addListener(string guid, IDownloadJobListener listener)
		{
			DownloadJob job = Instance.GetJob(guid);
			if (job != null)
			{
				job.OnFailure += listener.OnDownloadJobFailure;
				job.OnComplete += listener.OnDownloadJobComplete;
				job.OnProgress += listener.OnDownloadJobProgress;
				return true;
			}
			return false;
		}
		
		public static void removeListener(string guid, IDownloadJobListener listener)
		{
			DownloadJob job = Instance.GetJob(guid);
			if (job != null)
			{
				job.OnFailure -= listener.OnDownloadJobFailure;
				job.OnComplete -= listener.OnDownloadJobComplete;
				job.OnProgress -= listener.OnDownloadJobProgress;
			}
		}
		
		private DownloadJob GetJob(string guid)
		{
			return queue.Find(job => job.Guid == guid);
		}

		private List<DownloadJob> GetJobs(string guid)
		{
			return queue.FindAll(job => job.Guid == guid);
		}
	}
}


