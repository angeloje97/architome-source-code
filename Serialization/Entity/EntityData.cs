using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System;

namespace Architome
{
    [Serializable]
    public class EntityData
    {
        public int id;
        public int dataIndex;

        [Serializable] public class Info
        {
            public EntityInfo.Properties properties;
            public string entityName;
            public string entityDescription;
            public Role role;
            public Stats entityStats;
            public float health, mana, maxHealth, maxMana;
            

            public int classId; //Not Supported by iterator
            

            public Info(EntityInfo entity)
            {
                if (entity == null) return;
                foreach (var field in GetType().GetFields())
                {
                    var entityField = entity.GetType().GetField(field.Name);

                    if (entityField == null) continue;

                    var value = entityField.GetValue(entity);

                    field.SetValue(this, value);
                }

                if (entity.archClass)
                {
                    classId = entity.archClass._id;
                }

            }
        }

        [Serializable] public class InventoryData
        {
            public int maxSlots;
            [Serializable]
            public class ItemData
            {
                public int id, amount, slotNumber, LevelRequired, itemLevel;
                public Rarity rarity;
                public Stats stats;
                public List<int> buffsID;


                public ItemData(Architome.ItemData itemData, int slotNumber = 0)
                {
                    id = itemData.item._id;
                    amount = itemData.amount;
                    rarity = itemData.item.rarity;
                    this.slotNumber = slotNumber;

                    if (Item.Equipable(itemData.item))
                    {
                        var equipment = (Equipment)itemData.item;

                        stats = equipment.stats;
                        itemLevel = equipment.itemLevel;
                        LevelRequired = equipment.LevelRequired;
                        buffsID = equipment.EquipmentEffectsData();
                    }
                }

                public Architome.ItemData ArchItemData(DataMap.Maps maps)
                {
                    if (!maps.items.ContainsKey(id)) return null;
                    var item = UnityEngine.Object.Instantiate(maps.items[id]);

                    item.rarity = rarity;

                    HandleEquipment();

                    return new Architome.ItemData() { item = item, amount = amount };

                    void HandleEquipment()
                    {
                        if (!Item.Equipable(item)) return;

                        var equipment = (Equipment)item;

                        equipment.stats = stats;
                        equipment.itemLevel = itemLevel;
                        equipment.LevelRequired = LevelRequired;

                        equipment.equipmentEffects = new();
                        foreach (var buffId in buffsID)
                        {
                            if (maps.buffs.ContainsKey(buffId)) continue;

                            equipment.equipmentEffects.Add(maps.buffs[buffId].gameObject);
                        }
                    }
                }

                
            }

            public List<ItemData> items;

            //public InventoryData(Inventory inventory)
            //{
            //    if (inventory == null) return;
            //    maxSlots = inventory.maxSlots;

            //    items = new();

            //    for (int i = 0; i < inventory.inventoryItems.Count; i++)
            //    {
            //        var item = inventory.inventoryItems[i];


            //        if (item.item == null) continue;

            //        var newItemData = new ItemData() { id = item.item._id, amount = item.amount, slotNumber = i };

            //        if (Item.Equipable(item.item))
            //        {
            //            var equipment = (Equipment)item.item;
            //            newItemData.stats = equipment.stats;
            //        }


            //        items.Add(newItemData);
            //    }
            //}

            public List<Architome.ItemData> ItemDatas(DataMap.Maps maps)
            {

                var itemDatas = new List<Architome.ItemData>();


                for (int i = 0; i < maxSlots; i++)
                {
                    itemDatas.Add(new());
                }

                Debugger.UI(4330, $"Item Count : {items.Count}");
                Debugger.UI(4331, $"Item Datas Count: {itemDatas.Count}");

                foreach (var savedItem in items)
                {
                    var itemData = savedItem.ArchItemData(maps);
                    var slotNumber = savedItem.slotNumber;

                    itemDatas[slotNumber] = itemData;
                }

                //foreach (var item in items)
                //{
                //    if (item.slotNumber < 0 || item.slotNumber >= itemDatas.Count) continue;
                //    if(!maps.items.ContainsKey(item.id)) continue;
                //    var newItem = maps.items[item.id];
                //    itemDatas[item.slotNumber] = new() { amount = item.amount, item = newItem };

                //    Debugger.UI(4329, $"{newItem.itemName} : {item.slotNumber}");

                //    if (Item.Equipable(itemDatas[item.slotNumber].item))
                //    {
                //        var equipment = (Equipment) itemDatas[item.slotNumber].item;

                //        equipment.stats = item.stats;
                //    }
                //}

                return itemDatas;
            }

            public InventoryData(List<Architome.ItemData> itemDatas)
            {
                items = new();
                maxSlots = itemDatas.Count;
                for (int i = 0; i < itemDatas.Count; i++)
                {
                    var itemData = itemDatas[i];
                    if (itemData.item == null) continue;

                    var newItemData = new ItemData(itemDatas[i], i);
                    items.Add(newItemData);
                }
            }
        }
        [Serializable]
        public class EquipmentData
        {
            [Serializable]
            public class SlotData
            {
                public EquipmentSlotType slotType;
                public int itemId;
            }

            public List<SlotData> slots;

            public EquipmentData(CharacterInfo character)
            {
                if (character == null) return;
                slots = new();
                foreach (var slot in character.GetComponentsInChildren<EquipmentSlot>())
                {
                    if (slot.equipment == null) continue;
                    slots.Add(new() { slotType = slot.equipmentSlotType, itemId = slot.equipment._id });

                }
            }
        }
        [Serializable]
        public class CharacterData
        {

            public List<Vector2> originalParts;
            public int materialIndex;
            public Sex originalSex;

            public CharacterData(CharacterInfo character)
            {
                if (character == null) return;
                var architomeCharacter = character.GetComponentInChildren<ArchitomeCharacter>();
                if (architomeCharacter == null) return;

                originalSex = architomeCharacter.originalSex;
                materialIndex = architomeCharacter.currentMaterial;
                originalParts = architomeCharacter.originalParts;


            }

        }

        public string name;

        public Info info;
        public InventoryData inventory;
        public InventoryData equipment;
        //public EquipmentData equipment;
        public CharacterData characterData;

        public EntityData(EntityInfo entity, int dataIndex = -1)
        {
            id = entity._id;
            this.dataIndex = dataIndex;

            name = entity.entityName;
            info = new(entity);

            var entityInventory = entity.GetComponentInChildren<Inventory>();
            inventory = new(entityInventory.inventoryItems);

            //inventory = new(entity.GetComponentInChildren<Inventory>());

            var character = entity.GetComponentInChildren<CharacterInfo>();

            SetEquipment(character);

            characterData = new(character);
        }

        void SetEquipment(CharacterInfo character)
        {
            if (character == null)
            {
                equipment = new(new());
                return;

            }
            var itemData = new List<ItemData>();
            foreach (var equipmentSlot in character.GetComponentsInChildren<EquipmentSlot>())
            {
                if (equipmentSlot.equipment != null)
                {
                    itemData.Add(new() { item = null, amount = 0 });
                    continue;
                }
                itemData.Add(new() { item = equipmentSlot.equipment, amount = 1 });
            }

            equipment = new(itemData);
        }

    }
}
