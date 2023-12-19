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
            return validTransmissions.Contains(augment.validReceiver);
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
