using System;
using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
	public class LegacySubmissionData : Data
	{
		[JsonProperty("id")] public int Id;
		[JsonProperty("completedOn")] public DateTime CompletedDate = DateTime.MaxValue;
		[JsonProperty("score")] public float Score = -1f;
		[JsonProperty("passFail")] public bool passFail = false;
		[JsonProperty("lessonId")] public int lessonID;
	}
}
