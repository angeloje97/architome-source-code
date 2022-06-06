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
                public int id;
                public int amount;
                public int slotNumber;
                public Stats stats;

            }

            public List<ItemData> items;

            public InventoryData(Inventory inventory)
            {
                if (inventory == null) return;
                maxSlots = inventory.maxSlots;

                items = new();

                for (int i = 0; i < inventory.inventoryItems.Count; i++)
                {
                    var item = inventory.inventoryItems[i];

                    if (item.item == null) continue;

                    var newItemData = new ItemData() { id = item.item._id, amount = item.amount, slotNumber = i };

                    if (Item.Equipable(item.item))
                    {
                        var equipment = (Equipment)item.item;
                        newItemData.stats = equipment.stats;
                    }


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
        public EquipmentData equipment;
        public CharacterData characterData;

        public EntityData(EntityInfo entity, int dataIndex = -1)
        {
            id = entity._id;
            this.dataIndex = dataIndex;

            name = entity.entityName;
            info = new(entity);

            inventory = new(entity.GetComponentInChildren<Inventory>());

            var character = entity.GetComponentInChildren<CharacterInfo>();

            equipment = new(character);

            characterData = new(character);
        }

    }
}
