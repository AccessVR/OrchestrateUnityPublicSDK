using JetBrains.Annotations;

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

        public FileData([NotNull] string guid, [NotNull] string name)
        {
            _guid = guid;
            _name = name;
        }
        
        public FileData([NotNull] string guid, [NotNull] string name, [NotNull] FileData parent) : this(guid, name)
        {
            _parent = parent;
        }
        
        public void SetParent(FileData parent)
        {
            _parent = parent;
        }
    }
}