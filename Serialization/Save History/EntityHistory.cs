using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.History
{
    [Serializable]
    public class EntityHistory
    {
        public static EntityHistory active
        {
            get;
            private set;
        }

        public Dictionary<int, int> playerKills;
        public Dictionary<int, int> guildMemberDeaths;
        

        int EntityId(EntityInfo entity)
        {
            return entity._id;
        }

        public void AddKilledEntity(EntityInfo entity, int amount = 1)
        {
            playerKills ??= new();

            var name = entity.ToString();
            var id = EntityId(entity);

            if (!playerKills.ContainsKey(id))
            {
                playerKills.Add(id, amount);
                return;
            }

            playerKills[id] += amount;

        }

        public void AddKilledEntity(int id, int amount = 1)
        {
            if (!playerKills.ContainsKey(id))
            {
                playerKills.Add(id, amount);
                return;
            }

            playerKills[id] += amount;
        }

        public void AddGuildMemberDeaths(EntityInfo entity, int amount = 1)
        {
            guildMemberDeaths ??= new();

            var name = entity.ToString();
            var id = EntityId(entity);

            if (!guildMemberDeaths.ContainsKey(id))
            {
                guildMemberDeaths.Add(id, amount);
            }

            guildMemberDeaths[id] += amount;
        }


        public void AddGuildMemberDeaths(int id, int amount = 1)
        {
            if (!guildMemberDeaths.ContainsKey(id))
            {
                guildMemberDeaths.Add(id, amount);
                return;
            }

            guildMemberDeaths[id] += amount;
        }

        public int KillCount(int entityId)
        {
            playerKills ??= new();


            if (!playerKills.ContainsKey(entityId))
            {
                playerKills.Add(entityId, 0);
            }

            return playerKills[entityId];
        }
        
        public bool HasKilled(EntityInfo entity)
        {
            var id = EntityId(entity);
            return KillCount(id) > 0;
        }

        public void SetActiveSingleTon()
        {
            active = this;

            playerKills ??= new();
            guildMemberDeaths ??= new();


            Debugger.System(() => {
                var dataMap = DataMap.active;
                var entities = dataMap._maps.entities;

                Debugger.System(4756, $"{playerKills.Count}");

                foreach(KeyValuePair<int, int> pair in playerKills)
                {
                    if (entities.ContainsKey(pair.Key))
                    {
                        Debugger.System(5412, $"Player has killed {entities[pair.Key]} {pair.Value} times");
                    }
                }
            });
        }
    }
}
