using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{

    public abstract class Data
    {
        [JsonIgnore] private SceneData _parentScene;

        public virtual void SetParentScene(SceneData scene)
        {
            _parentScene = scene;
        }

        public SceneData GetParentScene(SceneData scene)
        {
            return _parentScene;
        }
    }
    
}