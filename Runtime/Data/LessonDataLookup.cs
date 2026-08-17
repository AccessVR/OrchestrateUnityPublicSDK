using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
    public class LessonDataLookup
    {
        [JsonProperty("id")] private int _id;
        [JsonProperty("assignmentId")] private int _assignmentId;
        [JsonProperty("guid")] private string _guid;
        [JsonProperty("embedKey")] private string _uniqueKey;
        // Reversible version/lesson hash carried by a preview launch code. When
        // present, the lesson fetch requests this preview version instead of the
        // published head (forwarded as ?preview=<hash>). Null/empty = published.
        [JsonProperty("preview")] private string _preview;

        public int Id => _id;
        public string Guid => _guid;
        public int AssignmentId => _assignmentId;
        public string Preview => _preview;
        public string UniqueKey => _uniqueKey;
        
        public LessonDataLookup(string guid)
        {
            _guid = guid;
        }

        public LessonDataLookup(int id)
        {
            _id = id;
        }

        public LessonDataLookup(int id, string guid)
        {
            _id = id;
            _guid = guid;
        }

        [JsonConstructor]
        public LessonDataLookup(int id, string guid, int assignmentId = 0, string preview = null, string uniqueKey = null)
        {
            _id = id;
            _assignmentId = assignmentId;
            _guid = guid;
            _preview = preview;
            _uniqueKey = uniqueKey;
        }
        
    }
}