namespace AccessVR.OrchestrateVR.SDK
{
    public interface IOfflineStateListener
    {
        public void OnOfflineStateChanged(bool newValue, bool oldValue);
    }
}