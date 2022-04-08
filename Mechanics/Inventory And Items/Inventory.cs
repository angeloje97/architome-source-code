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
        public List<Item> items;
        public int maxSlots = 5;

        public Action<Item> OnNewItem;
        public Action<Item> OnRemoveItem;

        void Start()
        {

        }

        private void Update()
        {
            HandleMaxSlots();
        }

        void HandleMaxSlots()
        {
            if (maxSlots != items.Count)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (i >= items.Count)
                    {
                        items.Add(null);
                    }
                }
            }
        }

        public int ItemCount()
        {
            int count = 0;
            foreach (Item item in items)
            {
                if (item != null)
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
                if (i >= items.Count)
                {
                    items.Add(null);
                }

                if (items[i] != null) { return; }
            }
        }
        // Update is called once per frame

        public bool PickUpItem(Item item)
        {
            if (ItemCount() == maxSlots) { return false; }

            var clone = Instantiate(item);
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == null)
                {
                    items[i] = item;
                }
            }

            entityInventoryUI.CreateItem(clone, entityInventoryUI.FirstAvailableSlot());
            OnNewItem?.Invoke(clone);

            return true;
        }



        public void RemoveItem(Item item)
        {
            foreach (Item current in items)
            {
                if (item == current)
                {
                    OnRemoveItem?.Invoke(current);
                    items.Remove(current);

                }
            }
        }
    }

}