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
        public static Actions active;
        public class ActionRequest : Request
        {

            public Dictionary<string, object> currentParameters;
            Action<ActionRequest> action;
            public ActionRequest(string name, Action action) : base (name)
            {
                this.action += (request) => action();
            }

            public ActionRequest(string name, Action<ActionRequest> action) : base(name)
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

        public readonly List<ActionRequest> requests = new ()
        {
            new("Kill All PartyMembers", () => {
                
            }),

            new("Teleport Party Members", () => {

            }) 
            {
                attributes = new()
                {
                    { "X", typeof(int) },
                    { "Y", typeof(int) },
                    { "Z", typeof(int) },
                } 
            },
        };
    }
}
