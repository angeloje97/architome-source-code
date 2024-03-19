using Architome.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    public enum eMonoEvent
    {
        OnDestroy,
        OnEnable,
        OnDisable,
    }

    #region MonoListener
    public class MonoActor : MonoBehaviour
    {

        #region Common Data

        ArchEventHandler<eMonoEvent, MonoActor> monoEvents;


        #endregion

        #region Initiation

        bool initiated = false;

        protected virtual void Awake()
        {
            monoEvents = new(this);
            initiated = true;
        }

        async Task UnitilInitiated() => await ArchAction.WaitUntil(() => initiated, true);

        #endregion

        #region Events

        private void OnDestroy()
        {
            Invoke(eMonoEvent.OnDestroy, this);
        }

        private async void OnEnable()
        {
            await UnitilInitiated();
            Invoke(eMonoEvent.OnEnable, this);
            
        }

        private async void OnDisable()
        {
            await UnitilInitiated();
            Invoke(eMonoEvent.OnDisable, this);
        }

        #endregion

        #region Event Handler Overrides

        public void Invoke(eMonoEvent trigger, MonoActor listener)
        {
            monoEvents.Invoke(trigger, this);
        }

        public Action AddListener(eMonoEvent trigger, Action<MonoActor> action, MonoActor otherActor)
        {
            return monoEvents.AddListener(trigger, action, otherActor);
        }
        public Action AddListener(eMonoEvent trigger, Action action, MonoActor otherActor)
        {
            return monoEvents.AddListener(trigger, action, otherActor, false);
        }

        #endregion

        void Update()
        {
        
        }
    }
    #endregion
}
