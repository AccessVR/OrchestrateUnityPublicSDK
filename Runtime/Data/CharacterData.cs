using System;
using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
    /// <summary>
    /// The Character a <see cref="ConversationEventData"/> layer talks to, as the
    /// published payload hydrates it: identity for display, runtime flags, and
    /// the relative path that starts this layer's conversation. The path is
    /// relative so the frozen payload survives a change of host; prefix it
    /// with <see cref="Orchestrate.GetUrl"/>.
    /// </summary>
    [Serializable]
    public class CharacterData
    {
        [JsonProperty("id")] public int Id;

        [JsonProperty("guid")] public string Guid;

        [JsonProperty("name")] public string Name;

        [JsonProperty("description")] public string Description;

        [JsonProperty("thumbnailUrl")] public string ThumbnailUrl;

        [JsonProperty("thumbnailVideoUrl")] public string ThumbnailVideoUrl;

        /// <summary>
        /// False when the Character was deactivated or lost its configuration
        /// after publish; nothing will launch from the layer then.
        /// </summary>
        [JsonProperty("isActive")] public bool IsActive = true;

        [JsonProperty("launchPath")] public string LaunchPath;
    }
}
