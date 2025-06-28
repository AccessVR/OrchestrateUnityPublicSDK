using System;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

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
        [JsonProperty("subtitles")] public List<TranscriptData> Subtitles;
        
        private Texture2D _texture;
        
        private Texture2D _thumbnailTexture;
        
        public bool HasThumbnail() => (IsImage() || IsVideo()) && !String.IsNullOrEmpty(thumbnailPath);
        public bool IsAudio() => assetTypeId == 4;
        public bool IsImage() => assetTypeId == 5;
        public bool IsVideo() => assetTypeId == 6;
        public bool IsAudioOrVideo() => IsAudio() || IsVideo();
        public bool HasSubtitles() => SrtSubtitles != null && !string.IsNullOrEmpty(SrtSubtitles.Content);
        public TranscriptData SrtSubtitles => Subtitles.FirstOrDefault(transcript => transcript.Format == TranscriptFormat.SRT);
        
        [JsonIgnore]
        public DownloadableFileData SubtitlesFileData
        {
            get
            {
                AssetPath assetPath = AssetPath.Make(path);
                
                DownloadableFileData subtitlesFileData = new (
                    "subtitles.srt", 
                    assetPath.Env,
                    GetType(), 
                    AssetPath.Make(path).Guid, 
                    "subtitles.srt"
                );
                    
                if (GetParentScene()?.GetParentLesson() != null)
                {
                    subtitlesFileData.WithParent(GetParentScene().GetParentLesson().FileData);   
                }

                subtitlesFileData.WithContents(SrtSubtitles.Content);

                return subtitlesFileData;
            }
        }

        [JsonIgnore] public DownloadableFileData FileData
        {
            get
            {
                AssetPath assetPath = AssetPath.Make(path);
                if (IsVideo() && originalPath != null)
                {
                    assetPath = AssetPath.Make(originalPath);
                }

                DownloadableFileData fileData = new DownloadableFileData(
                    Orchestrate.GetCdnUrl(assetPath.ToString()),
                    assetPath.Env,
                    GetType(),
                    assetPath.Guid,
                    assetPath.Name
                );
                
                if (GetParentScene()?.GetParentLesson() != null)
                {
                    fileData.WithParent(GetParentScene().GetParentLesson().FileData);   
                }
                
                return fileData;;    
            }
        }

        [JsonIgnore]
        [CanBeNull]
        public DownloadableFileData ThumbnailFileData
        {
            get
            {
                if (HasThumbnail())
                {
                    AssetPath assetPath = AssetPath.Make(thumbnailPath);
                    if (GetParentScene()?.GetParentLesson() != null)
                    {
                        return new DownloadableFileData(
                            Orchestrate.GetCdnUrl(assetPath.ToString()),
                            assetPath.Env,
                            GetType(),
                            assetPath.Guid, 
                            assetPath.Name, 
                            GetParentScene().GetParentLesson().FileData
                        );    
                    }
                    
                    return new DownloadableFileData(
                        Orchestrate.GetCdnUrl(assetPath.ToString()),
                        assetPath.Env,
                        GetType(),
                        assetPath.Guid,  
                        assetPath.Name
                    );    
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

            if (HasSubtitles())
            {
                files.Add(SubtitlesFileData);
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
            return _thumbnailTexture;
        }
    }
}