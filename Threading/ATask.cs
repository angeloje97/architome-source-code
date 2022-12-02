using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    public class ATask : MonoBehaviour
    {
        public static ATask active;
        private Action OnUpdate { get; set; }
        World activeWorld;
        private void Awake()
        {
            active = this;
        }
        void Start()
        {
            activeWorld = World.active;
        }

        // Update is called once per frame
        void Update()
        {
            OnUpdate?.Invoke();
        }

        public static async Task Delay(Action action, float seconds)
        {
            if (active == null) return;

            var operating = true;

            active.OnUpdate += HandleUpdate;

            while (operating)
            {
                await Task.Yield();
            }

            void HandleUpdate()
            {
                if(seconds >= 0)
                {
                    seconds -= Time.deltaTime;
                    return;
                }

                action();

                active.OnUpdate -= HandleUpdate;
                operating = false;
            }
        }

        public static async Task While(Predicate<object> predicate, Action action)
        {
            if (active == null) return;
            bool operating = true;
            active.OnUpdate += HandleAction;


            while (operating)
            {
                await Task.Yield();
            }

            void HandleAction()
            {
                if (!predicate(null))
                {
                    active.OnUpdate -= HandleAction;

                    operating = false;
                    return;
                }

                action();
            }
        }
    }
}
