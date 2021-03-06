using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.Events;
using Architome.Enums;



namespace Architome
{
    [RequireComponent(typeof(WorkInfo))]
    public class ArchChest : MonoBehaviour
    {

        [Serializable]
        public struct Info
        {
            [Header("Chest Properties")]
            public int level;
            public Rarity rarity;
            [Range(0, 3)]
            public int stars;
            public int maxChestSlots;
            public List<ItemData> items;
            public bool useItemPool;
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

        void CreateItemsFromItemPool()
        {
            if (!info.useItemPool) return;
            if (info.itemPool == null) return;

            info.items = info.itemPool.CreatePossibleItems(info.maxChestSlots);

        }
        public void Open()
        {

            if (!FindEntityThatOpened())
            {
                return;
            }

            events.OnOpen?.Invoke(this);
            isOpen = true;
            //WorldModuleCore.active.HandleChest(this);
            OnOpen?.Invoke();
            ChestRoutine();

        }

        

        private void OnModuleChange(bool isActive)
        {
            if (isActive) return;
            info.station.RemoveAllLingers();
            moduleOpen = false;
            currentModule.OnActiveChange -= OnModuleChange;
        }

        bool FindEntityThatOpened()
        {
            
            var entity = Entity.EntitiesWithinRange(transform.position, 5f).Find(entity => entity.Movement() && entity.Movement().Target() == transform);

            Debugger.InConsole(34589, $"{entity} opened {this}");
            if (entity == null) return false;

            info.entityOpened = entity;

            return true;
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

            //if (!slots.Contains(eventData.itemSlot)) return;

            //int index = slots.IndexOf(eventData.itemSlot);

            //if (eventData.newItem == null)
            //{
            //    info.items[index].item = null;
            //    info.items[index].amount = 0;
            //}
            //else
            //{
            //    info.items[index].item = eventData.newItem.item;
            //    info.items[index].amount = eventData.newItem.currentStacks;
            //}
        }

        void OnItemAction(ItemInfo item)
        {
            var entity = info.entityOpened;
            if (entity == null) return;

            entity.LootItem(item);
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