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
            public string name;
            public Dictionary<string, Type> attributes;

            public Dictionary<string, object> currentParameters;
            Action<Request> action;
            public Request(string name, Action action)
            {
                this.name = name;
                this.action += (request) => action();
            }

            public Request(string name, Action<Request> action)
            {
                this.name = name;
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

        public readonly List<Request> requests = new ()
        {
            new("Kill All PartyMembers", () => {
                
            })
        };
    }
}
