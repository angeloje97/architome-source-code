using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Architome
{
    public class GuildVault : MonoBehaviour
    {
        public static GuildVault active;

        [SerializeField] GuildManager manager;
        [SerializeField] Transform vaultParent;
        [SerializeField] Transform inventorySlots;

        List<InventorySlot> slots;
        List<ItemData> items;

        private void Start()
        {
            var itemSlotHandler = GetComponent<ItemSlotHandler>();

            if (itemSlotHandler)
            {
                itemSlotHandler.OnChangeItem += OnChangeItem;
                itemSlotHandler.OnItemAction += OnItemAction;
            }
        }

        private void Awake()
        {
            active = this;
        }

        public void InitializeModule()
        {
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
            var itemInfoPrefab = DungeoneerManager.active.prefabs.itemTemplate;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].item == null) continue;
                if (i < 0 || i >= slots.Count) continue;

                var slot = slots[i];
                var newItemInfo = Instantiate(itemInfoPrefab, vaultParent).GetComponent<ItemInfo>();

                newItemInfo.ManifestItem(items[i], true);

                newItemInfo.HandleNewSlot(slot);

                newItemInfo.ReturnToSlot(3);
            }
        }
        public bool VaultItem(ItemInfo item)
        {
            var stored =  item.InsertIntoSlots(slots);

            Debugger.UI(8719, $"Successfully stored {item} in vault: {stored}");

            return stored;
            //Debugger.UI(9024, $"Stackable item : {stackableItem}");

            //while (stackableItem)
            //{
            //    item.HandleItem(stackableItem);

            //    if (item.currentStacks <= 0) return true;

            //    stackableItem = StackableItem(item);
            //}

            //if (item.currentStacks == 0) return true;

            //var newSlot = AvailableSlot();

            //if (newSlot)
            //{
            //    item.HandleNewSlot(newSlot);
            //    return true;
            //}

            //return false;
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

        private void OnChangeItem(ItemEventData eventData)
        {
            var slot = eventData.itemSlot;
            if (!slots.Contains(slot)) return;
            var index = slots.IndexOf(slot);
            items[index] = new(eventData.newItem);
        }
        async void OnItemAction(ItemInfo info)
        {
            var contextMenu = ContextMenu.current;
            if (contextMenu == null) return;

            var options = new List<string>()
            {
                "Destroy",
            };

            


            HandleOptions();


            

            var userChoice = await contextMenu.UserChoice(new() 
            { 
                title = info.item.itemName,
                options = options
            });

            if (userChoice.index == -1) return;



            HandleSplit();
            HandleHalfSplit();
            HandleClaim();
            HandleDestroy();
            HandleSell();


            void HandleOptions()
            {

                if (info.currentStacks > 1)
                {
                    options.Insert(0, "Split in Half");
                    options.Insert(0, "Split");
                }

                
                if (!info.item.IsCurrency())
                {
                    options.Insert(0, "Sell");
                }
                else
                {
                    options.Insert(0, "Claim");
                }
            }


            async void HandleDestroy()
            {
                if (userChoice.stringValue != "Destroy") return;
                await info.SafeDestroy();
            }

            async void HandleSplit()
            {
                if (userChoice.stringValue != "Split") return;
                var availableSlot = AvailableSlot();

                if (availableSlot == null) return;

                var newItem = await info.HandleSplit();

                if (newItem == null) return;

                newItem.HandleNewSlot(availableSlot);
            }

            void HandleHalfSplit()
            {
                if (userChoice.stringValue != "Split in Half") return;
                var availableSlot = AvailableSlot();
                if (availableSlot == null) return;

                var newItem = info.SplitHalf();

                if (newItem == null) return;

                newItem.HandleNewSlot(availableSlot);
            }

            void HandleClaim()
            {
                if (userChoice.stringValue != "Claim") return;

                info.item.Use(new() { guildManager = manager, itemInfo = info });
            }

            void HandleSell()
            {
                if (userChoice.stringValue != "Sell") return;
            }

        }

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
    }
}
