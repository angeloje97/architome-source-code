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

            }
            void LoadInventory()
            {
                var inventory = entity.GetComponentInChildren<Inventory>();

                if (inventory == null) return;

                inventory.ClearInventory();


                inventory.inventoryItems = data.inventory.ItemDatas(_maps);

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
