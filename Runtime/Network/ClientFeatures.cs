namespace AccessVR.OrchestrateVR.SDK
{
    /// <summary>
    /// Optional capabilities this client declares to the server on every API
    /// request. The server withholds layer types a client has not declared —
    /// an older build that does not recognize a layer type would otherwise fail
    /// to load the whole scene — so each new layer type this client learns is
    /// added here.
    /// </summary>
    public static class ClientFeatures
    {
        public const string HeaderName = "X-OXR-Features";

        public const string ConversationLayers = "conversation-layers";

        public static readonly string[] Supported = { ConversationLayers };

        public static string HeaderValue => string.Join(",", Supported);
    }
}
