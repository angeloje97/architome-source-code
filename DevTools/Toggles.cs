using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.DevTools
{
    public class Toggles : DevTool
    {
        public class Request
        {
            bool currentState;

            Action<Request> OnStateChange;

            public Request(Action<bool> OnChangeState)
            {
                this.OnStateChange += (request) => {
                    OnChangeState?.Invoke(currentState);
                };
            }

            public Request(Action<Request> OnStateChange)
            {
                this.OnStateChange += (request) => OnStateChange?.Invoke(request);
            }

            public void SetState(bool state)
            {
                this.currentState = state;
                OnStateChange?.Invoke(this);
            }

            public void ToggleState()
            {
                SetState(!currentState);
            }
        }

        public Dictionary<string, Request> functions = new() 
        {

        };
    }
}
