using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class Inventory : EntityProp
    {
        public EntityInventoryUI entityInventoryUI;
        public List<ItemData> inventoryItems;
        public int maxSlots = 5;

        //public Action<Item> OnNewItem;
        //public Action<Item> OnRemoveItem;

        public Action<Inventory> OnLoadInventory;
        public Action<LootEventData> OnLootFail;

        [Header("Mob Drops")]
        [SerializeField] ItemPool itemPool;
        [SerializeField] bool dropItemsOnDeath;
        [SerializeField] bool generateItemPool;
        [SerializeField] bool variableItemValue;
        [SerializeField] bool testItemPool;
        


        new void GetDependencies()
        {
            base.GetDependencies();

            if (entityInfo)
            {
                entityInfo.infoEvents.OnLootItem += OnLootItem;

                if (dropItemsOnDeath)
                {
                    entityInfo.OnDeath += OnEntityDeath;
                }
            }
        }

        void Start()
        {
            GetDependencies();
            HandleItemPool();
        }

        private void Update()
        {
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

            if (testItemPool)
            {
                testItemPool = false;
                var temp = generateItemPool;
                generateItemPool = true;

                entityInfo = GetComponentInParent<EntityInfo>();
                HandleItemPool();

                generateItemPool = temp;
            }
        }
        void HandleItemPool()
        {
            if (!generateItemPool) return;
            if (itemPool == null) return;

            var rarity = entityInfo != null ? entityInfo.rarity : EntityRarity.Common;

            inventoryItems = itemPool.ItemsFromEntityRarity(maxSlots, rarity);

            if (!variableItemValue) return;
            var world = World.active;
            if (world == null) return;
            foreach (var itemData in inventoryItems)
            {
                if (!Item.Equipable(itemData.item)) continue;

                var equipment = (Equipment)itemData.item;

                var rolledRarity = world.RarityRoll(rarity);

                var level = entityInfo != null ? entityInfo.stats.Level : 1;

                var itemLevel = (int)(level * rolledRarity.valueMultiplier);


                if (itemLevel <= 0) itemLevel = 1;

                equipment.SetPower(level, itemLevel, rolledRarity.name);
            }
        }

        async void OnEntityDeath(CombatEventData eventData)
        {
            var worldActions = WorldActions.active;
            if (worldActions == null) return;

            foreach (var data in inventoryItems)
            {
                if (data.item == null) continue;
                worldActions.DropItem(data, entityInfo.transform.position, false, true);
                await Task.Delay(333);
            }
        }
        void HandleMaxSlots()
        {
            //if (maxSlots != inventoryItems.Count)
            //{
            //    inventoryItems.Clear();
            //    for (int i = 0; i < inventoryItems.Count; i++)
            //    {
            //        if (i >= inventoryItems.Count)
            //        {
            //            inventoryItems.Add(null);
            //        }
            //    }
            //}
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


        public void OnLootItem(LootEventData eventData)
        {
            eventData.succesful = LootItem(eventData.item);
        }


        public bool LootItem(ItemInfo info)
        {
            if (info == null) return false;
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



                if (info.isUI)
                {
                    var success = info.InsertIntoSlots(entityInventoryUI.inventorySlots);


                    return success;
                }
                else
                {
                    var availableSlot = entityInventoryUI.FirstAvailableSlot();
                    if (!availableSlot) return false;

                    var itemPrefab = World.active.prefabsUI.item;

                    var newItem = Instantiate(itemPrefab, entityInventoryUI.transform).GetComponent<ItemInfo>();

                    newItem.ManifestItem(new(info), true);

                    var success = newItem.InsertIntoSlots(entityInventoryUI.inventorySlots);

                    if (!success)
                    {
                        info.currentStacks = newItem.currentStacks;
                        return false;
                    }

                    return true;
                }
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

        public class LootEventData
        {
            public ItemInfo item;
            public bool succesful;
            public bool fromWorld;
            public string failReason;
        }
    }

    
}