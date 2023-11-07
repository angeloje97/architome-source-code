using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class SafeEvent<T>
    {
        Component source;
        Action<T> action;

        public SafeEvent(Component source)
        {
            this.source = source;
        }


        public Action AddListener(Action<T> actionToInvoke, Component listener)
        {
            var unsubscribed = false;

            void MiddleWare(T data)
            {
                if(listener == null)
                {
                    UnsubScribe();
                    return;
                }


                actionToInvoke(data);
            }

            return UnsubScribe;

            void UnsubScribe()
            {
                if (unsubscribed) return;
                unsubscribed = true;
                action -= MiddleWare;
            }
        }

        public void Invoke(T data)
        {
            if (source == null) return;
            action?.Invoke(data);
        }
    }
}
