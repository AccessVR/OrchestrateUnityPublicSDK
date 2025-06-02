using System.Linq;

namespace AccessVR.OrchestrateVR.SDK
{
    public class AssetPath
    {
        private string[] _parts;

        public string Env => _parts[0];

        public string Guid => _parts[1];

        public string Name => _parts[2];

        private AssetPath(string path)
        {
            _parts = path.Split('/');
        }

        public static AssetPath Make(string path)
        {
            return new AssetPath(path);
        }

        public override string ToString()
        {
            return string.Join('/', _parts);
        }
    }
}