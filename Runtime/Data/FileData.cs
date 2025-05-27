using System.IO;
using UnityEngine;

namespace AccessVR.OrchestrateVR.SDK
{
    public class FileData
    {
        protected string _url;
        
        protected string _guid;
        
        protected string _name;
        
        protected FileData _parent;
        
        public string Guid => _guid;
        
        public string Name => _name;
        
        public FileData Parent => _parent;

        public int Retries = 0;

        public string CachePath => Orchestrate.GetCachePath(this);
            
        public string TempCachePath => CachePath + ".tmp";

        public bool IsCached => Orchestrate.IsCached(this);

        public FileData(string guid, string name)
        {
            _guid = guid;
            _name = name;
        }
        
        public FileData(string guid, string name, FileData parent)
        {
            _guid = guid;
            _name = name;
            _parent = parent;
        }
        
        public void SetParent(FileData parent)
        {
            _parent = parent;
        }
    }
}