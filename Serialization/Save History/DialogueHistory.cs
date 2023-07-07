using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    [Serializable]
    public class DialogueHistory
    {
        public static DialogueHistory active { get; private set; }

        Dictionary<string, DialogueDataSet> dialogueSets;
        public void SetActiveSingleTon()
        {
            active = this;
            dialogueSets ??= new();
        }

        public void SaveDialogue(string referenceName, DialogueDataSet newSet)
        {
            if (!dialogueSets.ContainsKey(referenceName))
            {
                dialogueSets.Add(referenceName, newSet);
                return;
            }

            dialogueSets[referenceName] = newSet;
        }

        public DialogueDataSet LoadDialogue(string referenceName, DialogueDataSet original)
        {
            if (!dialogueSets.ContainsKey(referenceName))
            {
                return original;
            }

            return dialogueSets[referenceName];
        }
    }
}
