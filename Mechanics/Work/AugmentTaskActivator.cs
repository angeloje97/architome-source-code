using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Architome
{
    public class AugmentTaskActivator : MonoBehaviour
    {
        public int amountActivated;

        public List<int> validTransmissions;

        [SerializeField] List<ActivatorEvent> events;

        HashSet<AugmentTask> activeAugments;
        public class EventData
        {
            public AugmentTaskActivator taskActivator;
            public EntityInfo sourceEntity;

            public EventData(AugmentTaskActivator activator)
            {
                this.taskActivator = activator;
            }
        }

        public ArchEventHandler<eTaskActivatorEvent, EventData> eventHandlers;


        public EffectsHandler<eTaskActivatorEvent> effectHandler;

        public enum eTaskActivatorEvent
        {
            OnFirstActivated,
            OnReset,
            OnIncreaseAmountActivated,
        }


        private void Start()
        {
            events ??= new();
            GetDependencies();
        }

        void GetDependencies()
        {
            effectHandler.InitiateItemEffects((item) => {
                eventHandlers.AddListener(item.trigger, item.ActivateEffect, this);
            });
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
