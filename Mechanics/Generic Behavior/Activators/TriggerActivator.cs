using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class TriggerActivator : Activator
{
    public void OnTriggerEnter(Collider other)
    {

        ActivatorData data = new ActivatorData()
        {
            collider = other,
            gameObject = other.gameObject,

        };

        OnActivate?.Invoke(data);
        OnActivateUnity?.Invoke(data);
    }

}
