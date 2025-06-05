using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace AccessVR.OrchestrateVR.SDK
{
	public class Downloader : GenericSingleton<Downloader>
	{
		private const int MaxRetries = 3;
		
		private List<DownloadJob> _queue = new();

		private UnityWebRequest _request;
		
		public static UniTask<DownloadJob> Download(IDownloadable downloadable, IDownloadJobListener listener, CancellationToken? cancellationToken = null)
		{
			DownloadJob job = new DownloadJob(downloadable);
			job.AddListener(listener);
			return Download(job, cancellationToken);
		}
		
		public static UniTask<DownloadJob> Download(IDownloadable downloadable, CancellationToken? cancellationToken = null)
		{
			return Download(new DownloadJob(downloadable), cancellationToken);
		}
		
		private static UniTask<DownloadJob> Download(DownloadJob job, CancellationToken? cancellationToken = null)
		{
		    Instance._queue.Add(job);
		    Instance.StartCoroutine(Instance.StartNextDownload(job));
		    return job.Task;
		}

		private IEnumerator StartNextDownload(DownloadJob job)
		{
			if (Orchestrate.IsOffline)
			{
				job.FireFailure(Error.InternetRequired);
			}

			if (job.IsCancelled) yield break;
			
			DownloadableFileData file = job.NextFileData;
			
			if (file != null)
			{
				bool error = false;
				UnityWebRequest request  = Orchestrate.MakeCacheRequest(file);
				UnityWebRequestAsyncOperation operation = request.SendWebRequest();

				while (!operation.isDone)
				{
					if (job.IsCancelled)
					{
						request.Abort();
						yield break;
					}
					job.FireProgress(request.downloadProgress);
					yield return null;
				}
					
				if (request.result != UnityWebRequest.Result.Success)
				{
					// TODO: get more information about the failure
					OnDownloadError(job, file, new Error($"Failed to download File {file.Url}"));
					error = true;
				}
					
				if (!error)
				{
					try
					{
						Orchestrate.FinalizeTempCache(file);
						Debug.Log($"Downloaded {file}");
						
						if (!job.IsComplete && !job.IsCancelled)
						{
							StartCoroutine(StartNextDownload(job));
						}
						else if (!job.IsCancelled)
						{
							job.TryFireComplete();
							_queue.Remove(job);
						}
					}
					catch (IOException e)
					{
						job.FireFailure(new Error(e));
					}
				}
			}
			else
			{
				job.TryFireComplete();
				_queue.Remove(job);
			}
		}

		public void CancelJob(string guid)
		{
			GetJobs(guid).ForEach(job =>
			{
				job.Cancel();
				_queue.Remove(job);
			});
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
				job.FireFailure(new Error(ErrorType.TooManyRetries));
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