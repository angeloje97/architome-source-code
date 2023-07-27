using Architome.History;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class GuildQuestsDisplay : GuildHistoryDisplay
    {
        QuestHistory questHistory;
        protected override void GetDependencies()
        {
            base.GetDependencies();

            questHistory = QuestHistory.active;

            GatherData();
        }

        
        void GatherData()
        {
            if (questHistory == null) return;
            var completedQuests = questHistory.completedQuests;
            currentLogs = new();
            if (completedQuests == null) return;

            foreach(KeyValuePair<string, bool> item in completedQuests)
            {
                var completedText = item.Value ? "Completed" : "Incomeplete";
                currentLogs.Add($"{item.Key}: {completedText}");
            }
        }
    }
}
