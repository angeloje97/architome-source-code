using Architome.History;
using PixelCrushers.DialogueSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    public class HistoryRecorder : MonoBehaviour
    {
        public static HistoryRecorder active;
        EntityHistory entityHistory;
        QuestHistory questHistory;
        ArchSceneManager sceneManager;

        SaveSystem currentSaveSystem;

        public Dictionary<int, int> currentEnemyKills;
        public Dictionary<int, int> currentPlayerDeaths;
        public Dictionary<Quest, bool> questsCompleted;
        public Dictionary<int, ItemHistoryData> itemHistoryDatas;


        public SafeEvent<(EntityInfo, Inventory.LootEventData)> OnItemHistoryChange;

        private void Awake()
        {
            active = this;

            currentEnemyKills = new();
            currentPlayerDeaths = new();
            questsCompleted = new();
            itemHistoryDatas = new();

            SetUpEvents();
        }

        void SetUpEvents()
        {
            OnItemHistoryChange = new(this);
        }


        void Start()
        {
            GetDependencies();
            HandleQuests();
            HandleEntityDeathHandler();
            HandleItems();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        void GetDependencies()
        {
            sceneManager = ArchSceneManager.active;
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
            HandleItemHistory();

            currentEnemyKills = new();
            questsCompleted = new();
            currentPlayerDeaths = new();
            itemHistoryDatas = new();

            

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

            void HandleItemHistory()
            {
                var itemHistory = ItemHistory.active;
                if (itemHistory == null) return;
                foreach(KeyValuePair<int, ItemHistoryData> pair in itemHistoryDatas)
                {
                    itemHistory.UpdateHistoryData(pair.Value);
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

        async void HandleItems()
        {
            var gameManager = GameManager.active;
            HashSet<int> itemsObtainedSinceStart = new();
            var history = ItemHistory.active;

            while(gameManager.playableParties == null)
            {
                await Task.Yield();
            }

            foreach(var party in gameManager.playableParties)
            {
                foreach(var member in party.members)
                {
                    member.infoEvents.OnLootItem += (lootEvent) => { HandleLootItem(lootEvent, member); };
                }
            }


            void HandleLootItem(Inventory.LootEventData eventData, EntityInfo entity)
            {
                var id = eventData.itemInfo.item._id;
                var info = eventData.itemInfo;
                if (!itemHistoryDatas.ContainsKey(id))
                {
                    itemHistoryDatas.Add(id, new(info.item));
                }
                itemHistoryDatas[id].obtained = true;

                HandleHistoryChange();

                void HandleHistoryChange()
                {
                    if (itemsObtainedSinceStart.Contains(id)) return;
                    itemsObtainedSinceStart.Add(id);
                    if (history.HasPickedUp(id)) return;
                    OnItemHistoryChange.Invoke((entity, eventData));
                }
            }
        }
    }
}
