
namespace AccessVR.OrchestrateVR.SDK
{
    public class DownloadableFileData : FileData
    {
        public string Url => _url;
        
        public DownloadableFileData(string url, string guid, string name) : base(guid, name)
        {
            _url = url;
        }
        
        public DownloadableFileData(string url, string guid, string name, FileData parent) : base(guid, name, parent)
        {
            _url = url;
        }
    }
}