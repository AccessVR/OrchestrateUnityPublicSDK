using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace AccessVR.OrchestrateVR.SDK
{
	
	public class Downloader : GenericSingleton<Downloader>
	{
		private const int MaxRetries = 3;
		
		private List<DownloadJob> _queue = new();

		private UnityWebRequest _request;

		public static DownloadJob Download(IDownloadable downloadable)
		{
			DownloadJob job = new DownloadJob(downloadable);
			Download(job);
			return job;
		}
		
		public static void Download(DownloadJob job)
		{
			Instance._queue.Add(job);
			Instance.StartCoroutine(Instance.StartNextDownload(job));
		}

		private void Update()
		{
			foreach (DownloadJob job in _queue)
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
						Orchestrate.FinalizeTempCache(file);
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
						_queue.Remove(job);
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
				_queue.Remove(job);
			}
		}

		public void CancelJob(string guid)
		{
			GetJobs(guid).ForEach(job =>
			{
				job.IsCanceled = true;
				job.OnCancelled?.Invoke(job);
				_queue.Remove(job);
			});
			
			// TODO: RemoveCorruptedDownload(downloadInfo);
		}

		private void OnDownloadError(DownloadJob job, FileData file, Error error)
		{
			if (file.Retries < MaxRetries)
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
			_queue.Remove(job);
		}

		public static bool addListener(string guid, IDownloadJobListener listener)
		{
			DownloadJob job = Instance.GetJob(guid);
			if (job != null)
			{
				job.OnFailure += listener.OnDownloadJobFailure;
				job.OnComplete += listener.OnDownloadJobComplete;
				job.OnProgress += listener.OnDownloadJobProgress;
				job.OnCancelled += listener.OnDownloadJobCancelled;
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
				job.OnCancelled -= listener.OnDownloadJobCancelled;
			}
		}
		
		private DownloadJob GetJob(string guid)
		{
			return _queue.Find(job => job.Guid == guid);
		}

		private List<DownloadJob> GetJobs(string guid)
		{
			return _queue.FindAll(job => job.Guid == guid);
		}
	}
}