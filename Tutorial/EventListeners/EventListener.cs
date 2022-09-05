using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Architome.Tutorial
{
    public class EventListener : MonoBehaviour
    {
        public bool activated;
        public string title;
        [Multiline] public string description;
        [Multiline] public string tip;
        public event Action<EventListener> OnSuccessfulEvent;
        public event Action<EventListener> OnFailEvent;
        public UnityEvent OnSuccessfulEventUnity;
        public UnityEvent OnFailEventUnity;

        public bool listenOnStart;
        [SerializeField] protected bool initiated;

        protected void HandleStart()
        {
            if (initiated) return;
            if (listenOnStart)
            {
                StartEventListener();
            }
        }

        protected void ActivateEventListener()
        {
            activated = true;
            OnSuccessfulEvent?.Invoke(this);
            OnSuccessfulEventUnity?.Invoke();

            Debugger.Environment(4325, $"Completed event {title}");
        }

        protected void FailEventListeners()
        {
            OnFailEvent?.Invoke(this);
            OnFailEventUnity?.Invoke();
        }

        public virtual string Directions()
        {
            return "";
        }

        public virtual void StartEventListener()
        {
            initiated = true;
        }
    }
}
