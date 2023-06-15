using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

namespace Architome
{
    [RequireComponent(typeof(ItemSlotHandler))]
    public class ItemContainerModule : MonoBehaviour
    {
        ItemContainer itemContainer;
        ModuleInfo module;
        WorldModuleCore moduleCore;

        [Serializable]
        struct Info
        {
            public TextMeshProUGUI moduleTitle;
            public Image[] stars;
            public Transform slotParent;
        }


        [SerializeField] Info info;

        public List<InventorySlot> slots;
        [SerializeField] List<ItemData> itemDatas;

        bool active;

        
        public void SetItemContainer(ItemContainer itemContainer)
        {
            module = GetComponent<ModuleInfo>();
            module.SetActive(false, false);
            this.itemContainer = itemContainer;

            module.OnActiveChange += HandleActiveChange;

            GetDependencies();
            active = true;

            ArchAction.Yield(() => module.SetActive(true, true));
        }

        void GetDependencies()
        {
            var itemSlotHandler = GetComponent<ItemSlotHandler>();

            if (itemSlotHandler)
            {
                itemSlotHandler.OnChangeItem += OnChangeItem;
            }

            CreateSlots();
            CreateItems();
        }

        void CreateSlots()
        {
            if (info.slotParent == null) return;
            if (module.prefabs.inventorySlot == null) return;

            slots = new();

            for (int i = 0; i < itemContainer.maxCapacity; i++)
            {
                var newSlot = Instantiate(module.prefabs.inventorySlot, info.slotParent).GetComponent<InventorySlot>();
                newSlot.interactable = false;
                slots.Add(newSlot);
            }
        }

        void CreateItems()
        {
            if (info.slotParent == null) return;
            if (slots == null) return;
            if (module.prefabs.item == null) return;

            foreach (var itemData in itemContainer.items)
            {
                if (itemData.item == null) continue;
                var slot = FirstAvailableSlot();

                if (slot == null) return;

                var newItem = module.CreateItem(itemData, true);

                newItem.HandleNewSlot(slot);


                ArchAction.Yield(() => newItem.ReturnToSlot());
            }
        }
        public InventorySlot FirstAvailableSlot()
        {
            foreach (var slot in slots)
            {
                if (slot.currentItemInfo != null) continue;
                return slot;
            }

            return null;
        }
        public void OnChangeItem(ItemEventData eventData)
        {

            HandlePreviousItem();
            HandleNewItem();
            var slot = eventData.itemSlot;

            if (!slots.Contains(slot)) return;

            var index = slots.IndexOf(slot);

            var info = eventData.newItem;

            if (info == null)
            {
                itemContainer.items[index].item = null;
                itemContainer.items[index].amount = 0;
                return;
            }


            itemContainer.items[index].item = info.item;
            itemContainer.items[index].amount = info.currentStacks;

            void HandlePreviousItem()
            {
                if (eventData.previousItem == null) return;
                eventData.previousItem.OnItemAction -= OnItemAction;
            }

            void HandleNewItem()
            {
                if (eventData.newItem == null) return;
                eventData.newItem.OnItemAction += OnItemAction;
            }
        }

        public void OnItemAction(ItemInfo info)
        {
            //var entity = chest.info.entityOpened;

            //var inventory = entity.GetComponentInChildren<Inventory>();

            //if (inventory == null) return;


            //var success = inventory.LootItem(info);

            //var index = inventory.FirstAvailableSlotIndex();

            //inventory.inventoryItems[index].item = info.item;
            //inventory.inventoryItems[index].amount = info.currentStacks;

            //if (inventory.entityInventoryUI)
            //{
            //    inventory.entityInventoryUI.RedrawInventory();
            //}

            //info.OnItemAction -= OnItemAction;
            //Destroy(info.gameObject);
        }

        public async Task UntilClose()
        {
            while (active)
            {
                await Task.Yield();
            }
        }

        void HandleActiveChange(bool isActive)
        {
            if (isActive) return;
            Close();
        }

        public void Close()
        {
            if (!active) return;
            active = false;
            module.SetActive(false, true);
            ArchAction.Delay(() => {
                Destroy(gameObject);
            }, 1f);
        }

    }

}