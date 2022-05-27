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
        // Update is called once per frame

        public bool PickUpItem(Item item)
        {
            if (ItemCount() == maxSlots) { return false; }

            var clone = Instantiate(item);
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i] == null)
                {
                    inventoryItems[i].item = item;
                }
            }

            entityInventoryUI.CreateItem(clone, entityInventoryUI.FirstAvailableSlot());

            return true;
        }


    }

}