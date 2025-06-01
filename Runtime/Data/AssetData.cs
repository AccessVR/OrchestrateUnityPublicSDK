using System;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;

namespace AccessVR.OrchestrateVR.SDK
{
    public class AssetData : Data, IDownloadable
    {
        [JsonProperty("assetTypeId")] public int assetTypeId;
        [JsonProperty("videoTypeId")] public int? videoTypeId;
        [JsonProperty("displayedName")] public string displayedName;
        [JsonProperty("fileName")] public string fileName;
        [JsonProperty("portrait")] public bool portrait = false;
        [JsonProperty("width")] public int? width;
        [JsonProperty("height")] public int? height;
        [JsonProperty("path")] public string path;
        [JsonProperty("originalPath")] public string originalPath;
        [JsonProperty("thumbnailPath")] public string thumbnailPath;

        [JsonIgnore] private SceneData _parentScene;
        
        private Texture2D _texture;
        
        private Texture2D _thumbnailTexture;
        
        public bool HasThumbnail() => (IsImage() || IsVideo()) && !String.IsNullOrEmpty(thumbnailPath);
        public bool IsAudio() => assetTypeId == 4;
        public bool IsImage() => assetTypeId == 5;
        public bool IsVideo() => assetTypeId == 6;
        
        public bool IsAudioOrVideo() => IsAudio() || IsVideo();

        public DownloadableFileData FileData
        {
            get
            {
                string assetPath = path;
                
                if (IsVideo() && originalPath != null)
                {
                    assetPath = originalPath;
                }

                if (_parentScene?.GetParentLesson() != null)
                {
                    return new DownloadableFileData(
                        Orchestrate.CdnUrl(assetPath), 
                        AssetPath.Make(assetPath).Guid, 
                        path, 
                        _parentScene.GetParentLesson().FileData);    
                }
                else
                {
                    return new DownloadableFileData(
                        Orchestrate.CdnUrl(assetPath), 
                        AssetPath.Make(assetPath).Guid, 
                        path);    
                }
                
            }
        }

        public void SetParentScene(SceneData scene)
        {
            _parentScene = scene;
        }

        public SceneData GetParentScene(SceneData scene)
        {
            return _parentScene;
        }

        public DownloadableFileData ThumbnailFileData
        {
            get
            {
                if (HasThumbnail())
                {
                    if (_parentScene?.GetParentLesson() != null)
                    {
                        return new DownloadableFileData(
                            Orchestrate.CdnUrl(thumbnailPath), 
                            AssetPath.Make(thumbnailPath).Guid, 
                            path, 
                            _parentScene.GetParentLesson().FileData);    
                    }
                    else
                    {
                        return new DownloadableFileData(
                            Orchestrate.CdnUrl(thumbnailPath), 
                            AssetPath.Make(thumbnailPath).Guid, 
                            path);    
                    }
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