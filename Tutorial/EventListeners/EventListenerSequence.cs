using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.Tutorial
{
    public class EventListenerSequence : MonoBehaviour
    {
        public Action<EventListener> OnSuccesfulEvent, OnNewEvent;
        public List<EventListener> eventListeners;
        public Transform listenerTarget;

        private void OnValidate()
        {
            if (listenerTarget == null) return;

            foreach (var listener in listenerTarget.GetComponents<EventListener>())
            {
                listener.listenOnStart = false;
            }
        }

        void Start()
        {
            GetListenerTargets();
        }
        void GetListenerTargets()
        {
            if (listenerTarget == null) return;

            eventListeners = new();

            foreach (var listener in listenerTarget.GetComponents<EventListener>())
            {
                eventListeners.Add(listener);
                listener.OnSuccessfulEvent += HandleSuccesfulEvent;
            }
        }

        public void HandleSuccesfulEvent(EventListener listener)
        {
            OnSuccesfulEvent?.Invoke(listener);
            var index = eventListeners.IndexOf(listener);

            if (eventListeners.Count <= index + 1) return;

            eventListeners[index + 1].StartEventListener();

            OnNewEvent?.Invoke(eventListeners[index + 1]);
        }
    }
}
