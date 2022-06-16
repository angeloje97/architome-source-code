using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

namespace Architome
{
    [RequireComponent(typeof(ItemSlotHandler))]
    public class ChestModule : MonoBehaviour
    {
        ArchChest chest;
        ModuleInfo module;

        [Serializable]
        struct Info
        {
            public TextMeshProUGUI moduleTitle;
            public Image[] stars;
            public Transform slotParent;
        }


        [SerializeField] Info info;

        public List<InventorySlot> slots;

        
        public void SetChest(ArchChest chest)
        {
            this.chest = chest;

            this.chest.events.OnClose += OnClose;

            WorldModuleCore.active.OnChestOpen += OnAnotherChestOpen;

            module = GetComponent<ModuleInfo>();
            module.SetActive(false, false);

            module.OnActiveChange += OnActiveChange;

            GetDependencies();

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

            for (int i = 0; i < chest.info.maxChestSlots; i++)
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

            foreach (var itemData in chest.info.items)
            {
                if (itemData.item == null) continue;
                var slot = FirstAvailableSlot();

                if (slot == null) return;

                var newItem = module.CreateItem(itemData.item, itemData.amount, true).GetComponent<ItemInfo>();

                newItem.HandleNewSlot(slot);
                newItem.OnItemAction += OnItemAction;

                newItem.GetComponent<DragAndDrop>().enabled = false;

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
            var slot = eventData.itemSlot;

            if (!slots.Contains(slot)) return;

            var index = slots.IndexOf(slot);

            var info = eventData.newItem;

            if (info == null)
            {
                chest.info.items[index].item = null;
                chest.info.items[index].amount = 0;
                return;
            }


            chest.info.items[index].item = info.item;
            chest.info.items[index].amount = info.currentStacks;
        }

        public void OnItemAction(ItemInfo info)
        {
            var entity = chest.info.entityOpened;

            var inventory = entity.GetComponentInChildren<Inventory>();

            if (inventory == null) return;

            var index = inventory.FirstAvailableSlotIndex();

            if (index == -1) return;

            inventory.inventoryItems[index].item = info.item;
            inventory.inventoryItems[index].amount = info.currentStacks;

            if (inventory.entityInventoryUI)
            {
                inventory.entityInventoryUI.RedrawInventory();
            }

            info.OnItemAction -= OnItemAction;
            Destroy(info.gameObject);
        }

        public void OnClose(ArchChest chest)
        {
            module.SetActive(false, true);

            this.chest.events.OnClose -= OnClose;
            WorldModuleCore.active.OnChestOpen -= OnAnotherChestOpen;

            ArchAction.Delay(() => {
                Destroy(gameObject);
            }, 1f);
        }
        
        void OnActiveChange(bool isActive)
        {
            if (isActive) return;

            chest.GetComponent<WorkInfo>().RemoveAllLingers();
        }

        void OnAnotherChestOpen(ArchChest chest)
        {
            OnClose(this.chest);
        }
    }

}