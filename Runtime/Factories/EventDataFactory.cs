using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace AccessVR.OrchestrateVR.SDK
{
    public class EventDataFactory
    {
	    [CanBeNull]
	    public static EventData Make(string json, bool isActionEvent = false)
	    {
		    return Make(json, null, isActionEvent);
	    }
	    
	    [CanBeNull]
	    public static EventData Make(JObject json, bool isActionEvent = false)
	    {
		    return Make(json, null, isActionEvent);
	    }
	    
        [CanBeNull]
        public static EventData Make(string json, [CanBeNull] SceneData parent = null, bool isActionEvent = false)
        {
            return Make(JObject.Parse(json), parent, isActionEvent);
        }

        [CanBeNull]
        public static EventData Make(JObject json, [CanBeNull] SceneData parent = null, bool isActionEvent = false)
        {
	        int eventType = json["eventType"]?.Value<int>() ?? 0;

	        EventData eventData = eventType switch
	        {
		        1 => json.ToObject<MediaEventData>(),
		        2 => json.ToObject<InfoEventData>(), // TODO: shouldn't this be media?
		        3 => json.ToObject<QuestionEventData>(),
		        4 => json.ToObject<MediaEventData>(), // old video type
		        5 => json.ToObject<HotspotEventData>(),
		        6 => json.ToObject<MediaEventData>(),
		        _ => throw new Exception("Unknown event type: " + eventType)
	        };

	        eventData?.SetIsActionEvent(isActionEvent);
	        eventData?.SetParentScene(parent);

	        // Call AfterDeserialized to ensure custom post-processing is performed
	        eventData?.AfterDeserialized(new StreamingContext());

	        return eventData;
        }
    }
}