using System;
using JetBrains.Annotations;

namespace AccessVR.OrchestrateVR.SDK
{
    public class FileData
    {
        protected string _url;
        
        protected string _guid;
        
        protected string _env;
        
        protected Type _type;
        
        protected string _name;
        
        protected FileData _parent;
        
        public string Guid => _guid;
        
        public string Env => _env;
        
        public string Name => _name;
        
        public Type Type => _type;
        
        public FileData Parent => _parent;

        public int Retries = 0;

        public FileData([NotNull] string env, [NotNull] Type type, [NotNull] string guid, [NotNull] string name)
        {
            _env = env;
            _type = type;
            _guid = guid;
            _name = name;
        }
        
        public FileData([NotNull] Environment env, [NotNull] Type type, [NotNull] string guid, [NotNull] string name) : this(env.ToString(), type, guid, name)
        {
            //
        }
        
        public FileData([NotNull] string env, [NotNull] Type type, [NotNull] string guid, [NotNull] string name, [NotNull] FileData parent) : this(env, type, guid, name)
        {
            _parent = parent;
        }
        
        public FileData([NotNull] Environment env, [NotNull] Type type, [NotNull] string guid, [NotNull] string name, [NotNull] FileData parent) : this(env.ToString(), type, guid, name, parent)
        {
            //
        }

        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            // Reference equality check
            if (ReferenceEquals(this, obj)) return true;
            // Null and type check
            if (obj == null || obj.GetType() != this.GetType()) return false;

            return Orchestrate.GetCachePath(this) == Orchestrate.GetCachePath((FileData) obj);
        }

        public override string ToString()
        {
            return string.Join('/', new string[]
            {
                _env,
                _guid,
                _name
            });
        }
    }
}