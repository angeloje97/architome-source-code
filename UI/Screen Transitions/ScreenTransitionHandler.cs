using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;
using System.Linq;
using System.Threading.Tasks;

namespace Architome
{
    public enum TransitionEvents
    {
        BeforeTransitionIn,
        BeforeTransitionOut,
        AfterTransitionIn,
        AfterTransitionOut,
    }
    public class ScreenTransitionHandler : MonoBehaviour
    {
        public static ScreenTransitionHandler active;
        [Serializable]
        public struct Info
        {
            public Transition activeTransition;
            public List<Transition> transitions;
            public CanvasGroup canvasGroup;
        }

        public Info info;

        public List<Transition> transitions
        {
            get
            {
                if (info.transitions == null)
                {
                    info.transitions = new();
                }

                return info.transitions;
            }

            private set
            {
                info.transitions = value;
            }
        }

        public ArchSceneManager sceneManager;

        [SerializeField] bool waitForMapGeneration;

        Dictionary<TransitionEvents, Action<ScreenTransitionHandler>> transitionEvents;

        public List<Func<Task>> tasksBeforeTransitionIn;
        public List<Func<Task>> tasksBeforeTransitionOut;

        public void OnValidate()
        {
            transitions = GetComponentsInChildren<Transition>().ToList();
            info.canvasGroup = GetComponent<CanvasGroup>();


            if (info.canvasGroup)
            {
                ArchUI.SetCanvas(info.canvasGroup, false);
            }
        }

        private void Awake()
        {
            if(active && active != this)
            {
                Destroy(gameObject);
                return;
            }
            transitionEvents = new();
            active = this;
        }

        void InvokeEvent(TransitionEvents transEvent)
        {
            if (!transitionEvents.ContainsKey(transEvent))
            {
                transitionEvents.Add(transEvent, null);
            }

            transitionEvents[transEvent]?.Invoke(this);
        }

        public void AddListener<T>(TransitionEvents eventName, Action<ScreenTransitionHandler> action, T caller) where T: Component
        {
            if (!transitionEvents.ContainsKey(eventName))
            {
                transitionEvents.Add(eventName, null);
            }

            transitionEvents[eventName] += MiddleWare;


            void MiddleWare(ScreenTransitionHandler handler)
            {
                if(caller == null)
                {
                    transitionEvents[eventName] -= MiddleWare;
                    return;
                }

                action(handler);
            }
        }


        void Start()
        {
            ArchUI.SetCanvas(info.canvasGroup, true);
            sceneManager = ArchSceneManager.active;
            var gameManager = GameManager.active;
            if (sceneManager)
            {
                sceneManager.AddListener(SceneEvent.BeforeLoadScene, TasksBeforeLoadScene, this);
                sceneManager.AddListener(SceneEvent.OnRevealScene, TransitionOut, this);
            }

            //TransitionOut();
        }

        void Update()
        {

        }
        public void SetActiveTransition(Transition trans)
        {
            foreach (var transition in transitions)
            {
                transition.SetTransition(transition == trans);
            }

            info.activeTransition = trans;
        }

        public void TasksBeforeLoadScene(ArchSceneManager archSceneManager)
        {
            var tasks = archSceneManager.tasksBeforeLoad;
            
            tasks.Add(TransitionIn);
        }

        async Task TransitionIn()
        {
            var activeTransition = info.activeTransition;
            if (activeTransition == null) return;

            tasksBeforeTransitionIn = new();

            InvokeEvent(TransitionEvents.BeforeTransitionIn);

            foreach (var task in tasksBeforeTransitionIn)
            {
                await task();
            }

            info.canvasGroup.SetCanvas(true);
            await activeTransition.SceneTransitionIn();
            
            InvokeEvent(TransitionEvents.AfterTransitionIn);
        }

        async void TransitionOut()
        {
            var activeTransition = info.activeTransition;
            if (activeTransition == null) return;


            tasksBeforeTransitionOut = new();

            InvokeEvent(TransitionEvents.BeforeTransitionOut);

            foreach(var task in tasksBeforeTransitionOut)
            {
                await task();
            }

            await activeTransition.SceneTransitionOut();
            info.canvasGroup.SetCanvas(false);

            InvokeEvent(TransitionEvents.AfterTransitionOut);
        }

        
    }
}
