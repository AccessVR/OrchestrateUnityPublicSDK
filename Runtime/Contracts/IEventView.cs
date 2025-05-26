namespace AccessVR.OrchestrateVR.SDK
{
    public interface IEventView
    {
        void LoadData(AbstractEventData eventData);
        void UpdateLayout();
        void Show();
        void Hide();
        void Destroy();
        void Pause();
        void Play();
    }
}