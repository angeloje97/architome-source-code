using Architome.History;
using PixelCrushers.DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class HistoryRecorder : MonoBehaviour
    {
        EntityHistory entityHistory;
        QuestHistory questHistory;
        ArchSceneManager sceneManager;

        SaveSystem currentSaveSystem;

        public Dictionary<int, int> currentEnemyKills;
        public Dictionary<int, int> currentPlayerDeaths;
        public Dictionary<Quest, bool> questsCompleted;


        void Start()
        {
            GetDependencies();
            HandleQuests();
            HandleEntityDeathHandler();
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

        void HandleEntityDeathHandler()
        {
            var entityDeathHandler = EntityDeathHandler.active;
            if (entityDeathHandler == null) return;


            entityDeathHandler.OnEntityDeath += (CombatEventData eventData) =>
            {
                var target = eventData.target;
                if (target.IsPlayer())
                {
                    AddDeath(currentPlayerDeaths, target);
                    return;
                }

                var source = eventData.source;

                if (source.IsPlayer())
                {
                    AddDeath(currentEnemyKills, target);
                }
            };

            void AddDeath(Dictionary<int, int> records, EntityInfo entity)
            {
                var id = entity._id;

                if (!records.ContainsKey(id))
                {
                    records.Add(id, 1);
                }
                else
                {
                    records[id] += 1;
                }
            }
        }

        void HandleQuests()
        {
            var questManager = QuestManager.active;

            if (questManager == null) return;

            questManager.AddListener(QuestEvents.OnEnd, (Quest quest) =>
            {
                if (quest.info.state == Enums.QuestState.Completed)
                {
                    CompleteQuest(quest);
                }
            }, this);

            void CompleteQuest(Quest quest)
            {
                if (!questsCompleted.ContainsKey(quest))
                {
                    questsCompleted.Add(quest, true);
                }
                else { questsCompleted[quest] = true; }
            }
        }
    }
}
