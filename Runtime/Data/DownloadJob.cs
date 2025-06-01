using System;
using System.Collections.Generic;
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
		
		public bool IsCanceled = false;
		
		public UnityWebRequest CurrentRequest;
		
		public Action<DownloadJob, Error> OnFailure;
		
		public Action<DownloadJob> OnComplete;

		public Action<DownloadJob> OnCancelled;
		
		public event EventHandler<DownloadJobProgressEventArgs> OnProgress;

		public void AddFile(DownloadableFileData file) => _files.Add(file);
		
		public void RemoveFile(DownloadableFileData file) => _files.Remove(file);

		public IDownloadable Downloadable { get; }

		public DownloadJob(IDownloadable downloadable)
		{
			Downloadable = downloadable;
			downloadable.GetDownloadableFiles().ForEach(file => AddFile(file));
		}
		
		public void ReportProgress()
		{
			float progress = (CurrentRequest != null ? CurrentRequest.downloadProgress * 100 : 0) + (FilesDownloaded * 100f);
			OnProgress?.Invoke(this, new DownloadJobProgressEventArgs(progress));
		}
	}
    
}