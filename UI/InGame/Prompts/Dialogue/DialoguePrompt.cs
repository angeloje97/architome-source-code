using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class DialoguePrompt : MonoBehaviour
    {
        PromptInfo prompt;
        PromptInfoData promptData;
        DialogueDataSet currentSet;

        [Serializable]
        public struct Prefabs 
        {
            public Transform entryParents;
            public DialogueEntryBehavior npcEntry, playerEntry;
        }

        public Prefabs prefabs;


        void Update()
        {
        
        }

        void GetDependencies()
        {
            prompt = GetComponent<PromptInfo>();
        }

        public void EndDialogue()
        {
            prompt.ClosePrompt();
        }

        public void SetDialogueDataSet(DialogueEventData eventData)
        {
            GetDependencies();

            var sourceEntity = eventData.sourceEntity;
            currentSet = eventData.dataSet;
            promptData = new()
            {
                autoClose = false,
                handleClose = true,
                title = sourceEntity.ToString(),
                icon = sourceEntity.PortraitIcon()
            };


            SetEntries(eventData.dataSet.entries);
            HandleData(eventData);
        }

        public void HandleData(DialogueEventData eventData)
        {
            var currentIndex = currentSet.currentDialogueData;
            var dialogueData = currentSet.data[currentIndex];

            var text = dialogueData.text;
            var options = new List<OptionData>();
            AddEntry(new(eventData.sourceEntity.ToString(), dialogueData.text));

            foreach(var option in dialogueData.dialogueOptions)
            {
                options.Add(new(option.text, (OptionData promptOptionData) => {
                    if (option.endsDialogue)
                    {
                        EndDialogue();
                        return;
                    }
                    currentSet.currentDialogueData = option.nextTarget != -1 ? option.nextTarget : currentIndex + 1;
                    AddEntry(new(eventData.listener.ToString(), option.text, true));
                    HandleData(eventData);
                }));
            }


            promptData.options = options;
            prompt.RemoveOptions();

            prompt.SetPrompt(promptData, false);
        }

        public void AddEntry(DialogueEntry entry, bool addToSet = true)
        {
            var entryPrefab = entry.fromPlayer ? prefabs.playerEntry : prefabs.npcEntry;

            var newEntry = Instantiate(entryPrefab, prefabs.entryParents);
            newEntry.SetEntry(entry);

            if (addToSet)
            {
                currentSet.entries ??= new();
                currentSet.entries.Add(entry);
            }
        }

        public void ClearEntries()
        {
            var entries = prefabs.entryParents.GetComponentsInChildren<DialogueEntryBehavior>();
            foreach (var entry in entries)
            {
                Destroy(entry.gameObject);
            }
        }

        public void SetEntries(List<DialogueEntry> entries)
        {
            ClearEntries();

            if (entries == null) return;
            foreach(var entry in entries)
            {
                AddEntry(entry, false);
            }
        }

        
    }
}
