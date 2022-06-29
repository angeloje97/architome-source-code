using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System.Linq;

namespace Architome
{
    public class DungeonQuestGenerator : MonoBehaviour
    {
        MapInfo map;
        public GameObject questPrefab;
        public bool generateForcesKilled;
        public bool generateBossKilled;

        public Quest questGenerated;

        void Start()
        {
            

            GetDependencies();
        }

        void GetDependencies()
        {
            map = MapInfo.active;

            if (map)
            {
                if (map.generateEntities)
                {
                    map.EntityGenerator().OnEntitiesGenerated += OnEntitiesGenerated;
                }
            }

            var sceneManager = ArchSceneManager.active;

            if (sceneManager)
            {
                sceneManager.BeforeLoadScene += BeforeLoadScene;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        void BeforeLoadScene(ArchSceneManager manager)
        {
            if (questGenerated == null) return;
            if (questGenerated.info.state == QuestState.Completed) return;

            questGenerated.FailQuest();

        }

        public void OnEntitiesGenerated(MapEntityGenerator generator)
        {
            if(questPrefab == null) { return; }
            var dungeonQuest = QuestManager.active.AddQuest(questPrefab);



            dungeonQuest.SetSource(this, "Dungeon Quest");

            var objectives = new GameObject("Objectives");
            objectives.transform.SetParent(dungeonQuest.transform);

            bool activeObjectives = false;


            questGenerated = dungeonQuest;
            var entities = generator.GetComponentsInChildren<EntityInfo>();

            HandleGenerateForces();
            HandleGenerateBoss();

            if (!activeObjectives)
            {
                QuestManager.active.DeleteQuest(dungeonQuest);
                return;
            }
            
            dungeonQuest.questName = "Complete Dungeon Objectives";
            dungeonQuest.Activate();

            void HandleGenerateForces()
            {
                if (!generateForcesKilled) return;
                var enemyForces = entities.Where(entity => entity.rarity != EntityRarity.Boss).ToList();
                if (enemyForces.Count == 0) return;

                var killEntities = objectives.AddComponent<ObjectiveKillEnemyForces>();

                foreach(var entity in enemyForces)
                {
                    if(entity.npcType == NPCType.Hostile && entity.rarity != EntityRarity.Boss)
                    {
                        killEntities.HandleEntity(entity);
                    }
                }

                var difficulty = DifficultyModifications.active;

                if (difficulty)
                {
                    killEntities.percentageNeeded = difficulty.settings.minimumEnemyForces;
                }

                activeObjectives = true;

            }

            void HandleGenerateBoss()
            {
                if (!generateBossKilled) { return; }
                var count = 0;
                foreach (var entity in entities)
                {
                    if (entity.rarity != EntityRarity.Boss) continue;

                    var killEntity = objectives.AddComponent<ObjectiveKillEntity>();

                    killEntity.HandleEntity(entity);
                    count++;
                }

                if (count > 0) activeObjectives = true;
            }

            


        }
    }
}

