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
        public SaveHistory()
        {

        }

        public void SetActiveSingleTon()
        {
            active = this;

            entityHistory ??= new();
            questHistory ??= new();

            entityHistory.SetActiveSingleTon();
            questHistory.SetActiveSingleTon();
        }
    }
}
