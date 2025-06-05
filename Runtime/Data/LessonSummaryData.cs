using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
    public class LessonSummaryData
    {
        [JsonProperty("id")] public int Id { get; set;  }
        [JsonProperty("name")] public string Name { get; set;  }
        [JsonProperty("guid")] public string Guid { get; set;  }
        [JsonProperty("thumbnailEncoded")] public string ThumbnailEncoded { get; set;  }
        
    }
}