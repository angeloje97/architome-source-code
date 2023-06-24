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
            public Action<DialogueData> OnDialogueStart, OnDialogueEnd;
        }

        public static ArchDialogueManager active;


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
        
        public void StartDialogue(DialogueData data)
        {
            events.OnDialogueStart?.Invoke(data);
            OnStartDialogue?.Invoke();
        }

    }
}
