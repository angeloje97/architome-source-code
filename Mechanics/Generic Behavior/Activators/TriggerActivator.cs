using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Architome
{

    public class TriggerActivator : Activator
    {
        public Action<Collider, bool> OnTrigger;

        public void OnTriggerEnter(Collider other)
        {

            ActivatorData data = new ActivatorData()
            {
                collider = other,
                gameObject = other.gameObject,

            };

            OnTrigger?.Invoke(other, true);
            OnActivate?.Invoke(data);
            OnActivateUnity?.Invoke(data);
        }

        public void OnTriggerExit(Collider other)
        {

            OnTrigger?.Invoke(other, false);
        }

    }
}
