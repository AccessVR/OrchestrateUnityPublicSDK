namespace AccessVR.OrchestrateVR.SDK
{
    public class LessonDataLookup
    {
        private int _id;
        private string _guid;
        private bool _preview;
        private string _embedKey;
        
        public int Id => _id;
        public string Guid => _guid;
        public bool Preview => _preview;
        public string EmbedKey => _embedKey;

        public LessonDataLookup(string guid)
        {
            _guid = guid;
        }
        
        public LessonDataLookup(int id, string guid, bool preview = false, string embedKey = null)
        {
            _id = id;
            _guid = guid;
            _preview = preview;
            _embedKey = embedKey;
        }
        
    }
}