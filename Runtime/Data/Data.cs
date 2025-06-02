using JetBrains.Annotations;
using Newtonsoft.Json;

namespace AccessVR.OrchestrateVR.SDK
{
    public abstract class Data
    {
        [JsonIgnore] protected SceneData _parentScene;

        public virtual void SetParentScene(SceneData scene)
        {
            _parentScene = scene;
        }

        [CanBeNull]
        protected SceneData GetParentScene()
        {
            return _parentScene;
        }
    }
}