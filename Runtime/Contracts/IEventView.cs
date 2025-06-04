namespace AccessVR.OrchestrateVR.SDK
{
    public interface IEventView
    {
        void LoadData(EventData eventData);
        void UpdateLayout();
        void Show();
        void Hide();
        void Destroy();
        void Pause();
        void Play();
        bool IsDestroyed();
    }
}