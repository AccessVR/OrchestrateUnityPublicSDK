using JetBrains.Annotations;

namespace AccessVR.OrchestrateVR.SDK
{
    public class DownloadableFileData : FileData
    {
        public string Url => _url;
        
        public DownloadableFileData([NotNull] string url, [NotNull] string guid, [NotNull] string name) : base(guid, name)
        {
            _url = url;
        }
        
        public DownloadableFileData([NotNull] string url, [NotNull] string guid, [NotNull] string name, [NotNull] FileData parent) : base(guid, name, parent)
        {
            _url = url;
        }
    }
}