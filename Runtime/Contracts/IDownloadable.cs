using System.Collections.Generic;

namespace AccessVR.OrchestrateVR.SDK
{
    public interface IDownloadable
    {
        public List<DownloadableFileData> GetDownloadableFiles();
    }
}