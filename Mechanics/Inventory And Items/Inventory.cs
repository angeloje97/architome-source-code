using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Architome
{
    public class Inventory : MonoBehaviour
    {
        // Start is called before the first frame update
        public EntityInventoryUI entityInventoryUI;
        public List<ItemData> inventoryItems;
        public int maxSlots = 5;

        //public Action<Item> OnNewItem;
        //public Action<Item> OnRemoveItem;

        public Action<Inventory> OnLoadInventory;


        void Start()
        {

        }

        private void Update()
        {
            HandleMaxSlots();
        }

        void HandleMaxSlots()
        {
            if (maxSlots != inventoryItems.Count)
            {
                inventoryItems.Clear();
                for (int i = 0; i < inventoryItems.Count; i++)
                {
                    if (i >= inventoryItems.Count)
                    {
                        inventoryItems.Add(null);
                    }
                }
            }
        }

        public int FirstAvailableSlotIndex()
        {
            foreach (var data in inventoryItems)
            {
                if (data.item != null) continue;

                return inventoryItems.IndexOf(data);
            }

            return -1;
        }

        public void ClearInventory()
        {
            inventoryItems.Clear();
            for (int i = 0; i < maxSlots; i++)
            {
                inventoryItems.Add(new());
            }
        }

        public int ItemCount()
        {
            int count = 0;
            foreach (var itemData in inventoryItems)
            {
                if (itemData.item != null)
                {
                    count++;
                }
            }

            return count;
        }

        private void OnValidate()
        {
            for (int i = 0; i < maxSlots; i++)
            {
                if (i >= inventoryItems.Count)
                {
                    inventoryItems.Add(null);
                }

                if (i >= inventoryItems.Count)
                {
                    inventoryItems.Add(null);
                }

                if (inventoryItems[i] != null) { continue; }
            }
        }


        public bool LootItem(ItemInfo info)
        {
            if (LootViaModule())
            {
                return true;
            }

            if (HardLoot())
            {
                return true;
            }


            return false;
            

            bool LootViaModule()
            {
                if (entityInventoryUI == null) return false;

                var slot = entityInventoryUI.FirstAvailableSlot();

                if (slot == null) return false;

                info.HandleNewSlot(slot);

                if (slot.currentItemInfo != info) return false;


                var dragAndDrop = info.GetComponent<DragAndDrop>();

                dragAndDrop.enabled = true;

                return true;
            }

            bool HardLoot()
            {
                var slotIndex = FirstAvailableSlotIndex();

                if (slotIndex == -1) return false;

                inventoryItems[slotIndex].item = info.item;
                inventoryItems[slotIndex].amount = info.currentStacks;

                Destroy(info.gameObject);

                return true;
            }
        }
        // Update is called once per frame

        public bool PickUpItem(ItemInfo info)
        {
            if (ItemCount() == maxSlots) { return false; }



            var currentStacks = info.currentStacks;


            for (int i = 0; i < inventoryItems.Count; i++)
            {
                var data = inventoryItems[i];
                if (data.item == null) continue;
                if (data.item != info.item) continue;

                if (data.amount + currentStacks <= data.item.maxStacks)
                {
                    data.amount += currentStacks;
                    return true;
                }


                currentStacks = data.item.maxStacks - data.amount;
                data.amount = data.item.maxStacks;
            }

            if (currentStacks <= 0) return true;

            int index = -1;

            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i] == null)
                {
                    index = i;
                    break;
                }
            }

            var slot = entityInventoryUI.InventorySlot(index);

            if (slot == null) return false;

            var itemData = new ItemData()
            {
                item = info.item,
                amount = currentStacks,
            };

            entityInventoryUI.CreateItem(itemData, entityInventoryUI.FirstAvailableSlot());

            return true;
        }


    }

}