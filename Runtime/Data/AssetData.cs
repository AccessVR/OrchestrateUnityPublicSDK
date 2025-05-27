using System;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;

namespace AccessVR.OrchestrateVR.SDK
{
    public class AssetData : AbstractData, IDownloadable
    {
        [JsonProperty("assetTypeId")] public int assetTypeId;
        [JsonProperty("videoTypeId")] public int videoTypeId;
        [JsonProperty("displayedName")] public string displayedName;
        [JsonProperty("fileName")] public string fileName;
        [JsonProperty("portrait")] public bool portrait = false;
        [JsonProperty("width")] public int? width;
        [JsonProperty("height")] public int? height;
        [JsonProperty("path")] public string path;
        [JsonProperty("localPath")] public string localPath;
        [JsonProperty("originalPath")] public string originalPath;
        [JsonProperty("thumbnailPath")] public string thumbnailPath;
        [JsonProperty("thumbnailLocalPath")] public string thumbnailLocalPath;

        [JsonIgnore] public SceneData scene;
        
        private Texture2D _texture;
        
        private Texture2D _thumbnailTexture;
        
        public bool HasThumbnail() => (IsImage() || IsVideo()) && !String.IsNullOrEmpty(thumbnailPath);
        public bool IsImage() => assetTypeId == 5;
        public bool IsVideo() => assetTypeId == 6;

        public DownloadableFileData FileData
        {
            get
            {
                string mainPath = path;
                if (IsVideo() && originalPath != null)
                {
                    mainPath = originalPath;
                }
                return new DownloadableFileData(Orchestrate.CdnUrl(mainPath), AssetPath.Make(mainPath).Guid,
                    path, LessonData.Make(scene?.LessonGuid).FileData);
            }
        }

        public DownloadableFileData ThumbnailFileData
        {
            get
            {
                if (HasThumbnail())
                {
                    return new DownloadableFileData(Orchestrate.CdnUrl(thumbnailPath), AssetPath.Make(thumbnailPath).Guid,
                        thumbnailPath, LessonData.Make(scene?.LessonGuid).FileData);
                }
                return null;
            }
        }

        public List<DownloadableFileData> GetDownloadableFiles()
        {
            List<DownloadableFileData> files = new();
            files.Add(FileData);
            if (HasThumbnail())
            {
                files.Add(ThumbnailFileData);
            }
            return files;
        }
            
        public bool IsCached()
        {
            return GetDownloadableFiles().Aggregate(true, (isCached, file) => isCached && file.IsCached);
        }
        
        public async UniTask<Texture2D> LoadTexture()
        {
            if (_texture == null)
            {
                _texture = await Orchestrate.LoadTexture2D(FileData);
            }
            return _texture;
        }
        
        public async UniTask<Texture2D> LoadThumbnailTexture()
        {
            if (HasThumbnail())
            {
                if (_thumbnailTexture == null)
                {
                    _thumbnailTexture = await Orchestrate.LoadTexture2D(ThumbnailFileData);
                }
            }
            return null;
        }
    }
}