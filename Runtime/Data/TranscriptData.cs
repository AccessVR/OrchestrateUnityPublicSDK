using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace AccessVR.OrchestrateVR.SDK
{
    public enum TranscriptFormat
    {
        VTT,
        SRT,
        JSON
    }
    
    public class TranscriptData
    {
        [JsonProperty("id")] public int Id;
        [JsonProperty("format")] private string _format;
        [JsonProperty("lang")] public string Lang;
        [JsonProperty("content")] public string Content;
        [JsonIgnore] public TranscriptFormat Format { get; private set; }
        
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            Enum.TryParse(_format.ToUpper(), out TranscriptFormat parsed);
            Format = parsed;
        }
    }
    
    
}