using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System.Linq;

namespace Architome
{
    public class EntityDataLoader
    {
        static ArchitomeID database;
        static ArchitomeID _database
        {
            get
            {
                if (database == null)
                {
                    database = World.active.database;
                }

                return database;
            }
        }

        public static void LoadEntity(EntityData data, EntityInfo entity)
        {
            var characterInfo = entity.GetComponentInChildren<CharacterInfo>();
            
            LoadInfo();
            LoadCharacter();
            LoadEquipment();
            LoadInventory();

            void LoadInfo()
            {
                var info = data.info;

                foreach (var field in info.GetType().GetFields())
                {
                    var otherField = entity.GetType().GetField(field.Name);

                    if (otherField == null) continue;

                    if (field.GetValue(info).GetType() != otherField.GetValue(entity).GetType()) continue;

                    otherField.SetValue(entity, field.GetValue(info));

                }

                var archClass = Search.Class(_database.Classes, info.classId);

                if (archClass != null)
                {
                    entity.archClass = Object.Instantiate(archClass);
                }
            }

            void LoadCharacter()
            {
                var character = characterInfo.GetComponentInChildren<ArchitomeCharacter>();

                if (character == null) return;

                character.originalParts.Clear();

                foreach (var part in data.characterData.originalParts)
                {
                    character.originalParts.Add(part.ToVector2());
                }

                character.originalSex = data.characterData.originalSex;

                character.currentMaterial = data.characterData.materialIndex;

                character.LoadValues();
            }

            void LoadEquipment()
            {
                var equipmentSlots = characterInfo.GetComponentsInChildren<EquipmentSlot>();

                if (equipmentSlots.Length == 0)
                {
                    return;
                }

                foreach (var slot in equipmentSlots)
                {
                    slot.equipment = null;
                }

                var items = _database.Items;


                var slotMap = new Dictionary<EquipmentSlotType, Equipment>();

                foreach (var item in items)
                {
                    if (!Item.IsEquipment(item) && !Item.IsWeapon(item)) continue;

                    foreach (var slot in data.equipment.slots)
                    {
                        if (slotMap.ContainsKey(slot.slotType)) continue;
                        if (slot.itemId != item._id) continue;
                        slotMap.Add(slot.slotType, (Equipment)item);
                        break;
                    }
                }

                foreach (KeyValuePair<EquipmentSlotType, Equipment> slot in slotMap)
                {
                    var equipmentSlot = characterInfo.EquipmentSlot(slot.Key);

                    if (equipmentSlot == null) continue;
                    var clone = Object.Instantiate(slot.Value);
                    equipmentSlot.equipment = clone;
                    equipmentSlot.OnLoadEquipment?.Invoke(clone);
                }

            }

            void LoadInventory()
            {
                var inventory = entity.GetComponentInChildren<Inventory>();

                if (inventory == null) return;

                inventory.ClearInventory();

                var items = new Dictionary<EntityData.InventoryData.ItemData, Item>();

                foreach (var itemData in data.inventory.items)
                {
                    var item = Search.Item(database.Items, itemData.itemId);

                    if (item == null) continue;

                    Debugger.InConsole(45389, $"Loaded item is {item}");
                    items.Add(itemData, item);
                }

                foreach (KeyValuePair<EntityData.InventoryData.ItemData, Item> itemData in items)
                {
                    var data = itemData.Key;
                    if (data.slotNumber >=  inventory.inventoryItems.Count) continue;

                    inventory.inventoryItems[data.slotNumber] = new() { item = itemData.Value, amount = data.amount };
                }

                inventory.OnLoadInventory?.Invoke(inventory);
            }
        }
        public static void LoadEntities(List<EntityData> entityDatas, List<EntityInfo> entityInfos)
        {
            var entityDict = new Dictionary<string, EntityInfo>();

            foreach (var entity in entityInfos)
            {
                entityDict.Add(entity.entityName, entity);
            }

            foreach (var data in entityDatas)
            {
                var name = data.info.entityName;

                if (!entityDict.ContainsKey(name)) continue;

                var entity = entityDict[name];

                LoadEntity(data, entity);
            }
        }
        public static void LoadEntity(List<EntityData> datas, EntityInfo entity)
        {
            foreach (var data in datas)
            {
                if (!data.info.entityName.Equals(entity.entityName))
                {
                    continue;
                }

                LoadEntity(data, entity);

                return;
            }
        }


    }
}
