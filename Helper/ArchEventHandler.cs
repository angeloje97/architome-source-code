using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class ArchEventHandler<T, E>  where T: Enum
    {
        Component source;

        Dictionary<T, Action<E>> eventDict;

        public ArchEventHandler(Component source)
        {
            this.source = source;
            eventDict = new();
        }

        public Action AddListener(T eventType, Action<E> action, Component listener)
        {
            if (source == null) return () => { };
            if (!eventDict.ContainsKey(eventType))
            {
                eventDict.Add(eventType, null);
            }

            bool unsubscribed = false;
            eventDict[eventType] += MiddleWare;


            return Unsubscribe;

            void MiddleWare(E eventData)
            {
                if(listener == null)
                {
                    eventDict[eventType] -= MiddleWare;
                    unsubscribed = true;
                    return;
                }

                action(eventData);
            }

            void Unsubscribe()
            {
                if (unsubscribed) return;
                unsubscribed = true;
                if (source == null) return;

                eventDict[eventType] -= MiddleWare;
            }
        }

        public void InvokeEvent(T eventType, E eventData)
        {
            if (source == null) return;
            if (!eventDict.ContainsKey(eventType)) return;
            eventDict[eventType]?.Invoke(eventData);

        }

    }
}
