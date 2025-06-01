using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AccessVR.OrchestrateVR.SDK
{
    public enum ScreenType
    {
        Sphere360,
        Sphere180,
        Big,
        Pano
    }
    
    public class SceneData : Data, IDownloadable
    {
        [JsonProperty("id")] public int Id;
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
        [JsonProperty("course")] public string Course;
        [JsonProperty("skyboxAsset")] public AssetData Skybox;
        [JsonProperty("localAssetPath")] public string LocalAssetPath;
        
        [JsonProperty("currentTime")] public float CurrentTime = 0.0f;
        [JsonProperty("startTime")] public float StartTime = 0.0f;
        [JsonProperty("endTime")] public float EndTime = 0.0f;
        [JsonProperty("duration")] public float Duration = 0.0f;
        
        [JsonProperty("showInSceneList")] public bool ShowInSceneList = true;
        [JsonProperty("thumbnailAsset")] public AssetData Thumbnail;
        [JsonProperty("endAction")] public ActionData EndAction;
        [JsonProperty("thumbnailURL")] public string thumbnailURL;
        [JsonProperty("timedEvents")] private List<JObject> _timedEvents;
        [JsonProperty("initialView")] public InitialViewData InitialView;
        [JsonProperty("screenType")] private int _screenType;

        [JsonIgnore] public List<EventData> TimedEvents;
        [JsonIgnore] private LessonData _parentLesson;

        [JsonIgnore] public List<EventData> SortedTimedEvents
        {
            get
            {
                List<EventData> sortedTimedEvents = new List<EventData>(TimedEvents ?? new());
                sortedTimedEvents.Sort((e1, e2) => e1.StartTime < e2.StartTime ? -1 : 1);
                return sortedTimedEvents;
            }
        }
        
        [JsonIgnore] public ScreenType screenType;
        [JsonIgnore] public Scene scene;

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            screenType = _screenType switch
            {
                1 => ScreenType.Sphere180,
                2 => ScreenType.Big,
                3 => ScreenType.Pano,
                _ => ScreenType.Sphere360
            };
            
            Skybox?.SetParentScene(this);

            TimedEvents = _timedEvents.Select(timedEvent => EventDataFactory.Make(timedEvent, this)).ToList();
            
            EndAction?.SetParentScene(this);
        }

        public void SetParentLesson(LessonData lesson)
        {
            _parentLesson = lesson;
        }

        public LessonData GetParentLesson()
        {
            return _parentLesson;
        }

        public List<DownloadableFileData> GetDownloadableFiles()
        {
            List<DownloadableFileData> list = new List<DownloadableFileData>
            {
                Skybox?.FileData,
                Thumbnail?.FileData,
            };
            
            list.AddRange(TimedEvents
                .Select(timedEvent => timedEvent.GetDownloadableFiles())
                .SelectMany(fileList => fileList)
                .ToList());

            return list;
        }

        public bool HasThumbnail()
        {
            return Thumbnail != null;
        }
    }
}
