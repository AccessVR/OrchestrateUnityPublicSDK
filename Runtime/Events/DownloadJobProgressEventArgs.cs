using System;

namespace AccessVR.OrchestrateVR.SDK
{
    public class DownloadJobProgressEventArgs : EventArgs
    {
        public DownloadJobProgressEventArgs(float progress)
		{
			Progress = progress;
		}

		public float Progress { get; set; }
    }
}