using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using UnityEngine.Events;
using System;
using System.Linq;

namespace Architome
{
    public class MenuRouter : MonoBehaviour
    {
        [Serializable]
        public struct Route
        {
            public GameState state;
            public GameObject targetObject;
            public UnityEvent OnSelectRoute;
        }

        [SerializeField] List<Route> routes = new();
        public Action<Route> OnSetRoute;
        public GameState currentSatet;

        void GetDependenices()
        {
            Core.OnSetState += OnSetState;

        }


        private void Awake()
        {
            GetDependenices();
        }

        public void OnSetState(GameState newState)
        {
            currentSatet = newState;

            foreach (var route in routes)
            {
                var correct = route.state == newState;

                if (route.targetObject.activeSelf != correct)
                {
                    route.targetObject.SetActive(correct);
                }

                if (!correct) continue;

                route.OnSelectRoute?.Invoke();

                OnSetRoute?.Invoke(route);


                return;
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
