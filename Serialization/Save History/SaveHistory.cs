using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.History
{
    [Serializable]
    public class SaveHistory
    {
        public static SaveHistory active { get; private set; }

        public EntityHistory entityHistory;
        public QuestHistory questHistory;
        public DialogueHistory dialogueHistory;

        public SaveHistory()
        {

        }

        public void SetActiveSingleTon()
        {
            active = this;

            entityHistory ??= new();
            questHistory ??= new();
            dialogueHistory ??= new();

            entityHistory.SetActiveSingleTon();
            questHistory.SetActiveSingleTon();
            dialogueHistory.SetActiveSingleTon();
        }
    }
}
