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
	
	public class ActionData
	{
		public ActionType type;
		public int sceneId;
		public float timestamp = 0.0f; // same as default value, but helps with clarity
		public AbstractEventData eventData;
		
		public ActionData()
		{
			type = ActionType.None;
		}

		public ActionData(JObject json, SceneData parentScene)
		{
			LoadData(json, parentScene);
		}
		
		private void LoadData(JObject json, SceneData parentScene)
		{
			switch (json["type"].Value<int>())
			{
				default:
					type = ActionType.None;
					break;
				case 1:
					type = ActionType.CompleteLesson;
					break;
				case 2:
					type = ActionType.PreviousScene;
					break;
				case 3:
					type = ActionType.GoToScene;
					LoadGoToSceneData(json, parentScene);
					break;
				case 4:
					type = ActionType.ShowCard;
					LoadShowCardData(json, parentScene);
					break;
				case 5:
					type = ActionType.ReplayCurrentScene;
					LoadReplayCurrentSceneData(parentScene);
					break;
				case 6:
					type = ActionType.ResumePlayback;
					break;
			}
		}

		private void LoadShowCardData(JObject json, SceneData parentScene)
		{
			if (json["event"] != null && json["event"].Type != JTokenType.Null)
			{
				switch (json["event"]["eventType"]?.Value<int>())
				{
					default:
						eventData = null;
						break;
					case 1:
						eventData = json["event"].ToObject<MediaEventData>();
						break;
					case 2:
						eventData = json["event"].ToObject<InfoEventData>();
						break;
					case 3:
						eventData = json["event"].ToObject<QuestionEventData>();
						break;
					case 5:
						eventData = json["event"].ToObject<HotspotEventData>();
						break;
					case 6:
						eventData = json["event"].ToObject<MediaEventData>();
						break;
				}

				if (eventData != null) 
				{
					eventData.isActionEvent = true;
				}
			}
		}
		

		private void LoadGoToSceneData(JObject json, SceneData parentScene)
		{
			sceneId = json["sceneId"]?.Value<int>() ?? 0;
			timestamp = json["timeStamp"]?.Value<float>() ?? 0f;
		}

		private void LoadReplayCurrentSceneData(SceneData parentScene)
		{
			sceneId = parentScene.Id;
			timestamp = 0f;
		}
	}
}
