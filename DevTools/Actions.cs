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
            public Action<RequestHandler> onSetRequestHandler;
            public ActionRequest(string name, Action action) : base (name)
            {
                this.action += (request) => action();
            }

            public ActionRequest(string name, Action<ActionRequest> action) : base(name)
            {
                this.action += action;
            }

            public override void Invoke()
            {
                base.Invoke();
                action?.Invoke(this);
            }

            public override void Invoke(Dictionary<string, object> parameters)
            {
                base.Invoke(parameters);
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

        public List<ActionRequest> requests
        {
            //Free Commit
            get
            {
                return new()
                {
                    new("Test", (ActionRequest request) => {
                       var parameters = request.parameters;
                        Debugger.System(1082, "Running Test Action\n" +
                            $"Number: {parameters["number"]}\n" +
                            $"Boolean: {parameters["boolean"]}\n" +
                            $"Rarity: {parameters["rarity"]}");

                        Debugger.UI(1921, $"Testing Hot Reload");
                    }) {
                        attributes = new()
                        {
                            { "number", typeof(int) },
                            { "boolean", typeof(bool) },
                            { "rarity", typeof(Rarity) }
                        }
                    }
                    //new("Kill All PartyMembers", () => {

                    //}),

                    //new("Teleport Party Members", (Request request) => {

                    //})
                    //{
                    //    attributes = new()
                    //    {
                    //        { "X", typeof(int) },
                    //        { "Y", typeof(int) },
                    //        { "Z", typeof(int) },
                    //    }
                    //},

                    //new("Kill Entity", (Request request) => {
                    //    var targetHandler = ContainerTargetables.active;

                    //    foreach(var entity in targetHandler.selectedTargets)
                    //    {
                    //        entity.KillSelf();
                    //    }
                    //}),

                    //new("Spawn Entity", (Request request) => { })
                    //{
                    //    attributes = new()
                    //    {
                    //        { "EntityID", typeof(int) },
                    //        { "Entity Level", typeof(int) },
                    //        { "EntityRarity", typeof(EntityRarity) }
                    //    }
                    //},

                    //new("Load Scene", (Request request) => {

                    //    var scene = (ArchScene) request.parameters["Scene ID"];
                    //    var sceneManager = ArchSceneManager.active;
                    //    sceneManager.LoadScene(scene);

                    //})
                    //{
                    //    attributes = new()
                    //    {
                    //        { "Scene ID", typeof(ArchScene) },
                    //    }
                    //}
                };
            }
        }
    }
}
