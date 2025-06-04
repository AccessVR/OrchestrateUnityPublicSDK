using System;
using UnityEngine;
using System.Collections.Generic;

namespace AccessVR.OrchestrateVR.SDK
{
    public class EventViewFactory
    {
        private static Dictionary<Type, RegisteredType> _registry = new Dictionary<Type, RegisteredType>();

        public class RegisteredType
        {
            public GameObject Prefab { get; }
            public Func<EventData, GameObject> FactoryFunc  { get; }

            public RegisteredType(GameObject prefab)
            {
                Prefab = prefab;
            }

            public RegisteredType(Func<EventData, GameObject> factoryFunc)
            {
                FactoryFunc = factoryFunc;
            }

            public GameObject GetPrefabForEvent(EventData data)
            {
                if (Prefab != null)
                {
                    return Prefab;
                }
                if (FactoryFunc != null)
                {
                    return FactoryFunc.Invoke(data);
                }
                throw new InvalidOperationException("No prefab or factory function registered");
            }
        }
        
        public static void RegisterType<T>(GameObject eventViewPrefab) where T : EventData
        {
            _registry[typeof(T)] = new RegisteredType(eventViewPrefab);
        }

        public static void RegisterType<T>(Func<EventData, GameObject> factoryFunc) where T : EventData
        {
            _registry[typeof(T)] = new RegisteredType(factoryFunc);
        }

        public static IEventView Make(EventData eventData)
        {
            if (_registry.TryGetValue(eventData.GetType(), out RegisteredType registeredType))
            {
                GameObject prefab = registeredType.GetPrefabForEvent(eventData);
                GameObject gameObject = GameObject.Instantiate(prefab);
                IEventView eventView = gameObject.GetComponent<IEventView>();
                eventView.LoadData(eventData);
                return eventView;
            }
            throw new Exception($"No IEventView type registered for event data type: {eventData.GetType().Name}");
        }
    }
}