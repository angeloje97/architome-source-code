using Architome.Effects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Architome.Events;

namespace Architome
{
    public class AugmentTaskActivator : MonoBehaviour
    {


        public WorkInfo workSourceInfo;
        public List<int> validTransmissions;
        public int amountActivated;

        [SerializeField] List<ActivatorEvent> events;

        HashSet<AugmentTask> activeAugments;
        public class EventData
        {
            public AugmentTaskActivator taskActivator;
            public EntityInfo sourceEntity;
            public AugmentTask sourceAugment;
            public TaskInfo sourceTask;

            public EventData(AugmentTaskActivator activator)
            {
                this.taskActivator = activator;
            }
        }

        public class ActivatorEffectItem : EventItemHandler<eTaskActivatorEvent>
        {
            public bool playForDuration;
        }

        public ArchEventHandler<eTaskActivatorEvent, EventData> eventHandlers;



        public EffectsHandler<eTaskActivatorEvent, ActivatorEffectItem> effectHandler;

        public enum eTaskActivatorEvent
        {
            OnFirstActivated,
            OnReset,
            OnIncreaseAmountActivated,
            OnStartTask,
        }


        private void Start()
        {
            events ??= new();
            GetDependencies();
        }

        private void OnValidate()
        {
            effectHandler?.Validate();
        }

        void GetDependencies()
        {
            HandleListeners();
        }

        void HandleListeners()
        {
            effectHandler.InitiateItemEffects((item) =>
            {

                if (item.playForDuration && item.trigger == eTaskActivatorEvent.OnStartTask)
                {
                    eventHandlers.AddListener(item.trigger, (EventData data) => {
                        if (data.sourceTask == null)
                        {
                            return;
                        }

                        item.SetCanContinuePredicate(() => data.sourceTask.states.isBeingWorkedOn);
                        item.ActivateEffect();

                    }, this);
                    
                }
                else
                {
                    eventHandlers.AddListener(item.trigger, item.ActivateEffect, this);
                }

            });

            if (workSourceInfo)
            {
                workSourceInfo.taskEvents.OnStartTask += (TaskEventData data) => {
                    eventHandlers.Invoke(eTaskActivatorEvent.OnStartTask, new(this) { sourceTask = data.task });
                
                };
            }
        }



        public void IncrementActivated(AugmentTask augment)
        {
            amountActivated++;

            foreach(var activatorEvent in events)
            {
                activatorEvent.HandleIncrement(amountActivated);
            }
            var eventData = new EventData(this)
            {
                sourceEntity = augment.augment.entity,
                sourceAugment = augment,
            };

            if (amountActivated == 1)
            {
                eventHandlers.Invoke(eTaskActivatorEvent.OnFirstActivated, eventData);
            }

            eventHandlers.Invoke(eTaskActivatorEvent.OnIncreaseAmountActivated, eventData);
        }

        public bool ValidAugment(AugmentTask augment)
        {
            if (validTransmissions == null) return false;
            HandleNewAugment(augment);
            return validTransmissions.Contains(augment.validReceiver);
        }

        void HandleNewAugment(AugmentTask augment)
        {
            if (activeAugments.Contains(augment)) return;
            activeAugments.Add(augment);
        }

        public void HandleRemoveAugment(AugmentTask augment)
        {
            if (!activeAugments.Contains(augment)) return;
            activeAugments.Remove(augment);

            if (activeAugments.Count == 0) ResetAmountActivated();
        }

        public void ResetAmountActivated()
        {
            amountActivated = 0;
            eventHandlers.Invoke(eTaskActivatorEvent.OnReset, new EventData(this));
        }

        [Serializable]
        public class ActivatorEvent
        {
            public UnityEvent OnReachActivated;
            [SerializeField] int targetInt = 1;

            public void HandleIncrement(int newIncrement)
            {
                if (newIncrement != targetInt) return;
                OnReachActivated?.Invoke();
            } 
        }
    }
}
