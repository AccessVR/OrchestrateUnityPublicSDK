using Newtonsoft.Json;
using System;

namespace AccessVR.OrchestrateVR.SDK
{

    public class AssignmentData : Data
    {
        [JsonProperty("createdBy")] public string createdBy;
        [JsonProperty("dueDate")] public DateTime dueDate;
        [JsonProperty("createDate")] public DateTime createdDate;
        [JsonProperty("guid")] public string guid;
        [JsonProperty("isCompleted")] public bool isCompleted = false;
        [JsonProperty("completedDate")] public DateTime completedDate;
    }

}

