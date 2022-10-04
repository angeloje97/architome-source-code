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
        [SerializeField] Transform trailTarget;
        [SerializeField] bool enableTrail;

        public event Action<EventListener> OnSuccessfulEvent;
        public event Action<EventListener> OnFailEvent;
        public event Action<EventListener> OnStartEvent;
        public event Action<EventListener> OnEndEvent;

        public float extraSuccessfulTime = 0f;

        [Serializable]
        public struct UnityEvents
        {
            public UnityEvent OnSuccessfulEvent, OnFailEvent, OnStartEvent, OnEndEvent;
        }

        public UnityEvents events;

        public bool listenOnStart;
        [SerializeField] protected bool simple;
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

        public void HandleTrailEmission()
        {
            if (!enableTrail) return;
            if (trailTarget == null) return;

            var trailEmitter = TrailEmitter.activeTrailEmitter;
            if (trailEmitter == null) return;

            OnStartEvent += (EventListener listener) => {
                trailEmitter.SetTrail(trailTarget);
                trailEmitter.SetEmission(true);
            };

            OnEndEvent += (EventListener listener) => {
                trailEmitter.SetEmission(false);
            };
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

        public void PreventEntityDeathBeforeStart(EntityInfo entity)
        {
            entity.OnLifeChange += OnEntityLifeChange;

            OnStartEvent += (EventListener listener) => {
                entity.OnLifeChange -= OnEntityLifeChange;
            };

            void OnEntityLifeChange(bool isAlive)
            {
                ArchAction.Delay(() => {
                    if (initiated) return;
                    actions.Revive(entity, entity.transform.position);

                }, 2f);
            }
        }



        protected void CompleteEventListener()
        {
            if (completed) return;
            completed = true;
            OnSuccessfulEvent?.Invoke(this);
            events.OnSuccessfulEvent?.Invoke();
            OnEndEvent?.Invoke(this);
            events.OnEndEvent?.Invoke();

            Debugger.Environment(4325, $"Completed event {title}");
        } 

        protected void FailEventListeners()
        {
            OnFailEvent?.Invoke(this);
            events.OnFailEvent?.Invoke();

            OnEndEvent?.Invoke(this);
            events.OnEndEvent?.Invoke();
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
            events.OnStartEvent?.Invoke();
        }
    }
}
