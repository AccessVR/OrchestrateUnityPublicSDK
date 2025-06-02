using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace AccessVR.OrchestrateVR.SDK
{
    public class DownloadJob
	{
		public string Guid;
		
		private readonly List<DownloadableFileData> _files = new List<DownloadableFileData>();

		public DownloadableFileData NextFileData => _files.Find(file => !Orchestrate.CacheContains(file));
		
		private int FilesDownloaded => _files.FindAll(Orchestrate.CacheContains).Count;
		public bool IsComplete => _files.Count == FilesDownloaded;
		
		public bool IsCancelled = false;

		public bool HasFailed => FailureReason != null;

		public Error FailureReason;
		
		private Action<DownloadJob, Error> OnFailure;
		
		private Action<DownloadJob> OnComplete;

		private Action<DownloadJob> OnCancelled;
		
		private UniTaskCompletionSource<DownloadJob> _source = new ();

		private CancellationTokenRegistration? _cancellationTokenRegistration;
		
		public UniTask<DownloadJob> Task => _source.Task;
		
		public event EventHandler<DownloadJobProgressEventArgs> OnProgress;

		public void AddFile(DownloadableFileData file) => _files.Add(file);
		
		public void RemoveFile(DownloadableFileData file) => _files.Remove(file);

		public IDownloadable Downloadable { get; }

		public DownloadJob(IDownloadable downloadable, [CanBeNull] CancellationToken? cancellationToken = null)
		{
			Downloadable = downloadable;
			downloadable.GetDownloadableFiles().ForEach(AddFile);
			if (cancellationToken != null)
			{
				cancellationToken?.ThrowIfCancellationRequested();
			    if ((bool) cancellationToken?.CanBeCanceled)
			    {
			        _cancellationTokenRegistration = cancellationToken?.Register(Cancel);
			    }	
			}
		}

		public void AddListener(IDownloadJobListener listener)
		{
			OnFailure += listener.OnDownloadJobFailure;
			OnComplete += listener.OnDownloadJobComplete;
			OnProgress += listener.OnDownloadJobProgress;
			OnCancelled += listener.OnDownloadJobCancelled;
		}

		public void RemoveListener(IDownloadJobListener listener)
		{
			OnFailure -= listener.OnDownloadJobFailure;
			OnComplete -= listener.OnDownloadJobComplete;
			OnProgress -= listener.OnDownloadJobProgress;
			OnCancelled -= listener.OnDownloadJobCancelled;
		}

		public void Cancel()
		{
			IsCancelled = true;
			_source.TrySetResult(this);
		}

		public void FireFailure(Error error)
		{
			IsCancelled = true;
			FailureReason = error;
			OnFailure?.Invoke(this, error);
			_source.TrySetException(new DownloadJobException(this, error));
		}

		public void TryFireComplete()
		{
			if (!IsCancelled)
			{
				OnComplete?.Invoke(this);
				_source.TrySetResult(this);	
			}
		}
		
		public void FireProgress(float requestDownloadProgress)
		{
			float progress = (requestDownloadProgress + FilesDownloaded) / _files.Count;
			OnProgress?.Invoke(this, new DownloadJobProgressEventArgs(progress));
		}
	}
    
}