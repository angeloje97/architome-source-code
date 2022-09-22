using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Runtime.CompilerServices;

namespace Architome.Tutorial
{
    public class EventListener : MonoBehaviour
    {
        public bool completed;
        public string title;
        [Multiline] public string description;
        [Multiline] public string tip;
        public event Action<EventListener> OnSuccessfulEvent;
        public event Action<EventListener> OnFailEvent;
        public event Action<EventListener> OnStartEvent;
        public UnityEvent OnStartEventUnity;
        public UnityEvent OnSuccessfulEventUnity;
        public UnityEvent OnFailEventUnity;

        public bool listenOnStart;
        [SerializeField] protected bool initiated;


        protected KeyBindings keyBindData;
        protected ActionBarsInfo actionBarsManager;
        protected WorldActions actions;
        public void GetDependencies()
        {
            keyBindData = KeyBindings.active;
            actionBarsManager = ActionBarsInfo.active;
            actions = WorldActions.active;
        }


        protected void HandleStart()
        {
            if (initiated) return;
            if (listenOnStart)
            {
                StartEventListener();
            }
        }

        public void PreventEntityDeath(EntityInfo entity)
        {
            entity.OnLifeChange += OnEntityLifeChange;

            OnSuccessfulEvent += (EventListener listener) => {
                entity.OnLifeChange -= OnEntityLifeChange;
            };

            void OnEntityLifeChange(bool isAlive)
            {
                ArchAction.Delay(() => {
                    if (completed) return;
                    actions.Revive(entity, entity.transform.position);
                
                }, 2f);
            }
        }



        protected void CompleteEventListener()
        {
            if (completed) return;
            completed = true;
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

        public virtual string Tips()
        {
            return tip;
        }


        public string NotificationDescription()
        {
            var stringList = new List<string>() {
                description,
                Directions(),
                Tips()
            };

            return ArchString.NextLineList(stringList, 1);
        }



        public virtual void StartEventListener()
        {
            initiated = true;
            OnStartEvent?.Invoke(this);
            OnStartEventUnity?.Invoke();
        }
    }
}
