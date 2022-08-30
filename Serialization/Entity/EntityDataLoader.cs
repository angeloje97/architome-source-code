using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System.Linq;
using System.Threading.Tasks;

namespace Architome
{
    public class EntityDataLoader
    {

        static DataMap.Maps maps;
        static DataMap.Maps _maps
        {
            get
            {
                if (maps == null)
                {
                    maps = DataMap.active._maps;
                }

                if (maps != DataMap.active._maps)
                {
                    maps = DataMap.active._maps;
                }

                return maps;
            }
        }

        public static async Task<EntityInfo> SpawnEntity(EntityData data, Transform parent = null)
        {
            if (data == null) return null;
            if (data.dataIndex == -1) return null;

            var db = _maps.entities;

            var entity = db[data.id];

            var newEntity = Object.Instantiate(entity.gameObject, parent).GetComponent<EntityInfo>();

            

            await Task.Yield();

            LoadEntity(data, newEntity);

            return newEntity;


        }

        public static void LoadEntity(EntityData data, EntityInfo entity)
        {
            if (data == null || entity == null) return;
            var characterInfo = entity.GetComponentInChildren<CharacterInfo>();

            entity.SaveIndex = data.dataIndex;


            LoadInfo();
            LoadCharacter();
            LoadEquipment();
            LoadInventory();
            LoadAbilities();

            void LoadInfo()
            {
                var info = data.info;

                foreach (var field in info.GetType().GetFields())
                {
                    var otherField = entity.GetType().GetField(field.Name);

                    if (otherField == null) continue;

                    if (field.GetValue(info).GetType() != otherField.GetValue(entity).GetType()) continue;

                    otherField.SetValue(entity, field.GetValue(info));

                    if (field.Name == "health")
                    {
                        Debugger.InConsole(18935, $"{entity.entityName} | health: {entity.health}/{entity.maxHealth}");
                    }

                }


                var archClass = _maps.archClasses[info.classId];

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

                character.originalParts = data.characterData.originalParts;

                character.originalSex = data.characterData.originalSex;

                character.currentMaterial = data.characterData.materialIndex;

                character.LoadValues();
            }

            void LoadAbilities()
            {
                var abilityManager = entity.AbilityManager();
                var abilities = abilityManager.GetComponentsInChildren<AbilityInfo>();
                foreach (var abilityData in data.abilities.datas)
                {
                    var index = abilityData.index;
                    if (index < 0) continue;
                    if (index >= abilities.Length) continue;

                    abilities[index].augmentsData = abilityData.abilityInventory.ItemDatas(_maps);
                }
            }

            void LoadEquipment()
            {
                var equipmentSlots = characterInfo.GetComponentsInChildren<EquipmentSlot>();

                if (equipmentSlots.Length == 0)
                {
                    return;
                }

                var itemDatas = data.equipment.ItemDatas(_maps);

                var lastSlot = equipmentSlots[0];

                for (int i = 0; i < itemDatas.Count; i++)
                {
                    if (itemDatas[i].item == null) continue;
                    equipmentSlots[i].equipment = (Equipment)itemDatas[i].item;

                    lastSlot = equipmentSlots[i];
                }

                lastSlot.OnLoadEquipment?.Invoke(lastSlot.equipment);

                //foreach (var itemData in itemDatas)
                //{
                //    var equipment = (Equipment)itemData.item;

                //    var lastSlot = itemDatas.IndexOf(itemData) == itemDatas.Count - 1;

                //    foreach (var slot in equipmentSlots)
                //    {
                //        if (equipment.equipmentSlotType != slot.equipmentSlotType) continue;

                //        slot.equipment = equipment;

                //        if (lastSlot)
                //        {
                //            slot.OnLoadEquipment?.Invoke(equipment);
                //        }
                //    }

                //}

                //var slotMap = data.equipment.slots.ToDictionary(x => x.slotType, x => x);


                //var lastSlot = equipmentSlots[0];

                //foreach (var slot in equipmentSlots)
                //{
                //    slot.equipment = null;

                //    if (!slotMap.ContainsKey(slot.equipmentSlotType)) continue;

                //    var equipmentData = slotMap[slot.equipmentSlotType];

                //    var item = _maps.items[equipmentData.itemId];

                //    if (!Item.Equipable(item)) continue;

                //    slot.equipment = (Equipment)item;

                //    lastSlot = slot;

                //    //foreach (var equipmentData in data.equipment.slots)
                //    //{
                //    //    if (equipmentData.slotType != slot.equipmentSlotType) continue;
                //    //    var item = _maps.items[equipmentData.itemId];

                //    //    if (!Item.Equipable(item)) break;
                //    //    slot.equipment = (Equipment) item;

                //    //    lastSlot = slot;
                //    //    break;

                //    //}
                //}


                //lastSlot.OnLoadEquipment?.Invoke(lastSlot.equipment);

            }
            void LoadInventory()
            {
                var inventory = entity.GetComponentInChildren<Inventory>();

                if (inventory == null) return;

                inventory.ClearInventory();


                inventory.inventoryItems = data.inventory.ItemDatas(_maps);

                //foreach (var itemData in data.inventory.items)
                //{
                //    if (itemData.slotNumber >= inventory.inventoryItems.Count) continue;
                //    if (!_maps.items.ContainsKey(itemData.id)) continue;

                //    var item = Object.Instantiate(_maps.items[itemData.id]);

                //    if (Item.Equipable(item))
                //    {
                //        var equipment = (Equipment)item;
                //        equipment.stats = itemData.stats;
                //    }

                //    inventory.inventoryItems[itemData.slotNumber] = new() { amount = itemData.amount, item = item };
                    
                //}


                //var items = new Dictionary<EntityData.InventoryData.ItemData, Item>();

                //foreach (var itemData in data.inventory.items)
                //{
                //    var item = _maps.items[itemData.itemId];

                //    if (item == null) continue;

                //    Debugger.InConsole(45389, $"Loaded item is {item}");
                //    items.Add(itemData, item);
                //}

                //foreach (KeyValuePair<EntityData.InventoryData.ItemData, Item> itemData in items)
                //{
                //    var data = itemData.Key;
                //    if (data.slotNumber >=  inventory.inventoryItems.Count) continue;

                //    inventory.inventoryItems[data.slotNumber] = new() { item = itemData.Value, amount = data.amount };
                //}

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
