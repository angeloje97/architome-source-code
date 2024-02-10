using Architome.Events;
using System;
using System.Collections;
using System.Collections.Generic;
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

        private void Awake()
        {
            monoEvents = new(this);
        }

        void Start()
        {
        
        }

        #endregion

        #region Events

        private void OnDestroy()
        {
            Invoke(eMonoEvent.OnDestroy, this);
        }

        private void OnEnable()
        {
            Invoke(eMonoEvent.OnEnable, this);
        }

        private void OnDisable()
        {
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
            return monoEvents.AddListener(trigger, action, otherActor);
        }

        #endregion

        void Update()
        {
        
        }
    }
    #endregion
}
