using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System.Linq;

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
                var enemyForces = entities.Where(entity => entity.rarity != EntityRarity.Boss).ToList();
                if (enemyForces.Count == 0)
                {
                    killEntities.prompt = "Slay Enemy Forces (Completed)";
                    ArchAction.Delay(() => 
                    {
                        killEntities.CompleteObjective();
                    }, .125f);
                    return;
                }
                foreach(var entity in enemyForces)
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
                var bosses = entities.Where(entity => entity.rarity == EntityRarity.Boss).ToList();
                var count = 0;
                foreach (var entity in bosses)
                {
                    if (count > 0)
                    {
                        var newObjective = Instantiate(killEntity, dungeonQuest.transform);
                        killEntity = newObjective.GetComponent<ObjectiveKillEntity>();
                    }

                    killEntity.HandleEntity(entity);
                    count++;
                    
                }

            }

            dungeonQuest.questName = "Complete Dungeon Objectives";
            dungeonQuest.Activate();


        }
    }
}

