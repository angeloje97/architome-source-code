using Architome.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Architome.DevTools
{
    public class Actions : DevTool
    {
        public class Request
        {
            public Dictionary<string, Type> validAttributes;

            public Dictionary<string, object> currentParameters;
            Action<Request> action;
            public Request(Action action)
            {
                this.action += (request) => action();
            }

            public Request(Action<Request> action)
            {
                this.action += action;
            }

            public void Invoke(Dictionary<string, object> parameters)
            {
                this.currentParameters = parameters;
                action?.Invoke(this);
            }
        }
        public override List<GameState> availableStates
        {
            get
            {
                if (_availableStates == null)
                {
                    _availableStates = new()
                    {
                        GameState.Play
                    };
                }

                return _availableStates;
            }
        }
    }
}
