using Architome.History;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class GuildKillsDisplay : GuildHistoryDisplay
    {
        EntityHistory entityHistory;
        protected override void GetDependencies()
        {
            base.GetDependencies();
            entityHistory = EntityHistory.active;
            GatherData();

        }

        void GatherData()
        {
            if (entityHistory.playerKills == null) return;
            if (currentDataMap == null) return;
            currentLogs = new();
            var entities = DataManager.Entities;

            foreach(var log in entityHistory.playerKills)
            {
                if (!entities.ContainsKey(log.Key)) continue;
                currentLogs.Add($"{entities[log.Key]}: {log.Value} Kills");
            }

            if(currentLogs.Count == 0)
            {
                currentLogs.Add("No History");
            }
        }
    }
}
