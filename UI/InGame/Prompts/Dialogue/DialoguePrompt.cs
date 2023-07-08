using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        bool active;


        void Update()
        {
        
        }

        void GetDependencies()
        {
            prompt = GetComponent<PromptInfo>();
        }

        public void EndDialogue()
        {
            active = false;
            prompt.ClosePrompt();
        }

        public async Task DialogueToEnd()
        {
            while (active) await Task.Yield();
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

            prompt.SetPrompt(promptData, false);

            active = true;
            if (currentSet.clearAtStart)
            {
                currentSet.entries = new();
            }

            SetEntries(currentSet.entries);
            HandleData(eventData);
        }

        public async void HandleData(DialogueEventData eventData)
        {
            var currentIndex = currentSet.currentDialogueData;
            var dialogueData = currentSet.data[currentIndex];

            var text = dialogueData.text;
            var options = new List<OptionData>();
            await AddEntry(new(eventData.sourceEntity.ToString(), dialogueData.text), crawlText: true);

            foreach(var option in dialogueData.dialogueOptions)
            {
                options.Add(new(option.text, async (OptionData promptOptionData) => {

                    eventData.source.InvokeOption(option.triggerString, eventData);

                    if(option.nextTarget != -2)
                    {
                        currentSet.currentDialogueData = option.nextTarget != -1 ? option.nextTarget : currentIndex + 1;
                    }


                    if(currentSet.currentDialogueData >= currentSet.data.Count)
                    {
                        currentSet.disabled = true;
                    }

                    await AddEntry(new(eventData.listener.ToString(), option.text, true));

                    if (option.endsDialogue || currentSet.disabled)
                    {
                        eventData.source.HandleDisabled();
                        EndDialogue();
                        return;
                    }
                    prompt.RemoveOptions();
                    
                    HandleData(eventData);
                }));
            }


            promptData.options = options;

            prompt.SetPrompt(promptData, false);
        }

        public async Task AddEntry(DialogueEntry entry, bool addToSet = true, bool crawlText = false)
        {
            var entryPrefab = entry.fromPlayer ? prefabs.playerEntry : prefabs.npcEntry;

            var newEntry = Instantiate(entryPrefab, prefabs.entryParents);
            newEntry.crawlText = crawlText;
            await newEntry.SetEntry(entry);

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

        public async void SetEntries(List<DialogueEntry> entries)
        {
            ClearEntries();

            if (entries == null) return;
            var tasks = new List<Task>();
            foreach(var entry in entries)
            {
                tasks.Add(AddEntry(entry, false, false));
            }

            await Task.WhenAll(tasks);
        }

        
    }
}
