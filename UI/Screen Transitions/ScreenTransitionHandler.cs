using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;
using System.Linq;

namespace Architome
{
    public class ScreenTransitionHandler : MonoBehaviour
    {
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

        public void OnValidate()
        {
            transitions = GetComponentsInChildren<Transition>().ToList();
            info.canvasGroup = GetComponent<CanvasGroup>();


            if (info.canvasGroup)
            {
                ArchUI.SetCanvas(info.canvasGroup, false);
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

                if (gameManager.GameState == GameState.Play)
                {
                    sceneManager.AddListener(SceneEvent.OnLoadScene, OnLoadScene, this);
                }
            }

            HandleMapRoomGenerator();
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
        void HandleMapRoomGenerator()
        {
            if (!waitForMapGeneration)
            {
                TransitionOut();
                return;
            }

            var mapRoomGenerator = MapRoomGenerator.active;
            var mapInfo = MapInfo.active;

            if (mapRoomGenerator == null)
            {
                TransitionOut();
                return;
            }
            

            info.activeTransition.SetActive(false);

            mapRoomGenerator.OnAllRoomsHidden += (MapRoomGenerator roomGenerator) =>
            {
                if (roomGenerator.hideRooms)
                {
                    TransitionOut();
                }
                else
                {
                    ArchAction.Delay(() => TransitionOut(), 1f);
                }
                
            };

        }

        public void TasksBeforeLoadScene(ArchSceneManager archSceneManager)
        {
            var tasks = archSceneManager.tasksBeforeLoad;
            var activeTransition = info.activeTransition;
            if (activeTransition == null) return;


            tasks.Add(activeTransition.SceneTransitionIn());

            transform.SetAsLastSibling();
        }

        async void TransitionOut()
        {
            var activeTransition = info.activeTransition;
            if (activeTransition == null) return;

            transform.SetAsLastSibling();
            await activeTransition.SceneTransitionOut();
        }

        public void OnLoadScene(ArchSceneManager sceneManager)
        {

            ArchAction.Delay(() => {
                if (this == null) return;
                HandleMapRoomGenerator();
            }, 2f);
        }

        
    }
}
