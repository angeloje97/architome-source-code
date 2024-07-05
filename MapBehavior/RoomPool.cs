using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using JetBrains.Annotations;
using System.Threading.Tasks;

namespace Architome
{
    public enum EntityTier
    {
        Tier1,
        Tier2,
        Tier3,
        Tier1Spawner,
        Tier2Spawner,
        Neutral,
        Boss,
    }

    public delegate Task TierListAction(EntityTier tier, List<EntityInfo> list);

    public class RoomPool : ScriptableObject
    {
        int id;

        public int _id
        {
            get
            {
                return idSet ? 999999 : id;
            }

            private set
            {
                id = value;
            }
        }

        bool idSet;
        [Serializable]
        public class PatrolGroup
        {
            public List<EntityInfo> entityMembers;
        }

        

        public List<EntityInfo> EntityListFromTier(EntityTier tier)
        {
            return tier switch
            {
                EntityTier.Tier1 => tier1Entities,
                EntityTier.Tier2 => tier2Entities,
                EntityTier.Tier3 => tier3Entities,
                EntityTier.Tier1Spawner => tier1Spawners,
                EntityTier.Tier2Spawner => tier2Spawners,
                EntityTier.Neutral => neutralEntities,
                EntityTier.Boss => bossEntities,
                _ => tier1Entities
            };
        }

        Dictionary<EntityTier, List<EntityInfo>> entitiesDict;


        public List<EntityInfo> tier1Entities;
        
        public List<EntityInfo> tier2Entities;
        
        public List<EntityInfo> tier3Entities;
        
        public List<EntityInfo> tier1Spawners;
        public List<EntityInfo> tier2Spawners;

        public List<EntityInfo> neutralEntities;
        public List<EntityInfo> bossEntities;

        public List<ArchChest> chests;
        public List<PatrolGroup> patrolGroups;

        private void OnValidate()
        {
            CleanChests();
            HandleDict();
            void CleanChests()
            {
                if (chests == null) return;
                for (int i = 0; i < chests.Count; i++)
                {
                    var chest = chests[i];

                    if (chest == null)
                    {
                        chests.RemoveAt(i);
                        i--;
                    }
                }
            }

            void HandleDict()
            {
                entitiesDict = new();

                foreach(EntityTier tier in Enum.GetValues(typeof(EntityTier)))
                {
                    var listFromTier = EntityListFromTier(tier);
                    if (listFromTier == null || listFromTier.Count == 0) continue;

                    entitiesDict.Add(tier, listFromTier);
                }
            }

        }
        
        public async Task HandleTierLists(TierListAction action)
        {
            foreach(KeyValuePair<EntityTier, List<EntityInfo>> keyValuePair in entitiesDict)
            {
                await action?.Invoke(keyValuePair.Key, keyValuePair.Value);
            }
        }
        

        public EntityInfo RandomEntity(EntityTier entityTier)
        {
            return ArchGeneric.RandomItem(EntityListFromTier(entityTier));
        }
    }
}
