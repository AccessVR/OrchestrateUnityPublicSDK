using System.Collections.Generic;
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
	
	public class ActionData: Data, IDownloadable
	{
		[JsonProperty("id")] private string _Id;

		[JsonIgnore]
		public string Id => _Id ??= System.Guid.NewGuid().ToString();

		[JsonProperty("sceneId")] private int? _sceneId;
		[JsonProperty("timeStamp")] public float Timestamp;
		[JsonProperty("event")] public JObject _eventData;
		[JsonProperty("type")] private int _type;

		public int SceneId => _sceneId ?? -1;

		[JsonIgnore] public ActionType Type = ActionType.None;
		[JsonIgnore] public EventData EventData;

		public static ActionData NoAction => new ActionData();

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

		public List<DownloadableFileData> GetDownloadableFiles()
		{
			return EventData?.GetDownloadableFiles() ?? new();
		}

		public override void SetParentScene(SceneData scene)
		{
			base.SetParentScene(scene);
			EventData?.SetParentScene(scene);
		}

	}
	
}
