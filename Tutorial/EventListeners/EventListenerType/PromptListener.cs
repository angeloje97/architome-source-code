using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.Tutorial
{
    public class AutoCompleteEventListener : EventListener
    {
        [Header("Auto Complete Properties")]
        [Multiline]
        public string promptText;

        PromptHandler promptHandler;

        //The purpose of this class is mainly just to send out a direction or tip and automatically completing itself.
        


        void Start()
        {
            GetDependencies();
            HandleStart();
        }

        new void GetDependencies()
        {
            base.GetDependencies();
            promptHandler = PromptHandler.active;
        }

        public override async void StartEventListener()
        {
            base.StartEventListener();

            if(promptHandler == null)
            {
                CompleteEventListener();
                return;
            }

            await promptHandler.GeneralPrompt(new()
            {
                title = title,
                question = ArchString.NextLineList(new() { 
                    Directions(),
                    Tips()
                }),
                options = new() { "Okay" }
            });

            CompleteEventListener();
        }

    }
}
