using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
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

        
        public void OnValidate()
        {
            transitions = GetComponentsInChildren<Transition>().ToList();
        }

        public void SetActiveTransition(Transition trans)
        {
            foreach (var transition in transitions)
            {
                transition.SetTransition(transition == trans);
            }

            info.activeTransition = trans;
        }

        void Start()
        {
            sceneManager = ArchSceneManager.active;
            var gameManager = GameManager.active;
            if (sceneManager)
            {
                sceneManager.BeforeLoadScene += TasksBeforeLoadScene;
                

                if (gameManager.GameState == Enums.GameState.Play)
                {
                    sceneManager.OnLoadScene += OnLoadScene;
                }
            }
        }

        public void TasksBeforeLoadScene(ArchSceneManager archSceneManager)
        {
            var tasks = archSceneManager.tasksBeforeLoad;
            var activeTransition = info.activeTransition;
            if (activeTransition == null) return;

            

            tasks.Add(activeTransition.SceneTransitionIn());
            activeTransition.transform.SetAsLastSibling();

            transform.SetAsLastSibling();
        }


        public async void OnLoadScene(ArchSceneManager sceneManager)
        {
            var activeTransition = info.activeTransition;
            if (activeTransition == null) return;

            await activeTransition.SceneTransitionOut();
            transform.SetAsLastSibling();
            activeTransition.transform.SetAsLastSibling();
        }

        void Update()
        {
        
        }
    }
}
