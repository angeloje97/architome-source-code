using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using UnityEditor.Rendering;
using LootLabels;
using JetBrains.Annotations;

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
        [SerializeField] List<ItemPool> itemPools;
        [SerializeField] bool dropItemsOnDeath;
        [SerializeField] bool generateItemPool;
        [SerializeField] bool variableItemValue;
        [SerializeField] bool testItemPool;
        [SerializeField] bool migrateItemPool;
        


        public override void GetDependencies()
        {
            if (entityInfo)
            {
                entityInfo.infoEvents.OnLootItem += OnLootItem;
                entityInfo.infoEvents.OnCanPickUpCheck += OnCanPickUp;

                if (dropItemsOnDeath)
                {
                    combatEvents.AddListenerHealth(eHealthEvent.OnDeath, OnEntityDeath, this);
                }
            }

            HandleItemPools();
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
                HandleItemPools();

                generateItemPool = temp;
            }

            HandleItemPoolMigration();
            
        }

        void HandleItemPoolMigration()
        {
            if (!migrateItemPool) return;
            migrateItemPool = false;
            if (itemPool == null) return;
            itemPools ??= new();
            if (itemPools.Contains(itemPool)) return;

            itemPools.Add(itemPool);

        }

        void HandleItemPools()
        {
            if (!generateItemPool) return;
            if (itemPools == null) return;
            inventoryItems = new();
            var rarity = entityInfo != null ? entityInfo.rarity : EntityRarity.Common;

            foreach (var itemPool in itemPools)
            {
                HandleItemPool(itemPool);
            }

            HandleVariableValues();


            void HandleItemPool(ItemPool itemPool)
            {
                if (!generateItemPool) return;
                if (itemPool == null) return;

                var pooledItems = itemPool.ItemsFromEntityRarity(rarity, new()
                {
                    maxItems = maxSlots
                });

                foreach (var item in pooledItems)
                {
                    inventoryItems.Add(item);
                }

                
            }

            void HandleVariableValues()
            {
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

        }
        


        void OnEntityDeath(HealthEvent eventData)
        {
            DropInventory();
        }

        async void DropInventory()
        {
            var worldActions = WorldActions.active;
            if (worldActions == null) return;

            foreach (var data in inventoryItems)
            {
                if (data.item == null) continue;
                var droppedItem = worldActions.DropItem(data, entityInfo.transform.position, true, true);
                entityInfo.infoEvents.OnDropItem?.Invoke(new(droppedItem, entityInfo));
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

        public void OnCanPickUp(ItemData itemData, List<bool> checks)
        {
            foreach (var check in checks)
            {
                if (check) return;
            }


            float amount = itemData.amount;
            int availableSlots = 0;
            foreach (var invData in inventoryItems)
            {
                if (invData.item == null)
                {
                    availableSlots++;
                    continue;
                }
                var item = invData.item;
                if (!item.Equals(itemData.item)) continue;

                var space = item.MaxStacks - itemData.amount;
                amount -= space;
            }

            if (amount <= 0) checks.Add(true);
            else if(availableSlots > 0) checks.Add(true);

        }


        public void OnLootItem(LootEventData eventData)
        {
            Debugger.UI(8716, $"Item is mutable {eventData.isMutable}");
            if (!eventData.isMutable) return;

            eventData.SetSuccessful(LootItem(eventData.itemInfo));

        }

        public List<ItemInfo> ItemsInInventory(ItemData data)
        {
            var items = new List<ItemInfo>();
            if (entityInventoryUI == null) return items;
            
            foreach(var item in entityInventoryUI.ItemsFromData(data))
            {
                items.Add(item);
            }

            return items;
        }

        public void ItemsInInventory(Action<ItemInfo> itemAction, ItemData data)
        {
            entityInventoryUI.ItemsFromData(itemAction, data);
        }


        public bool LootItem(ItemInfo info)
        {
            if (info == null) return false;
            if (LootViaModule())
            {
                Debugger.UI(8717, $"Successfuly Looted Item via module");
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

                    var newItem = Instantiate(itemPrefab, entityInventoryUI.transform);

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

        

        public class LootEventData
        {
            public ItemInfo itemInfo;
            public bool fromWorld;
            public string failReason;

            public bool succesful { get; private set; }
            public  bool successfulSet { get; private set; }

            public bool isMutable 
            { 
                get 
                {
                    if (!succesful && successfulSet)
                    {
                        return false;
                    }

                    return true;
                } 
            }

            public bool dropped;
            public bool droppedFromPlayer { get; set; }

            public bool SetSuccessful(bool val)
            {
                if (successfulSet) return false;
                successfulSet = true;

                succesful = val;


                return true;
            }


            public LootEventData(ItemInfo itemInfo)
            {
                this.itemInfo = itemInfo;
                succesful = true;
                successfulSet = false;
            }

            public LootEventData(ItemInfo itemInfo, EntityInfo sourceEntity)
            {
                this.itemInfo = itemInfo;
                succesful = true;
                successfulSet = false;

                dropped = true;
                if (Entity.IsPlayer(sourceEntity))
                {
                    droppedFromPlayer = true;
                }
            }
        }
    }

    
}