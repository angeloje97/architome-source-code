using Architome.History;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class HistoryRecorder : MonoBehaviour
    {
        MapEntityGenerator entityGenerator;
        EntityHistory entityHistory;
        QuestHistory questHistory;
        DungeonQuestGenerator questGenerator;
        ArchSceneManager sceneManager;

        SaveSystem currentSaveSystem;

        public Dictionary<int, int> currentEnemyKills;
        public Dictionary<int, int> currentPlayerDeaths;
        public Dictionary<Quest, bool> questsCompleted;


        void Start()
        {
            GetDependencies();
            HandleEntityGenerator();
            HandleQuestGenerator();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        void GetDependencies()
        {
            sceneManager = ArchSceneManager.active;
            currentEnemyKills = new();
            currentPlayerDeaths = new();
            questsCompleted = new();

            if (sceneManager)
            {
                sceneManager.AddListener(SceneEvent.OnLoadScene, () => {
                    HandleEntityGenerator();
                    HandleQuestGenerator();
                }, this);
            }

            currentSaveSystem = SaveSystem.active;

            if (currentSaveSystem)
            {
                currentSaveSystem.AddListener(SaveEvent.BeforeSave, BeforeSaveGame, this);
            }
        }

        void BeforeSaveGame(SaveSystem system, SaveGame currentSave)
        {
            if (currentSave == null) return;

            HandleEntityHistory();
            HandleQuestHistory();

            currentEnemyKills = new();
            questsCompleted = new();
            currentPlayerDeaths = new();

            void HandleEntityHistory()
            {
                entityHistory = EntityHistory.active;
                if (entityHistory == null) return;

                foreach(KeyValuePair<int, int> pair in currentEnemyKills)
                {
                    entityHistory.AddKilledEntity(pair.Key, pair.Value);
                }

                foreach(KeyValuePair<int, int> pair in currentPlayerDeaths)
                {
                    entityHistory.AddGuildMemberDeaths(pair.Key, pair.Value);
                }
            }

            void HandleQuestHistory()
            {
                questHistory = QuestHistory.active;
                if (questHistory == null) return;

                foreach(KeyValuePair<Quest, bool> pair in questsCompleted)
                {
                    questHistory.CompleteQuest(pair.Key);
                }
                
            }
        }

        void HandleEntityGenerator()
        {
            entityGenerator = MapEntityGenerator.active;

            if (entityGenerator == null) return;


            entityGenerator.OnGenerateEntity += (MapEntityGenerator generator, EntityInfo entity) => {
                var id = entity._id;
                entity.OnDeath += (CombatEventData combatData) => {
                    if (!combatData.source.IsPlayer()) return;

                    Debugger.System(6141, $"Added {entity} to the list of kills");
                    if (!currentEnemyKills.ContainsKey(id))
                    {
                        currentEnemyKills.Add(id, 1);
                    }
                    else
                    {
                        currentEnemyKills[id]++;
                    }
                }; 
            };
        }

        void HandleQuestGenerator()
        {
            questGenerator = DungeonQuestGenerator.active;
            questHistory = QuestHistory.active;

            if (questGenerator == null || questHistory == null) return;

            questGenerator.OnGenerateQuest += (Quest quest) => {
                quest.OnCompleted += (Quest quest) => {
                    if (!questsCompleted.ContainsKey(quest))
                    {
                        questsCompleted.Add(quest, true);
                    }
                    else
                    {
                        questsCompleted[quest] = true;
                    }
                };
            };

        }
    }
}
