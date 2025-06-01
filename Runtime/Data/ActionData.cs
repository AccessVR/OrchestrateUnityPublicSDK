using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AccessVR.OrchestrateVR.SDK
{
	public enum ActionType
	{
		None,
		CompleteLesson,
		PreviousScene,
		GoToScene,
		ShowCard,
		ReplayCurrentScene,
		ResumePlayback,
	}
	
	public class ActionData: Data
	{
		[JsonProperty("id")] private string Id;
		[JsonProperty("sceneId")] private int? _sceneId;
		[JsonProperty("timeStamp")] public float Timestamp;
		[JsonProperty("event")] public JObject _eventData;
		[JsonProperty("type")] private int _type;
		
		public int SceneId => _sceneId ?? -1;
		
		[JsonIgnore] public ActionType Type = ActionType.None;
		[JsonIgnore] public EventData EventData;

		[OnDeserialized]
		public void OnDeserialized(StreamingContext context)
		{
			Type = _type switch
			{
				1 => ActionType.CompleteLesson,
				2 => ActionType.PreviousScene,
				3 => ActionType.GoToScene,
				4 => ActionType.ShowCard,
				5 => ActionType.ReplayCurrentScene,
				6 => ActionType.ResumePlayback,
				_ => ActionType.None
			};

			if (_eventData != null)
			{
				EventData = EventDataFactory.Make(_eventData, true);
			}
		}

		public override void SetParentScene(SceneData scene)
		{
			base.SetParentScene(scene);
			EventData?.SetParentScene(scene);
		}

	}
	
}
