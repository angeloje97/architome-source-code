using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class NotificationFX : MonoBehaviour
    {
        [Serializable]
        public struct FX
        {
            public NotificationEvent trigger;
            public int animationIndex;
            public string animationTrigger;
            public AudioClip clip;
        }

        [SerializeField] Notification notification;
        [SerializeField] AudioManager audioManager;
        [SerializeField] Animator animator;
        [SerializeField] CanvasGroup canvas;
        [SerializeField] SizeFitter sizeFitter;

        public List<FX> effects;

        bool transitionPlaying;

        void Start()
        {
            GetDependencies();
            AddListeners();
            HandleTransitions();
        }
        private void GetDependencies()
        {
            audioManager = GetComponentInParent<AudioManager>();
        }

        void HandleTransitions()
        {
            notification.AddActionEvent(NotificationEvent.OnFirstAppear, () => {
                var initialAudioClip = notification.currentTypeInfo.audioClip;

                if (!initialAudioClip) return;

                audioManager.PlaySound(initialAudioClip);
            });

            notification.AddActionEvent(NotificationEvent.OnAppear, () =>
            {
                _= TransitionIn();
            });


            notification.AddActionEvent(NotificationEvent.BeforeHide, () =>
            {
                notification.tasksBeforeHide.Add(TransitionOut());
            });

            notification.AddActionEvent(NotificationEvent.OnDismiss, () => {
                notification.tasksBeforeDismiss.Add(TransitionOut());
            });
        }


        void AddListeners()
        {
            foreach (var fx in effects)
            {
                notification.AddActionEvent(fx.trigger, () => HandleFX(fx));
            }
        }

        void HandleFX(FX fx)
        {
            HandleAnimation();

            void HandleAnimation()
            {
                if (fx.animationTrigger != null && fx.animationTrigger.Length > 0)
                {
                    animator.SetTrigger(fx.animationTrigger);
                }

                if (fx.animationIndex != 0)
                {
                    animator.SetInteger("Index", fx.animationIndex);
                    ArchAction.Delay(() => { 
                        animator.SetInteger("Index", 0); 
                    }, 1f);

                }
            }
        }


        public async Task TransitionIn()
        {
            animator.SetTrigger("TransitionIn");

            transitionPlaying = false;

            while (!transitionPlaying)
            {
                await Task.Yield();
            }
        }

        public async Task TransitionOut()
        {
            transitionPlaying = true;

            animator.SetTrigger("TransitionOut");

            while (transitionPlaying)
            {
                await Task.Yield();
            }
        }

        public void EndTransition()
        {
            transitionPlaying = false;
        }



    }
}
