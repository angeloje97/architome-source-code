using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class DungeonQuestGenerator : MonoBehaviour
    {
        public GameObject questPrefab;
        public bool generateForcesKilled;
        public bool generateBossKilled;

        void Start()
        {
            MapInfo.active.EntityGenerator().OnEntitiesGenerated += OnEntitiesGenerated;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnEntitiesGenerated(MapEntityGenerator generator)
        {
            if(questPrefab == null) { return; }
            var dungeonQuest = QuestManager.active.AddQuest(questPrefab);


            var entities = generator.GetComponentsInChildren<EntityInfo>();

            HandleGenerateForces();
            HandleGenerateBoss();

            void HandleGenerateForces()
            {
                if (!generateForcesKilled) return;
                var killEntities = dungeonQuest.GetComponentInChildren<ObjectiveKillEntities>();

                foreach(var entity in entities)
                {
                    if(entity.npcType == NPCType.Hostile && entity.rarity != EntityRarity.Boss)
                    {
                        killEntities.HandleEntity(entity);
                    }
                }

            }

            void HandleGenerateBoss()
            {
                if (!generateBossKilled) { return; }
                var killEntity = dungeonQuest.GetComponentInChildren<ObjectiveKillEntity>();

                foreach(var entity in entities)
                {
                    if(entity.npcType == NPCType.Hostile && entity.rarity == EntityRarity.Boss)
                    {
                        killEntity.HandleEntity(entity);
                        return;
                    }
                }

            }
            dungeonQuest.Activate();


        }
    }
}

