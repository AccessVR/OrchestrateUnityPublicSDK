using System;
using Newtonsoft.Json;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace AccessVR.OrchestrateVR.SDK
{
    public class AssetData : AbstractData
    {
        [JsonProperty("assetTypeId")] public int assetTypeId;
        [JsonProperty("videoTypeId")] public int videoTypeId;
        [JsonProperty("displayedName")] public string displayedName;
        [JsonProperty("fileName")] public string fileName;
        [JsonProperty("parentScene")] public SceneData parentScene;
        [JsonProperty("portrait")] public bool portrait = false;
        [JsonProperty("width")] public int? width;
        [JsonProperty("height")] public int? height;
        [JsonProperty("path")] public string path;
        [JsonProperty("localPath")] public string localPath;
        [JsonProperty("originalPath")] public string originalPath;
        private Texture2D texture;
        public string tempLocalPath => localPath + ".tmp";
        [JsonProperty("thumbnailPath")] public string thumbnailPath;
        [JsonProperty("thumbnailLocalPath")] public string thumbnailLocalPath;
        private Texture2D thumbnailTexture;
        public string tempThumbnailLocalPath => thumbnailLocalPath + ".tmp";
        public bool HasThumbnail() => (IsImage() || IsVideo()) && !string.IsNullOrEmpty(thumbnailPath);
        public bool IsImage() => assetTypeId == 5;
        public bool IsVideo() => assetTypeId == 6;
        public bool IsCached() => File.Exists(localPath) && (!HasThumbnail() || File.Exists(thumbnailLocalPath));
        public async UniTask<Texture2D> LoadTexture()
        {
            if (texture == null)
            {
                string uri = "file://" + localPath;
                Debug.Log("Loading Texture from " + uri);
                UnityWebRequest request = UnityWebRequestTexture.GetTexture(uri, true);
                await request.SendWebRequest();
                if (request.isDone)
                {
                    texture = DownloadHandlerTexture.GetContent(request);
                }
            }
            return texture;
        }
        public async UniTask<Texture2D> LoadThumbnailTexture()
        {
            if (thumbnailLocalPath != null)
            {
                if (thumbnailTexture == null)
                {
                    string uri = "file://" + thumbnailLocalPath;
                    Debug.Log("Loading Texture from " + uri);
                    UnityWebRequest request = UnityWebRequestTexture.GetTexture(uri, true);
                    await request.SendWebRequest();
                    if (request.isDone)
                    {
                        thumbnailTexture = DownloadHandlerTexture.GetContent(request);
                    }
                }
            }
            return thumbnailTexture;
        }
    }
}