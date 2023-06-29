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
                title = sourceEntity.ToString(),
                icon = sourceEntity.PortraitIcon()
            };



            SetEntries(eventData.dataSet.entries);
            HandleData(currentSet.currentDialogueData, eventData);
        }

        public void HandleData(int dataIndex, DialogueEventData eventData)
        {
            var dialogueData = currentSet.data[dataIndex];

            var text = dialogueData.text;
            var options = new List<OptionData>();

            foreach(var option in dialogueData.dialogueOptions)
            {
                options.Add(new(option.text, (OptionData promptOptionData) => {
                    if (option.endsDialogue)
                    {
                        EndDialogue();
                        return;
                    }
                    var nextTarget = option.nextTarget != -1 ? option.nextTarget : dataIndex + 1;
                    AddEntry(new(eventData.listener.ToString(), option.text, true));
                    HandleData(dataIndex, eventData);
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

            foreach(var entry in entries)
            {
                AddEntry(entry, false);
            }
        }

        
    }
}
