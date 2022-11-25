using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome.Tutorial
{
    public class EventListenerSequence : MonoBehaviour
    {
        public Action<EventListener> OnSuccesfulEvent, OnNewEvent;
        public List<EventListener> eventListeners;
       
        public Transform listenerTarget;

        public float delay;
        public float delayBetweenDirections;

        public bool useLast;
        public string lastTitle;
        [Multiline]
        public string lastDescription;


        private void OnValidate()
        {
            if (listenerTarget == null) return;

            foreach (var listener in listenerTarget.GetComponents<EventListener>())
            {
                listener.listenOnStart = false;
            }
        }

        void Start()
        {
            ArchAction.Delay(() => GetListenerTargets(), delay);
        }
        void GetListenerTargets()
        {
            if (listenerTarget == null) return;

            eventListeners = listenerTarget.GetComponents<EventListener>().ToList();

            if (eventListeners.Count == 0) return;

            var notificationManager = NotificationManager.active;

            HandleNotification(eventListeners[0], notificationManager);

            for (int i = 0; i < eventListeners.Count - 1; i++)
            {

                var nextListener = eventListeners[i + 1];
                var current = eventListeners[i];
                
                eventListeners[i].OnSuccessfulEvent += (EventListener listener) =>
                {
                    ArchAction.Delay(() => {
                        HandleNotification(nextListener, notificationManager);
                    }, delayBetweenDirections + current.extraSuccessfulTime);
                    
                };

                if(i + 1 == eventListeners.Count - 1)
                {
                    nextListener.OnSuccessfulEvent += (eventListener) => HandleLastDirection(notificationManager);
                }
            }
        }

        public async void HandleLastDirection(NotificationManager manager)
        {
            if (!useLast) return;

            await manager.CreateNotification(new(NotificationType.Success)
            {
                name = lastTitle,
                description = lastDescription
            });
        }

        public async void HandleNotification(EventListener listener, NotificationManager manager)
        {
            listener.StartEventListener();
            var direction = await manager.CreateDirectionNotification(new(NotificationType.Primary) {
                name = listener.title,
                description = listener.NotificationDescription(),
                dismissable = false,
            });


            listener.OnSuccessfulEvent +=  (EventListener listener) => {
                direction.CompleteDirection();
                direction.Bump();
                SystemAudio.PlayNotification(NotificationType.Info);

            };
        }

        public void HandleSuccesfulEvent(EventListener listener)
        {
            OnSuccesfulEvent?.Invoke(listener);
            var index = eventListeners.IndexOf(listener);

            if (eventListeners.Count <= index + 1) return;

            eventListeners[index + 1].StartEventListener();

            OnNewEvent?.Invoke(eventListeners[index + 1]);

        }
    }
}
