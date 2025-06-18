using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
    public class LessonDataLookup
    {
        [JsonProperty("id")] private int _id;
        [JsonProperty("assignmentId")] private int _assignmentId;
        [JsonProperty("guid")] private string _guid;
        [JsonProperty("embedKey")] private string _uniqueKey;
        [JsonProperty("preview")] private bool _preview;
        
        public int Id => _id;
        public string Guid => _guid;
        public int AssignmentId => _assignmentId;
        public bool Preview => _preview;
        public string UniqueKey => _uniqueKey;
        
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

        public LessonDataLookup(int id, string guid)
        {
            _id = id;
            _guid = guid;
        }
        
        [JsonConstructor]
        public LessonDataLookup(int id, string guid, int assignmentId = 0, bool preview = false, string uniqueKey = null)
        {
            _id = id;
            _assignmentId = assignmentId;
            _guid = guid;
            _preview = preview;
            _uniqueKey = uniqueKey;
        }
        
    }
}