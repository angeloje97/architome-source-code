using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class DialoguePrompt : MonoBehaviour
    {
        PromptInfo prompt;

        DialogueDataSet currentSet;

        // Update is called once per frame
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

        public void StartDialoguePrompt(DialogueEventData eventData)
        {
            GetDependencies();

            SetDialogueDataSet(eventData.dataSet);

            prompt.SetPrompt(new()
            {
                autoClose = false,
                title = eventData.sourceEntity.ToString(),
            }, false);
        }

        public void SetDialogueDataSet(DialogueDataSet set)
        {
            currentSet = set;
        }
    }
}
