using Architome.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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


        ArchEventHandler<eMonoEvent, MonoActor> _monoEvents;
        ArchEventHandler<eMonoEvent, MonoActor> monoEvents
        {
            get
            {
                if(_monoEvents == null)
                {
                    Defect.CreateIndicator(transform, "Null Mono Event", new($"{this} class did not initialize Mono events"));
                    _monoEvents = new(this);
                }

                return _monoEvents;
            }
        }



        #endregion

        #region Initiation

        bool initiated = false;

        protected virtual void Awake()
        {
            _monoEvents = new(this);
            initiated = true;
        }

        async Task UnitilInitiated() => await ArchAction.WaitUntil(() => initiated, true);

        #endregion

        #region Events

        private void OnDestroy()
        {
            if (this == null) return;
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
            try
            {
                monoEvents.Invoke(trigger, this);

            }catch(Exception ex)
            {
                Defect.CreateIndicator(transform, $"{name}: ex.Message", ex);
                throw ex;
            }
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
