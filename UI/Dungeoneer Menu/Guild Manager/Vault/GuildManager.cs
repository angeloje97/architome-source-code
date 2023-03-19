using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System;
using System.Threading.Tasks;
using static UnityEngine.EventSystems.EventTrigger;

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
            public List<ItemData> currencies;
            public List<ItemData> vault = new(20);

            public int level = 1;
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

        KeyBindings keybindManager;

        [SerializeField] bool updateVault;
        

        public Dictionary<EntityInfo, EntitySlot> slotMap;
        public Action<EntityInfo> OnSelectEntity { get; set; }
        public Action<List<ItemData>> OnCurrenciesChange { get; set; }

        public Action<Currency, int, List<bool>> OnCanSpendCheck { get; set; }
        public Action<Currency, int, List<bool>> OnOutSourceSpendCheck { get; set; }

        [Serializable]
        public struct States
        {
            public bool choosingCardAction;


            bool choosingCardActionCheck;
            public Action<bool> OnChoosingActionChange;

            public void HandleEvents()
            {
                if (choosingCardActionCheck != choosingCardAction)
                {
                    choosingCardActionCheck = choosingCardAction;
                    OnChoosingActionChange?.Invoke(choosingCardAction);
                }
            }
        }

        public States states;

        private void Awake()
        {
            active = this;
            
            
        }


        private void Start()
        {
            GetDependencies();
            LoadGuildData();
            AcquireSlots();
            InitiateModules();

        }

        private void OnValidate()
        {
            HandleUpdateVault();

            void HandleUpdateVault()
            {
                if (!updateVault) return;
                updateVault = false;

                for (int i = 0; i < guildInfo.currencies.Count; i++)
                {
                    var currency = guildInfo.currencies[i];
                    if (currency == null) continue;
                    if (currency.item.IsCurrency()) continue;

                    guildInfo.currencies.RemoveAt(i);
                    i--;

                }
            }
        }

        private void Update()
        {
            states.HandleEvents();
        }


        void GetDependencies()
        {
            var pauseMenu = PauseMenu.active;

            manager = GetComponentInParent<DungeoneerManager>();
            keybindManager = KeyBindings.active;
            var saveSystem = SaveSystem.active;
            ArchSceneManager.active.AddListener(SceneEvent.BeforeLoadScene, BeforeLoadScene, this);
            

            if (manager)
            {
                manager.OnNewEntity += OnNewEntity;
                manager.OnLoadSave += OnLoadSave;

            }
            if (pauseMenu)
            {
                pauseMenu.OnTryOpenPause += OnEscape;
            }

            if (saveSystem)
            {
                saveSystem.AddListener(SaveEvent.BeforeSave, BeforeSave, this);
            }

            foreach(var slot in partySlots)
            {
                var toolTipElement = slot.GetComponent<ToolTipElement>();
                if (!toolTipElement) continue;

                toolTipElement.OnCanShowCheck += (ToolTipElement element) => {
                    HandleToolTip(slot.entity, element);
                };
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
            var toolTipElement = card.GetComponent<ToolTipElement>();
            rosterCards.Add(card);

            var save = manager.currentSave;

            if (toolTipElement)
            {
                toolTipElement.OnCanShowCheck += (ToolTipElement element) => { HandleToolTip(card.entity, element);  };
            }

            if (save == null) return;

            var entityIndex = entity.SaveIndex;

            if (save.selectedEntitiesIndex == null) return;
            if (!save.selectedEntitiesIndex.Contains(entityIndex)) return;

            var selectedIndex = save.selectedEntitiesIndex.IndexOf(entityIndex);

            var partySlot = partySlots[selectedIndex];

            RosterToParty(partySlot, card);


        }

        public int CurrencyIndex(Currency currency)
        {
            var currencies = guildInfo.currencies;

            if (currencies != null)
            {
                for(int i = 0; i < currencies.Count; i++)
                {
                    if (currencies[i].item.Equals(currency))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public void GainCurrency(Currency currency, int amount)
        {
            guildInfo.currencies ??= new();
            HandleCurrency();
            //OnCurrenciesChange?.Invoke(guildInfo.currencies);
            ArchAction.Yield(() => OnCurrenciesChange?.Invoke(guildInfo.currencies));

            void HandleCurrency()
            {
                foreach (var guildCurrency in guildInfo.currencies)
                {
                    var isEqual = guildCurrency.item.Equals(currency);

                    Debugger.UI(5349, $"{currency.itemName} == {guildCurrency.item.itemName} : {isEqual}");

                    if (!isEqual) continue;
                    guildCurrency.amount += amount;
                    return;
                }

                guildInfo.currencies.Add(new() { item = currency, amount = amount });

            }

        }

        public bool CanSpend(Currency currency, int amount)
        {
            var currencyIndex = CurrencyIndex(currency);
            if (currencyIndex == -1) return false;
            if (guildInfo.currencies[currencyIndex].amount < amount) return false;


            return true;
        }

        public bool OutSourceSpends(Currency currency, int amount)
        {
            var checks = new List<bool>();

            OnOutSourceSpendCheck?.Invoke(currency, amount, checks);

            foreach(var check in checks)
            {
                if (check) return true;
            }

            return false;
        }

        public bool SpendCurrency(Currency currency, int amount)
        {
            var currencies = guildInfo.currencies;

            if (OutSourceSpends(currency, amount)) return true;
            if (currencies == null) return false;
            var currencyIndex = CurrencyIndex(currency);
            if (currencyIndex == -1) return false;

            if (currencies[currencyIndex].amount < amount) return false;

            currencies[currencyIndex].amount -= amount;
            ArchAction.Yield(() => OnCurrenciesChange?.Invoke(guildInfo.currencies));

            return true;
            

        }
        public bool HasCurrency(Currency currency, int amount)
        {
            foreach (var guildCurrency in guildInfo.currencies)
            {
                if (!guildCurrency.item.Equals(currency)) continue;
                if (guildCurrency.amount < amount) continue;

                return true;
            }

            return false;
        }
        void BeforeLoadScene(ArchSceneManager sceneManager)
        {
            SaveEntities();
            var currentSave = SaveSystem.current;
            if (currentSave != null)
            {
                currentSave.guildData = new(this);
            }
        }

        void BeforeSave(SaveSystem system, SaveGame save)
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
            var currentSave = SaveSystem.current;
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

        public void HandleToolTip(object entityObject, ToolTipElement element)
        {
            if(entityObject == null)
            {
                element.checks.Add(false);
                return;
            }

            if(entityObject.GetType() != typeof(EntityInfo))
            {
                element.checks.Add(false);
                return;
            }

            var entity = (EntityInfo)entityObject;

            var rightClickIndex = keybindManager.SpriteIndex(KeyCode.Mouse1);

            element.data = new()
            {
                name = $"{entity}",
                description = $"Use <sprite={rightClickIndex}> for more options."
            };
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



        public async void HandleEntityAction(object entityData)
        {
            if (entityData == null) return;
            if (entityData.GetType() != typeof(EntityInfo)) return;
            var entity = (EntityInfo)entityData;
            if (entity == null) return;

            while (states.choosingCardAction)
            {
                await Task.Yield();
            }

            states.choosingCardAction = true;

            ContextMenu.OptionData firstChoice = InParty(entity) ?
                new("Remove from party", (data) => {
                    slotMap[entity].entity = null;
                }) : 
                new("Add to party", (data) => {
                    var slot = FirstAvailablePartySlot(entity.role);
                    slot.entity = entity;
                });


            var choices = new List<ContextMenu.OptionData>()
            {
                firstChoice,
                new("Equipment", (data) => {
                    if (equipmentModule == null) return;
                    equipmentModule.SelectEntity(entity);
                    equipmentModule.SetActive(true);

                }),
            };

            var response = await ContextMenu.current.UserChoice(new()
            {
                title = $"{entity.entityName}",
                options = choices
            });

            var userOption = response.index;

            states.choosingCardAction = false;

            if (userOption == -1) return;

            //HandleAddToParty();
            //HandleRemoveFromParty();
            //HandleEquipment();
            UpdatePartyManager(true);
        }

        public void OnPartySlotSelect(EntitySlot slot)
        {

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

        void UnusedFunction()
        {

        }

        
    }
}
