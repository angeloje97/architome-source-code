using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using System;
using System.Threading.Tasks;

namespace Architome
{
    public class Transition : MonoBehaviour
    {
        [Serializable]
        public struct Info
        {
            public Animator animator;
            public ScreenTransitionHandler handler;
        }

        [SerializeField] Info info;

        public bool active;

        bool transitioning = false;

        private void OnValidate()
        {
            UpdateHandler();
        }

        private void OnDestroy()
        {
            UpdateHandler();
        }

        public void SetActive(bool active)
        {
            info.animator.SetBool("IsPlaying", active);
        }

        void UpdateHandler()
        {

            info.handler = GetComponentInParent<ScreenTransitionHandler>(true);
            var handler = info.handler;
            var transitions = handler.transitions;

            if (transitions == null) return;

            if (active)
            {
                handler.SetActiveTransition(this);
            }

            RemoveNulls();
            AddTransition();

            void AddTransition()
            {
                if (!handler.transitions.Contains(this))
                {
                    handler.transitions.Add(this);
                }
            }
            void RemoveNulls()
            {
                var handler = GetComponentInParent<ScreenTransitionHandler>();
                var existingTransitions = new HashSet<Transition>();

                for (int i = 0; i < transitions.Count; i++)
                {
                    var transition = transitions[i];
                    if (transition == null)
                    {

                        handler.transitions.RemoveAt(i);
                        continue;
                    }

                    if (existingTransitions.Contains(transition))
                    {
                        handler.transitions.RemoveAt(i);
                        continue;

                    }

                    existingTransitions.Add(transition);

                    
                }
            }
        }

        public void SetTransition(bool active)
        {
            this.active = active;
        }

        async public Task SceneTransitionIn()
        {
            Debugger.InConsole(89429, $"{this} transitioning");

            
            transitioning = true;

            info.animator.SetBool("IsPlaying", false);

            while (transitioning)
            {
                await Task.Yield();
            }
        }

        async public Task SceneTransitionOut()
        {
            
            transitioning = true;

            info.animator.SetBool("IsPlaying", true);

            while (transitioning)
            {
                await Task.Yield();
            }
        }

        public void EndTransition()
        {
            transitioning = false;
        }
    }
}
