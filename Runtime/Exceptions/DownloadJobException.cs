using System;

namespace AccessVR.OrchestrateVR.SDK
{
    public class DownloadJobException: Exception
    {
        public DownloadJob DownloadJob { get; }

        public Error Error { get; }

        public DownloadJobException(DownloadJob job, Error error): base(error.Message, error.Cause)
        {
            DownloadJob = job;
            Error = error;
        }
    }
}