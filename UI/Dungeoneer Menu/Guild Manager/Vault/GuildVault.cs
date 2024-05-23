using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Architome
{
    public class GuildVault : MonoActor
    {
        public static GuildVault active;

        [SerializeField] GuildManager manager;
        [SerializeField] Transform vaultParent;
        [SerializeField] Transform inventorySlots;

        List<InventorySlot> slots;
        List<ItemData> items;

        public Currency defaultSellCurrency;

        private void Start()
        {
            var itemSlotHandler = GetComponent<ItemSlotHandler>();

            if (itemSlotHandler)
            {
                itemSlotHandler.OnChangeItem += OnChangeItem;
                itemSlotHandler.AddListener(eItemEvent.OnItemAction, OnItemAction, this);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            active = this;
        }

        #region Initialization
        public void InitializeModule()
        {
            manager.guildInfo.vault ??= new();
            items = manager.guildInfo.vault;

            CreateInventorySlots(manager.guildInfo.maxSlots);
            CreateItems();
        }
        void CreateInventorySlots(int amount)
        {
            if (vaultParent == null) return;
            var dungeoneering = DungeoneerManager.active;

            var slotPrefab = dungeoneering.prefabs.inventorySlot;
            if (slotPrefab == null) return;

            slots = new();

            for (int i = 0; i < amount; i++)
            {
                slots.Add(Instantiate(slotPrefab, inventorySlots).GetComponent<InventorySlot>());
            }

        }
        void CreateItems()
        {
            SaveSystem.Operate((SaveGame save) => {
                CoreLoader.LoadInventory(save.guildData.inventory, slots);
            });
        }
        public bool VaultItem(ItemInfo item)
        {
            var stored =  item.InsertIntoSlots(slots);

            Debugger.UI(8719, $"Successfully stored {item} in vault: {stored}");

            return stored;
        }

        public ItemInfo StackableItem(ItemInfo item)
        {
            foreach (var slot in slots)
            {
                if (slot.currentItemInfo == null) continue;
                if (!slot.currentItemInfo.SameItem(item)) continue;
                if (!slot.currentItemInfo.OpenToStack()) continue;

                return slot.currentItemInfo;
            }

            return null;
        }

        public InventorySlot AvailableSlot()
        {
            foreach (var slot in slots)
            {
                if (slot.currentItemInfo != null) continue;
                return slot;
            }

            return null;
        }

        #endregion

        #region Event Listeners
        private void OnChangeItem(ItemEventData eventData)
        {
            var slot = eventData.itemSlot;
            if (!slots.Contains(slot)) return;
            var index = slots.IndexOf(slot);

            while(index > items.Count)
            {
                items.Add(new());
            }

            items[index] = new(eventData.newItem);
        }
        async void OnItemAction(ItemEventData eventData)
        {
            var info = eventData.newItem;
            var contextMenu = ContextMenu.current;
            if (contextMenu == null) return;

            var options = new List<ContextMenu.OptionData>()
            {
                new("Destroy", async(data) => {
                    await info.SafeDestroy();
                }),
            };

            HandleOptions();

            var userChoice = await contextMenu.UserChoice(new() 
            { 
                title = info.item.itemName,
                options = options
            });

            if (userChoice.index == -1) return;


            void HandleOptions()
            {

                if (info.currentStacks > 1)
                {
                    options.Insert(0, new("Split in Half", (data) => {
                        var availableSlot = AvailableSlot();
                        if (availableSlot == null) return;

                        var newItem = info.SplitHalf();

                        if (newItem == null) return;

                        newItem.HandleNewSlot(availableSlot);
                    }));
                    options.Insert(0, new("Split", async(data) => {
                        var availableSlot = AvailableSlot();

                        if (availableSlot == null) return;

                        var newItem = await info.HandleSplit();

                        if (newItem == null) return;

                        newItem.HandleNewSlot(availableSlot);
                    }));
                }

                


                if (!info.item.IsCurrency())
                {
                    options.Insert(0, new("Sell", (data) => {
                        manager.GainCurrency(defaultSellCurrency, (int)(info.item.value * info.currentStacks));
                        info.DestroySelf(true);
                    }));
                }
                else
                {
                    options.Insert(0, new(info.item.UseString(), (data) => {
                        info.item.Use(new() { guildManager = manager, itemInfo = info });
                    }));
                }

                if (info.item.GetType() == typeof(LootBox))
                {
                    options.Insert(0, new(info.item.UseString(), (data) => {
                        info.item.Use(new()
                        {
                            itemInfo = info,
                            slots = slots,
                        });
                    }));
                }


            }

        }
        #endregion

        #region Slot Management
        void UpdateInventorySlots()
        {
            var slotPrefab = DungeoneerManager.active.prefabs.inventorySlot;
            while (slots.Count < manager.guildInfo.maxSlots)
            {
                slots.Add(Instantiate(slotPrefab, inventorySlots).GetComponent<InventorySlot>());
            }
        }
        public void AddSlot()
        {



            manager.guildInfo.maxSlots++;
            items.Add(new());
            UpdateInventorySlots();
        }
        public int SlotPrice()
        {
            if (slots == null) return 0;

            return 12 * slots.Count;
        }
        #endregion
    }
}
