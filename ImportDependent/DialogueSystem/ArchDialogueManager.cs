using Architome.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Architome
{
    public class ArchDialogueManager : MonoBehaviour
    {
        public struct Events {
            public Action<DialogueEventData> OnDialogueStart, OnDialogueEnd;
        }

        public static ArchDialogueManager active;

        ArchInput archInput;

        TaskQueueHandler taskQueue;

        private void Awake()
        {
            if (active)
            {
                Destroy(gameObject);
                return;
            }
            active = this;

            ArchGeneric.DontDestroyOnLoad(gameObject, true);
        }



        public Events events;

        public UnityEvent OnStartDialogue;
        
        private void Start()
        {
            if (active != this) return;
            archInput = ArchInput.active;
            taskQueue = new();
        }
        public void StartDialogue(DialogueEventData data)
        {

            taskQueue.AddTask(async () => {
                var active = true;

                archInput.SetTempInput(ArchInputMode.InGameUI, (object obj) => {
                    return active;
                });
                events.OnDialogueStart?.Invoke(data);
                OnStartDialogue?.Invoke();
                var promptHandler = PromptHandler.active;

                var dialoguePrompt = await promptHandler.DialoguePrompt();
                dialoguePrompt.SetDialogueDataSet(data);

                await dialoguePrompt.DialogueToEnd();
                active = false;
            });
        }

    }
}
