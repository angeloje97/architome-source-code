using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System;
using System.Threading.Tasks;

namespace Architome
{
    public class GuildManager : MonoBehaviour
    {
        public static GuildManager active { get; private set; }
        public GuildInfo guildInfo;
        [Serializable]
        public class GuildInfo
        {
            public int maxSlots = 20;
            public List<ItemData> vault = new(20);
            public int level = 1;
            public int gold = 500;
        }


        public DungeoneerManager manager;

        public List<EntitySlot> partySlots;
        public List<EntityCard> rosterCards;


        public EntityInfo selectedEntity;
        

        [Serializable]
        public struct Info
        {
            public Transform rosterCardParents;
            public Transform partySlotParent;
        }

        [Serializable]
        public struct Prefabs
        {
            public GameObject rosterSlotTemplate;
            public GameObject partySlotTemplate;
        }

        public Info info;
        public Prefabs prefabs;

        [Header("Modules")]
        public GuildVault guildVault;
        public ModuleInfo equipmentModule;
        

        public Dictionary<EntityInfo, EntitySlot> slotMap;
        public Action<EntityInfo> OnSelectEntity;
        public Action<int> OnChangeGold;

        private void Awake()
        {
            active = this;
            
            GetDependencies();
            LoadGuildData();
            AcquireSlots();
        }


        private void Start()
        {
            InitiateModules();
        }

        void GetDependencies()
        {
            var pauseMenu = PauseMenu.active;

            manager = GetComponentInParent<DungeoneerManager>();
            ArchSceneManager.active.BeforeLoadScene += BeforeLoadScene;
            SaveSystem.active.BeforeSave += BeforeSave;

            if (manager)
            {
                manager.OnNewEntity += OnNewEntity;
                manager.BeforeCheckCondition += BeforeCheckCondition;
                manager.OnLoadSave += OnLoadSave;

                if (manager.info.entityIcons != null)
                {
                    foreach (var icon in manager.info.entityIcons)
                    {
                        icon.OnIconAction += OnIconAction;
                    }
                }
            }
            if (pauseMenu)
            {
                pauseMenu.OnTryOpenPause += OnEscape;
            }
            

        }

        void InitiateModules()
        {
            guildVault.InitializeModule();
        }

        void LoadGuildData()
        {
            var currentSave = Core.currentSave;
            if (currentSave == null) return;

            var guildData = currentSave.guildData;
            if (guildData == null)
            {

                
                currentSave.guildData = new(this);
                currentSave.guildData.inventory.maxSlots = guildInfo.maxSlots;
                return;
            }

            guildData.LoadData(guildInfo);
        }
        void AcquireSlots()
        {
            foreach (var slot in partySlots)
            {
                slot.OnSlotAction += OnPartySlotAction;
                slot.OnSlotSelect += OnPartySlotSelect;
            }
        }
        public void OnNewEntity(EntityInfo entity)
        {
            var rosterTemplate = prefabs.rosterSlotTemplate;
            if (rosterTemplate == null) return;
            var parent = info.rosterCardParents;
            if (parent == null) return;
            var iconTemplate = manager.prefabs.entityCard;
            if (iconTemplate == null) return;

            var card = Instantiate(iconTemplate, parent).GetComponent<EntityCard>();
            card.SetEntity(entity);


            card.OnSelect += OnSelectCard;
            card.OnAction += OnCardAction;
            rosterCards.Add(card);

            var save = manager.currentSave;

            if (save == null) return;

            var entityIndex = entity.SaveIndex;

            if (save.selectedEntitiesIndex == null) return;
            if (!save.selectedEntitiesIndex.Contains(entityIndex)) return;

            var selectedIndex = save.selectedEntitiesIndex.IndexOf(entityIndex);

            var partySlot = partySlots[selectedIndex];

            RosterToParty(partySlot, card);


        }
        public bool SpendGold(int amount)
        {
            if (guildInfo.gold < amount) return false;

            guildInfo.gold -= amount;

            OnChangeGold?.Invoke(guildInfo.gold);

            return true;
        }

        public bool GainGold(int amount)
        {

            guildInfo.gold += amount;

            OnChangeGold?.Invoke(guildInfo.gold);

            return true;
        }

        void BeforeLoadScene(ArchSceneManager sceneManager)
        {
            SaveEntities();
        }

        void BeforeSave(SaveGame save)
        {
            save.guildData = new(this);
            SaveEntities();
        }

        void OnEscape(PauseMenu menu)
        {
            if (equipmentModule == null) return;
            if (!equipmentModule.isActive) return;

            menu.pauseBlocked = true;

            equipmentModule.SetActive(false);
        }

        void SaveEntities()
        {
            var currentSave = Core.currentSave;
            if (currentSave == null) return;

            foreach (var entity in manager.entities.pool)
            {
                currentSave.SaveEntity(entity);
            }

            currentSave.selectedEntitiesIndex = new();

            foreach (var slot in partySlots)
            {
                if (slot.entity == null) continue;
                currentSave.selectedEntitiesIndex.Add(slot.entity.SaveIndex);
            }
        }

        public void BeforeCheckCondition(List<bool> conditions)
        {
            if (slotMap == null)
            {
                conditions.Add(false);
                return;
            }

            conditions.Add(slotMap.Count == 5);
        }

        public bool InParty(EntityInfo entity)
        {
            if (entity == null) return false;
            foreach (var slot in partySlots)
            {
                if (slot.entity == entity) return true;
            }

            return false;
        }


        async void HandleEntityAction(object entityData)
        {
            if (entityData == null) return;
            if (entityData.GetType() != typeof(EntityInfo)) return;
            var entity = (EntityInfo)entityData;

            if (entity == null) return;

            var firstChoice = InParty(entity) ? "Remove from party" : "Add to party";

            var choices = new List<string>()
            {
                firstChoice,
                "Equipment",
            };

            var response = await ContextMenu.current.UserChoice(new()
            {
                title = $"{entity.entityName}",
                options = choices
            });

            var userOption = response.index;

            if (userOption == -1) return;

            HandleAddToParty();
            HandleRemoveFromParty();
            HandleEquipment();
            UpdatePartyManager(true);

            void HandleAddToParty()
            {
                if (response.stringValue != "Add to party") return;

                var slot = FirstAvailablePartySlot(entity.role);

                slot.entity = entity;

                //RosterToParty(slot, card);
            }

            void HandleRemoveFromParty()
            {
                if (response.stringValue != "Remove from party") return;

                slotMap[entity].entity = null;
            }

            void HandleEquipment()
            {
                if (response.stringValue != "Equipment") return;
                if (equipmentModule == null) return;

                equipmentModule.SelectEntity(entity);
                equipmentModule.SetActive(true);

            }
        }

        public void OnPartySlotSelect(EntitySlot slot)
        {

        }

        public void OnIconAction(Icon icon)
        {
            HandleEntityAction(icon.data);
        }

        public void OnPartySlotAction(EntitySlot slot)
        {
            HandleEntityAction(slot.entity);
        }

        public void OnCardAction(EntityCard card)
        {
            HandleEntityAction(card.entity);
        }
        public void OnSelectCard(EntityCard slotIcon)
        {
            

        }

        public void UpdatePartyManager(bool updateDungeoneerManager = true)
        {
            UpdateDictionary();
            //UpdateRoster();

            if (updateDungeoneerManager)
            {
                UpdateManager();
            }

            void UpdateDictionary()
            {
                slotMap = new();

                foreach (var slot in partySlots)
                {
                    if (slot.entity == null) continue;
                    slotMap.Add(slot.entity, slot);
                }
            }

            //void UpdateRoster()
            //{
            //    foreach (var card in rosterCards)
            //    {
            //        if (card.entity == null) continue;
            //        if (slotMap.ContainsKey(card.entity))
            //        {
            //            card.SetCard(false);
            //        }
            //        else
            //        {
            //            card.SetCard(true);
            //        }
            //    }
            //}

            void UpdateManager()
            {
                if (manager == null) return;

                var newList = new List<EntityInfo>();

                foreach (var keyValuePair in slotMap)
                {
                    var entity = keyValuePair.Key;

                    newList.Add(entity);
                }

                manager.SetSelectedEntities(newList, PartyLevel());

            }

        }

        public void RosterToParty(EntitySlot slot, EntityCard card)
        {

            slot.entity = card.entity;

        }
        public void OnLoadSave(SaveGame save)
        {
            UpdatePartyManager();
        }
        public EntitySlot FirstAvailablePartySlot(Role role)
        {
            foreach (var slot in partySlots)
            {
                if (slot.entity != null) continue;
                if (slot.slotRole != role) continue;

                return slot;
                
            }

            return null;
        }
        public float PartyLevel()
        {
            var totalEntityLevels = 0f;

            foreach (var slot in partySlots)
            {
                if (slot.entity == null) continue;
                totalEntityLevels += slot.entity.entityStats.Level;
            }

            return totalEntityLevels / 5f;
        }

        
    }
}
