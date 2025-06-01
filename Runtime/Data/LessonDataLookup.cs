using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
    public class LessonDataLookup
    {
        [JsonProperty("id")] private int _id;
        [JsonProperty("guid")] private string _guid;
        [JsonProperty("embedKey")] private string _embedKey;
        private bool _preview;
        
        public int Id => _id;
        public string Guid => _guid;
        public bool Preview => _preview;
        public string EmbedKey => _embedKey;
        
        public LessonDataLookup(string guid)
        {
            _guid = guid;
        }

        public LessonDataLookup(int id)
        {
            _id = id;
        }

        public LessonDataLookup(int id, bool preview)
        {
            _id = id;
            _preview = preview;
        }

        public LessonDataLookup(int id, string embedKey)
        {
            _id = id;
            _embedKey = embedKey;
        }
        
        [JsonConstructor]
        public LessonDataLookup(int id, string guid, bool preview = false, string embedKey = null)
        {
            _id = id;
            _guid = guid;
            _preview = preview;
            _embedKey = embedKey;
        }
        
    }
}