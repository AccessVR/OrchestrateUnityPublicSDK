using System;
using UnityEngine;
using System.Collections.Generic;

namespace AccessVR.OrchestrateVR.SDK
{
    public class EventViewFactory
    {
        private static Dictionary<string, GameObject> registry = new Dictionary<string, GameObject>();

        public static void RegisterType(string eventDataType, GameObject eventViewPrefab)
        {
            Type abstractEventData = typeof(AbstractEventData);
            if (!Type.GetType(eventDataType).IsAssignableFrom(abstractEventData)) 
            {
                throw new Exception($"Event data type {eventDataType} does not inherit from AbstractEventData");
            }

            registry[eventDataType] = eventViewPrefab;
        }

        public static IEventView CreateView(AbstractEventData eventData)
        {
            if (registry.TryGetValue(eventData.GetType().Name, out GameObject eventViewPrefab))
            {
                GameObject gameObject = GameObject.Instantiate(eventViewPrefab);
                IEventView eventView = gameObject.GetComponent<IEventView>();
                eventView.LoadData(eventData);
                return eventView;
            }
            else
            {
                throw new Exception($"No IEventView type registered for event data type: {eventData.GetType().Name}");
            }
        }
    }
}