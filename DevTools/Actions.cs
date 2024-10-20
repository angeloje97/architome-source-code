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

            Action<ActionRequest> action;
            public ActionRequest(string name, Action action) : base (name)
            {
                this.action += (request) => action();
            }

            public ActionRequest(string name, Action<ActionRequest> action) : base(name)
            {
                this.action += action;
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

        public readonly List<Request> requests = new()
        {
            new("Kill All PartyMembers", () => {

            }),

            new("Teleport Party Members", (Request request) => {

            })
            {
                attributes = new()
                {
                    { "X", typeof(int) },
                    { "Y", typeof(int) },
                    { "Z", typeof(int) },
                }
            },

            new("Kill Entity", (Request request) => {
                var targetHandler = ContainerTargetables.active;

                foreach(var selected in targetHandler.selectedTargets)
                {
                    var entity = selected.GetComponent<EntityInfo>();
                    entity.KillSelf();
                }
            }),

            new("Spawn Entity", (Request request) => { })
            {
                attributes = new()
                {
                    { "EntityID", typeof(int) },
                    { "Entity Level", typeof(int) },
                    { "EntityRarity", typeof(EntityRarity) }
                }
            }
        };
    }
}
