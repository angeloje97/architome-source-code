using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Architome
{
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


        public List<EntityInfo> tier1Entities;
        
        public List<EntityInfo> tier2Entities;
        
        public List<EntityInfo> tier3Entities;
        
        public List<SpawnerInfo> tier1Spawners;
        public List<SpawnerInfo> tier2Spawners;
        public List<GameObject> neutralEntities;
        public List<GameObject> bossEntities;
        public List<ArchChest> chests;
        public List<PatrolGroup> patrolGroups;

        private void OnValidate()
        {
            CleanChests();
            void CleanChests()
            {
                if (chests == null) return;
                for (int i = 0; i < chests.Count; i++)
                {
                    var chest = chests[i].GetComponent<ArchChest>();

                    if (chest == null)
                    {
                        chests.RemoveAt(i);
                        i--;
                    }
                }
            }

        }
    }
}
