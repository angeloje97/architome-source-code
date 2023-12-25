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

        private void Start()
        {
            events ??= new();
        }

        public void IncrementActivated()
        {
            amountActivated++;

            foreach(var activatorEvent in events)
            {
                activatorEvent.HandleIncrement(amountActivated);
            }
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