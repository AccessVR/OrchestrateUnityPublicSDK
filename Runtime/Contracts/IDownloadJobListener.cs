namespace AccessVR.OrchestrateVR.SDK
{
    public interface IDownloadJobListener
    {
        public void OnDownloadJobProgress(object job, DownloadJobProgressEventArgs eventArgs);

        public void OnDownloadJobFailure(DownloadJob job, Error error);

        public void OnDownloadJobComplete(DownloadJob job);
        
        public void OnDownloadJobCancelled(DownloadJob job);
    }

}