using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AccessVR.OrchestrateVR.SDK
{
	public class SubmissionData : AbstractData
	{
		[JsonProperty("id")] public int Id;
		[JsonProperty("completedOn")] public DateTime CompletedDate = DateTime.MaxValue;
		[JsonProperty("score")] public float Score = -1f;
		[JsonProperty("passFail")] public bool passFail = false;
		[JsonProperty("lessonId")] public int lessonID;
		[JsonProperty("markedForDelete")] public bool markedForDelete = false;

		private string DateFormatString = "yyyy/MM/dd hh:mm:ss tt";
		private DateTime runningCompletionDate;

		public SubmissionData()
		{
		}

		public SubmissionData(JToken jsonData)
		{
			//JObject json = jsonData.ToObject<JObject>();
			LoadFromJson(jsonData);
		}

		private void LoadFromJson(JToken jsonObject)
		{
			//if (jsonObject["id"] != null)
			//	Id = (int)jsonObject["id"];

			try
			{
				Id = (int)jsonObject["id"];
			}
			catch
			{

			}

			//JObject json = token.ToObject<JObject>();
			//if (jsonObject["completedOn"] != null)
			//{


				//DateTime.TryParseExact(dateString, DateFormatString, null, System.Globalization.DateTimeStyles.AllowWhiteSpaces |
				//   System.Globalization.DateTimeStyles.AdjustToUniversal,
				//   out runningCompletionDate);
				//if (DateTime.Compare(runningCompletionDate, CompletedDate) < 0)
				//	CompletedDate = runningCompletionDate;
			//}

			try
			{
				string dateString = jsonObject["completedOn"].ToString();
				DateTime.TryParse(dateString, out CompletedDate);
			}
			catch
			{

			}




			try
			{
				Score = (int)jsonObject["score"];
			}
			catch
			{
				//no score for you
			}

			//if (jsonObject["score"] != null)
			//{
			//	Score = (int)jsonObject["score"];
			//}try{
			//
			try
			{
				lessonID = (int)jsonObject["lessonId"];
			}
			catch
			{

			}

			try
			{
				passFail = (jsonObject["passFail"].ToString() == "Completed");
			}
			catch
			{

			}

			//if (jsonObject["lessonId"] != null)
			//{
				
			//}

			//if (jsonObject["passFail"] != null)
			//{
				
			//}

			//if (jsonObject["submissions"] != null)
			//{
			//	foreach (JObject token in jsonObject["submissions"])
			//	{
				
			//	}
			//}
		}
	}
}
