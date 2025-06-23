using JetBrains.Annotations;
using System;

namespace AccessVR.OrchestrateVR.SDK
{
    public class DownloadableFileData : FileData
    {
        protected string _url;
        
        public string Url => _url;
        
        public DownloadableFileData([NotNull] string url, [NotNull] string env, [NotNull] Type type, [NotNull] string guid, [NotNull] string name) : base(env, type, guid, name)
        {
            _url = url;
        }
        
        public DownloadableFileData([NotNull] string url, [NotNull] string env, [NotNull] Type type, [NotNull] string guid, [NotNull] string name, [NotNull] FileData parent) : base(env, type, guid, name, parent)
        {
            _url = url;
        }
    }
}