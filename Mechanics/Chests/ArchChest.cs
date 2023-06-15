using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.Events;
using Architome.Enums;



namespace Architome
{
    public enum InteractionType
    {
        DropItems,
        ShowModule,
    }
    [RequireComponent(typeof(WorkInfo))]
    public class ArchChest : MonoBehaviour
    {

        public InteractionType interactionType;

        [Serializable]
        public struct Info
        {
            [Header("Chest Properties")]
            public int level;
            public Rarity rarity;
            [Range(0, 3)]
            public int stars;
            public int maxChestSlots;
            public int minItems;
            public int maxItems;
            public bool useItemPool;
            //public bool useRequestData;
            //public ItemPool.RequestData requestData;
            public bool createItemPools; // OnValidateField
            public List<ItemData> items;
            public ItemPool itemPool;

            [Header("Interactions")]
            public EntityInfo entityOpened;
            public WorkInfo station;
        }

        [Serializable]
        public struct Effects
        {
            public bool noLootTurnsOffEffects;

            [Header("Sound Effects")]
            public AudioClip onOpenSound;
            public AudioClip whileOpenSound;
            public AudioClip onCloseSound;
            public AudioClip whileClosedSound;


            [Header("Particle Effects")]
            public ParticleSystem onOpenParticle;
            public ParticleSystem whileOpenParticle;
            public ParticleSystem onCloseParticle;
            public ParticleSystem whileClosedParticle;


        }

        public Info info;
        public Effects effects;

        public struct Events
        {
            public Action<ArchChest> OnOpen;
            public Action<ArchChest> OnClose;
        }

        public Events events;
        public bool isOpen;

        ModuleInfo currentModule;
        List<InventorySlot> slots;
        bool moduleOpen;

        public UnityEvent OnOpen;
        public UnityEvent OnClose;

        void GetDependencies()
        {
            info.station = GetComponent<WorkInfo>();
        }
        void Start()
        {
            GetDependencies();
            CreateItemsFromItemPool();
        }

        private void OnValidate()
        {
            if (!info.createItemPools) return; info.createItemPools = false;



            if (info.itemPool == null) return;


            var itemsFromPool = info.itemPool.ItemsFromRarity(info.rarity, new() {
                minItems = info.minItems,
                maxItems = info.maxItems,
                useMinMax = true,
                replaceNull = true,
                chanceMultiplier = 3,
                uniqueItems = true,
            });

            SetItems(itemsFromPool);
        }

        void CreateItemsFromItemPool()
        {
            if (!info.useItemPool) return;
            if (info.itemPool == null) return;

            var world = World.active;

            var chestRarityProperty = world.RarityProperty(info.rarity);

            var itemsFromPool = info.itemPool.ItemsFromRarity(info.rarity, new() {
                minItems = info.minItems,
                maxItems = info.maxItems,
                useMinMax = true,
                replaceNull = true,
                uniqueItems = true,
                chanceMultiplier = chestRarityProperty.valueMultiplier });

            SetItems(itemsFromPool);
        }
        public void Open(TaskEventData eventData)
        {
            info.entityOpened = eventData.task.CurrentWorkers[0];

            events.OnOpen?.Invoke(this);
            isOpen = true;
            OnOpen?.Invoke();

            
            if (interactionType == InteractionType.ShowModule)
            {

                ChestRoutine();
            }
            else
            {
                DropItems();
            }

        }

        async void DropItems()
        {
            var world = WorldActions.active;
            if (world == null) return;
            foreach (var item in info.items)
            {
                if (item.item == null) continue;
                world.DropItem(item, transform.position + new Vector3(0, 1.5F, 0), true, true);

                await Task.Delay(333);
            }
        }

        

        private void OnModuleChange(bool isActive)
        {
            if (isActive) return;
            info.station.RemoveAllLingers();
            moduleOpen = false;
            currentModule.OnActiveChange -= OnModuleChange;
        }


        async void ChestRoutine()
        {
            await EntityBrowsing();
            Close();
        }

        public async Task EntityBrowsing()
        {
            if (info.entityOpened == null) return;
            if (info.entityOpened.Movement() == null) return;
            if (info.station == null) return;

            CreateChestUI();

            while (info.station.IsOfWorkStation(info.entityOpened.Target()))
            {
                await Task.Yield();
            }

        }

        void CreateChestUI()
        {
            var itemBin = IGGUIInfo.active.CreateItemBin();
            currentModule = itemBin.GetComponent<ModuleInfo>();
            itemBin.SetItemBin(new()
            {
                title = "Chest",
                items = info.items
            });

            currentModule.SetActive(false, false);
            ArchAction.Yield(() => {
                currentModule.SetActive(true);
                currentModule.OnActiveChange += OnModuleChange;
            });


            slots = itemBin.Slots();
            var slotHandler = itemBin.GetComponent<ItemSlotHandler>();

            slotHandler.OnChangeItem += OnChangeItem;

        }

        void OnChangeItem(ItemEventData eventData)
        {
            if (eventData.previousItem)
            {
                eventData.previousItem.OnItemAction -= OnItemAction;
            }

            if (eventData.newItem)
            {
                eventData.newItem.OnItemAction += OnItemAction;
            }
        }

        async void OnItemAction(ItemInfo item)
        {
            var entity = info.entityOpened;
            if (entity == null) return;

            var contextMenu = ContextMenu.current;

            if (contextMenu == null) return;

            await contextMenu.UserChoice(new() {
                title = $"{item}",
                options = new()
                {
                    new("Loot Item", HandleLootItem)
                }
            });

            void HandleLootItem()
            {
                entity.LootItem(item);
            }

        }

        public void SetItems(List<ItemData> items)
        {
            info.items = items;

            while(info.items.Count < info.maxItems)
            {
                info.items.Add(new());
            }

            var world = World.active;
            if (!world) return;

            foreach (var itemData in info.items)
            {
                if (itemData.item == null) continue;
                if (!Item.Equipable(itemData.item)) continue;

                var equipment = (Equipment)itemData.item;

                var rarityProperty = world.RarityRoll(info.rarity);

                var itemLevel = (int)(info.level * rarityProperty.valueMultiplier);

                if (itemLevel <= 0) itemLevel = 1;

                equipment.SetPower(info.level, itemLevel, rarityProperty.name);
            }
        }

        public void Close()
        {
            if (currentModule && moduleOpen)
            {
                currentModule.SetActive(false, true);
                moduleOpen = false;
            }

            isOpen = false;
            events.OnClose?.Invoke(this);
            OnClose?.Invoke();
            info.entityOpened = null;
        }
    }
}