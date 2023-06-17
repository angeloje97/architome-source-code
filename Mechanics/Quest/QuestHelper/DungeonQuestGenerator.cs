using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System;

namespace Architome
{
    public class DungeonQuestGenerator : MonoBehaviour
    {
        public static DungeonQuestGenerator active { get; set; }
        MapInfo map;
        public Quest questPrefab;
        public bool generateForcesKilled;
        public bool generateBossKilled;

        public Quest questGenerated;
        ArchSceneManager sceneManager;

        public Action<Quest> OnGenerateQuest;

        private void Awake()
        {
            active = this;
        }

        void Start()
        {
            if (!generateBossKilled && !generateForcesKilled) return;

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

            sceneManager = ArchSceneManager.active;

            if (sceneManager)
            {

                sceneManager.AddListener(SceneEvent.BeforeLoadScene, BeforeLoadScene, this);
                sceneManager.AddListener(SceneEvent.BeforeConfirmLoad, BeforeConfirmLoad, this);
            }
        }

        private void OnDestroy()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        void BeforeLoadScene(ArchSceneManager manager)
        {
        }

        void BeforeConfirmLoad(ArchSceneManager sceneManager)
        {
            if (questGenerated == null) return;
            if (questGenerated.info.state != QuestState.Active) return;

            Debugger.UI(3290, $"Activing {gameObject} prompt");

            sceneManager.tasksBeforeConfirmLoad.Add(CheckCancelQuest);

            async Task<bool> CheckCancelQuest()
            {
                var promptHandler = PromptHandler.active;
                if (promptHandler == null) return true;

                var proceeding = false;

                var choice = await promptHandler.GeneralPrompt(new()
                {
                    title = questGenerated.questName,
                    question = "This quest is incomplete and will be abandoned when entering the next level.",
                    options = new()
                    {
                        new("Proceed", (option) => {
                            proceeding = true;
                            questGenerated.ForceFail();
                        }),
                        new("Cancel") {isEscape = true}
                    },
                    blocksScreen = true,
                    
                });

                return proceeding;
            }
        }

        public string QuestName()
        {
            var dungeons = Core.currentDungeon;
            if (dungeons == null) return "Generated Dungeon Quest";
            var index = Core.dungeonIndex;
            if (index < 0 || index >= dungeons.Count) return "Generated Dungeon Quest";
            return dungeons[index].levelName;
        }

        async public void OnEntitiesGenerated(MapEntityGenerator generator)
        {
            if(questPrefab == null) { return; }
            var quest = questPrefab.GetComponent<Quest>();
            var dungeonQuest = QuestManager.active.AddQuest(quest);

            await Task.Delay(1000);

            dungeonQuest.SetSource(this, "Dungeon Quest");

            var objectives = new GameObject("Objectives");
            objectives.transform.SetParent(dungeonQuest.transform);

            bool activeObjectives = false;


            questGenerated = dungeonQuest;
            var entities = generator.GetComponentsInChildren<EntityInfo>();
            var bosses = new List<EntityInfo>();
            float experience = 0f;

            HandleTimer();
            HandleGenerateForces();
            HandleGenerateBoss();

            if (!activeObjectives)
            {
                QuestManager.active.DeleteQuest(dungeonQuest);
                return;
            }

            dungeonQuest.rewards.experience = experience;
            dungeonQuest.questName = QuestName();

            ArchAction.Yield(() => { 
                dungeonQuest.Activate();
                OnGenerateQuest?.Invoke(dungeonQuest);
            });



            void HandleGenerateForces()
            {
                if (!generateForcesKilled) return;
                //var enemyForces = entities.Where(entity => entity.rarity != EntityRarity.Boss).ToList();
                //if (enemyForces.Count == 0) return;

                var killEntities = objectives.AddComponent<ObjectiveKillEnemyForces>();
                var count = 0;
                foreach(var entity in entities)
                {
                    if (entity.npcType != NPCType.Hostile) continue;
                    if (entity.rarity == EntityRarity.Boss)
                    {
                        bosses.Add(entity);
                        continue;
                    }

                    killEntities.HandleEntity(entity);
                    ////experience += entity.maxHealth * .25f;
                    count++;
                }

                if (count == 0)
                {
                    Destroy(killEntities);
                    return;
                }

                var difficulty = DifficultyModifications.active;

                if (difficulty)
                {
                    killEntities.percentageNeeded = difficulty.settings.minimumEnemyForces;
                }



                activeObjectives = true;

            }

            void HandleTimer()
            {
                var timerObjective = objectives.AddComponent<ObjectiveTimer>();

                timerObjective.transform.SetAsFirstSibling();
                timerObjective.timer = 1800f;
                timerObjective.enableMemberDeaths = true;
                timerObjective.deathTimerPenalty = 15f;
            }

            void HandleGenerateBoss()
            {
                if (!generateBossKilled) { return; }
                var count = 0;

                
                foreach (var boss in bosses)
                {
                    var killEntity = objectives.AddComponent<ObjectiveKillEntity>();

                    killEntity.HandleEntity(boss);
                    //experience += entity.maxHealth * .25f;

                    count++;
                }

                if (count > 0) activeObjectives = true;
            }

            


        }
    }
}

