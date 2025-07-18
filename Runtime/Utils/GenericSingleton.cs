using UnityEngine;

namespace AccessVR.OrchestrateVR.SDK
{
    public class GenericSingleton<T> : MonoBehaviour where T : Component
    {
        protected static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).Name;
                        _instance = obj.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }

        public virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;

                if (transform.parent == null) 
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else 
            {
                Destroy(gameObject);
            }
        }
    }
}