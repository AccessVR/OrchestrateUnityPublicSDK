using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using UnityEngine;

namespace AccessVR.OrchestrateVR.SDK
{
    public enum ScreenType
    {
        Sphere360,
        Sphere180,
        Big,
        Pano
    }
    
    public class SceneData : AbstractData
    {
        [JsonProperty("id")] public int Id;
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
        [JsonProperty("course")] public string Course;
        [JsonProperty("skyboxAsset")] public AssetData Skybox;
        [JsonProperty("assets")] public List<AssetData> Assets;
        [JsonProperty("localAssetPath")] public string LocalAssetPath;
        [JsonProperty("lessonGuid")] public string LessonGuid;
        [JsonProperty("cachedJson")] public string CachedJson;
        [JsonProperty("initialRotation")] public Vector3 InitialRotation = new Vector3(0,0,90);
        [JsonProperty("initialLatitude")] public float InitialLatitude = 0;
        [JsonProperty("initialLongitude")] public float InitialLongitude = 0;
        [JsonProperty("initialQuaternion")] public Quaternion InitialQuaternion = new Quaternion(0,0,0,0);
        [JsonProperty("initialRot2")] public Vector3 InitialRot2 = new Vector3(0, 0, 90);
        [JsonProperty("currentTime")] public float CurrentTime = 0.0f;
        [JsonProperty("startTime")] public float StartTime = 0.0f;
        [JsonProperty("endTime")] public float EndTime = 0.0f;
        [JsonProperty("duration")] public float Duration = 0.0f;
        [JsonProperty("isLoaded")] public bool IsLoaded = false;
        [JsonProperty("isInitial")] public bool IsInitial = false;
        [JsonProperty("showInSceneList")] public bool ShowInSceneList = true;
        [JsonProperty("thumbnailAsset")] public AssetData Thumbnail;
        [JsonProperty("endAction")] public ActionData endAction;
        [JsonIgnore] public Scene scene;
        [JsonProperty("thumbnailURL")] public string thumbnailURL;
        [JsonProperty("timedEvents")] public List<AbstractEventData> TimedEvents;
        [JsonProperty("screenType")] public ScreenType screenType;

        // TODO: why is this here?
        // [JsonIgnore] public ViewerController controller;
        
        public bool IsCached
        {
            get
            {
                bool cached = true;
                if (Assets != null)
                {
                    foreach(AssetData asset in Assets)
                    {
                        if (!asset.IsCached())
                        {
                            cached = false;
                        }
                    }
                }
                return cached;    
            }
        }

        public void AddAsset(AssetData asset)
        {
            if (Assets == null) Assets = new List<AssetData>();
            Assets.Add(asset);
        }

        public bool HasThumbnail()
        {
            return Thumbnail != null;
        }
    }
}
