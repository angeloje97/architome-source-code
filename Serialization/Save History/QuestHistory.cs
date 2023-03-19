using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.History
{

    [Serializable]
    public class QuestHistory
    {
        public static QuestHistory active { get; private set; }
        public Dictionary<string, bool> completedQuests;

        public void CompleteQuest(Quest quest)
        {
            completedQuests ??= new();
            var questName = quest.ToString();

            if (!completedQuests.ContainsKey(questName))
            {
                completedQuests.Add(questName, true);
                return;
            }

            completedQuests[questName] = true;
        }

        public bool IsComplete(string questName)
        {
            completedQuests ??= new();

            if (!completedQuests.ContainsKey(questName))
            {
                completedQuests.Add(questName, false);
            }

            return completedQuests[questName];
        }

        public void SetActiveSingleTon()
        {
            active = this;
        }
    }
}
