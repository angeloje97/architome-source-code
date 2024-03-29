using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Architome.Enums;
using System.Threading.Tasks;
using UnityEditor;

namespace Architome
{
    [CreateAssetMenu(fileName = "New Database", menuName = "Architome/Database")]
    public class ArchitomeID : ScriptableObject
    {
        public bool updateData;
        public bool sortData;
        public bool resetData;
        public bool forceID;
        public bool checkDuplicates;



        public List<BuffInfo> Buffs;
        public List<AbilityInfo> Abilities;
        public List<EntityInfo> Entities;
        public List<CatalystInfo> Catalysts;
        public List<Item> Items;
        public List<ArchClass> Classes;
        public List<RoomInfo> Rooms;

        public List<GameObject> prefabs;

        [Serializable]
        public class BuffSearch
        {
            public ArchitomeID idDataBase;
            public bool search;
            public string searchQuery;

            [Header("Settings")]
            public bool updateID;
            public bool searchAnyBuff;
            public bool emptyName;
            public bool noIcon;
            public BuffTargetType buffTargetType;
            public List<BuffInfo> results;
            public void Update()
            {
                if (!idDataBase) return;
                if (!search) return;

                HandleUpdateID();
                HandleSearch();
            }
            void HandleUpdateID()
            {
                if (!updateID) return;

                updateID = false;

                var buffs = idDataBase.Buffs.OrderBy(buff => buff._id).ToList();

                for (int i = 0; i < buffs.Count; i++)
                {
                    
                    buffs[i].SetId(i + 1, idDataBase.forceID);
                    
                }
            }
            void HandleSearch()
            {
                results = idDataBase.Buffs.Where(buff => buff.name.ToLower().Contains(searchQuery.ToLower())).ToList();


                if (!searchAnyBuff)
                {
                    results = results.Where(buff => buff.buffTargetType == buffTargetType).ToList();
                }

                if (emptyName)
                {
                    results = results.Where(buff => buff.name == null || buff.name.Length == 0).ToList();
                }

                if (noIcon)
                {
                    results = results.Where(buff => buff.buffIcon == null).ToList();
                }
            }
        }
        [Serializable]
        public class ItemIDSearch
        {
            public ArchitomeID idDatabase;
            public bool search;
            public bool anyItemType;
            public bool hasIcon;
            public bool missingIcon;
            public bool missingName;
            public ItemType itemType;
            public string searchQuery;

            public List<Item> results;

            public ItemEffects defaultEffects;
            public bool setDefaultEffects;
            public bool clearDefaultEffects;

            [Header("Actions")]
            public bool updateId;
            public bool updateMaxStacks;

            public void Update()
            {
                if (idDatabase == null) return;
                UpdateId();
                UpdateMaxStacks();
                if (!search) return;

                SetDefaultFX();
                ClearDefaultEffects();

                if (hasIcon && missingIcon)
                {
                    missingIcon = false;
                }


                if (!anyItemType)
                {
                    results = idDatabase.Items.Where(item => item.itemType == this.itemType).ToList();
                }
                else
                {
                    results = idDatabase.Items;
                }

                if (hasIcon)
                {
                    results = results.Where(item => item.itemIcon != null).ToList();
                }

                if (missingIcon)
                {
                    results = results.Where(item => item.itemIcon == null).ToList();
                }

                if (missingName)
                {
                    results = results.Where(item => item.itemName == null || item.itemName.Length == 0).ToList();
                }

                if (searchQuery.Length > 0)
                {
                    results = results.Where(item => item.name.ToLower().Contains(searchQuery.ToLower())
                    || item.itemName.ToLower().Contains(searchQuery.ToLower())).ToList();
                }
            }

            void HandleActions()
            {
                UpdateId();
                UpdateMaxStacks();
            }

            public void UpdateId()
            {
                if (!updateId) return;
                updateId = false;

                var items = idDatabase.Items.OrderBy(item => item._id).ToList();

                for (int i = 0; i < items.Count; i++)
                {
                    items[i].SetId(i + 1, idDatabase.forceID);
                }
            }

            void UpdateMaxStacks()
            {
                if (!updateMaxStacks) return;
                updateMaxStacks = false;
                var items = idDatabase.Items;

                foreach(var item in items)
                {
                    if (item.MaxStacks == 0)
                    {
                        item.SetMaxStacks(1);
                    }
                }
            }

            public void SetDefaultFX()
            {
                if (!setDefaultEffects) return;
                setDefaultEffects = false;

                foreach (var item in idDatabase.Items)
                {
                    if (item.effects != null) continue;
                    if (HandleEquipment(item)) continue;
                    if (HandleWeapon(item)) continue;
                    if (HandleCurrency(item)) continue;

                    if (defaultEffects.defaultItemFX)
                    {
                        item.effects = defaultEffects.defaultItemFX;
                        item.usingDefaultFX = true;
                    }
                }

                bool HandleCurrency(Item item)
                {
                    if (item.GetType() != typeof(Currency)) return false;
                    if (defaultEffects.defaultCurrencyFX == null) return false;

                    item.usingDefaultFX = true;
                    item.effects = defaultEffects.defaultCurrencyFX;


                    return true;
                }

                bool HandleEquipment(Item item)
                {
                    if (!Item.IsEquipment(item)) return false;
                    if (defaultEffects.equipmentFX == null) return false;

                    var equipment = (Equipment)item;
                    foreach (var effect in defaultEffects.equipmentFX)
                    {
                        if (effect.armorType != equipment.armorType) continue;
                        if (effect.defaultFX == null) continue;

                        equipment.effects = effect.defaultFX;
                        item.usingDefaultFX = true;
                        return true;

                    }

                    return false;
                }

                bool HandleWeapon(Item item)
                {
                    if (!Item.IsWeapon(item)) return false;
                    if (defaultEffects.weaponFX == null) return false;

                    var weapon = (Weapon)item;

                    foreach (var effects in defaultEffects.weaponFX)
                    {
                        if (effects.weaponType != weapon.weaponType) continue;
                        if (effects.defaultEffects == null) continue;

                        weapon.effects = effects.defaultEffects;
                        item.usingDefaultFX = true;

                        return true;
                    }


                    return false;
                }
            }

            public void ClearDefaultEffects()
            {
                if (!clearDefaultEffects) return;
                clearDefaultEffects = false;
                foreach (var item in idDatabase.Items)
                {
                    if (!item.usingDefaultFX) continue;
                    item.usingDefaultFX = false;
                    item.effects = null;

                }
            }

            [Serializable]
            public class ItemEffects
            {
                public ItemFX defaultItemFX;
                public ItemFX defaultCurrencyFX;
                public List<EquipmentItemFX> equipmentFX;
                public List<WeaponFX> weaponFX;


                [Serializable]
                public class EquipmentItemFX
                {
                    public ArmorType armorType;
                    public ItemFX defaultFX;
                }

                [Serializable]
                public class WeaponFX
                {
                    public WeaponType weaponType;
                    public ItemFX defaultEffects;
                }
            }
        }

        [Serializable]
        public class AbilitySearch
        {
            public ArchitomeID architomeID;
            public bool search;

            public void Update()
            {
                if (!search) return;
            }
        }
        [Serializable]
        public class CatalystSearch
        {
            public ArchitomeID architomeID;
            public bool search;

            public void Update()
            {
                if (!search) return;
            }
        }
        [Serializable]
        public class EntitySearch
        {
            public ArchitomeID idDatabase;
            public bool search;
            public bool anyEntity;
            public bool nullPortrait;
            public bool character;
            public bool updateId;
            public NPCType npcType;
            public EntityRarity rarity;

            public string searchQuery;


            public List<EntityInfo> results;

            public void Update()
            {
                if (idDatabase == null) return;
                if (!search) return;

                UpdateID();

                if (!anyEntity)
                {
                    results = idDatabase.Entities.Where(entity => entity.npcType == this.npcType && entity.rarity == this.rarity).ToList();
                    
                }
                else
                {
                    results = idDatabase.Entities;
                }

                if (nullPortrait)
                {
                    results = results.Where(entity => entity.PortraitIcon() == null).ToList();
                }

                if (character)
                {
                    results = results.Where(entity => entity.GetComponentInChildren<CharacterBodyParts>() != null).ToList();
                }

                if (searchQuery.Length > 0)
                {
                    var searchString = searchQuery.ToLower();

                    results = results.Where(entity => entity.name.ToLower().Contains(searchString) || entity.entityName.ToLower().Contains(searchString)).ToList();
                }

            }

            void UpdateID()
            {
                if (!updateId) return;
                updateId = false;

                var entities = idDatabase.Entities.OrderBy(entity => entity._id).ToList();
                

                for (int i = 0; i < entities.Count; i++)
                {
                    entities[i].SetId(i + 1, idDatabase.forceID);
                }
            }
        }

        [Serializable]
        public class ArchClassSearch
        {
            public ArchitomeID idDatabase;
            public bool search;
            public bool updateId;

            public void Update()
            {
                if (idDatabase == null) return;
                if (!search) return;

                UpdateID();
            }

            void UpdateID()
            {
                if (!updateId) return;
                updateId = false;

                var classes = idDatabase.Classes.OrderBy(item => item._id).ToList();

                for (int i = 0; i < classes.Count; i++)
                {
                    classes[i].SetID(i + 1, idDatabase.forceID);
                }
            }



        }

        [Serializable]
        public class RoomSearch
        {
            public ArchitomeID idDatabase;
            public bool search;
            public bool updateId;

            public void Update()
            {
                if (idDatabase == null) return;
                if (!search) return;

                UpdateID();
            }

            void UpdateID()
            {
                if (!updateId) return;
                updateId = false;

                var rooms = idDatabase.Rooms.OrderBy(item => item._id).ToList();

                for (int i = 0; i < rooms.Count; i++)
                {
                    rooms[i].SetId(i + 1, idDatabase.forceID);
                }
            }
        }

        [Header("Database Search Interface")]
        public ItemIDSearch itemSearch;
        public AbilitySearch abilitySearch;
        public CatalystSearch catalystSearch;
        public BuffSearch buffSearch;
        public EntitySearch entitySearch;
        public ArchClassSearch classSearch;
        public RoomSearch roomSearch;

        private void OnValidate()
        {
            ResetData();
            UpdateData();
            SortData();
            CheckDuplicates();


            itemSearch.Update();
            entitySearch.Update();
            buffSearch.Update();
            classSearch.Update();
            roomSearch.Update();
        }

        void CheckDuplicates()
        {
            if (!checkDuplicates) return;
            checkDuplicates = false;

            List<int> keys = new();

            foreach (var entity in Entities)
            {
                if (keys.Contains(entity._id))
                {
                    throw new Exception($"Duplicated id of {entity._id} in entities");
                }

                keys.Add(entity._id);
            }

            keys = new();

            foreach (var buff in Buffs)
            {
                if (keys.Contains(buff._id))
                {
                    throw new Exception($"Duplicated id of {buff._id} in buffs");
                }

                keys.Add(buff._id);
            }

            keys = new();

            foreach (var item in Items)
            {
                if (keys.Contains(item._id))
                {
                    throw new Exception($"Duplicated id of {item._id} of {item}");
                }
            }

            keys = new();

            foreach (var archClass in Classes)
            {
                if (keys.Contains(archClass._id))
                {
                    throw new Exception($"Duplicated id of {archClass._id} of {archClass}");
                }

                keys.Add(archClass._id);
            }

            keys = new();

            foreach (var room in Rooms)
            {
                if (keys.Contains(room._id))
                {
                    throw new Exception($"Duplicated id of {room._id} of {room}");
                }
                keys.Add(room._id);
            }



            Debugger.InConsole(4914, $"All ids are clear");
        }

        void ResetData()
        {
            if (!resetData) return;
            resetData = false;


            Buffs.Clear();
            Abilities.Clear();
            Entities.Clear();
            Catalysts.Clear();
            Items.Clear();
            Classes.Clear();
            Rooms.Clear();
        }

        async void UpdateData()
        {
            if (!updateData) return;
            updateData = false;



            //Prefabs
            foreach (var path in prefabs)
            {
                var success = false;

                if (UpdateBuffs(path))
                {
                    success = true;
                }
                else if (UpdateAbilities(path))
                {
                    success = true;
                }
                else if (UpdateEntities(path))
                {
                    success = true;
                }
                else if (UpdateCatalysts(path))
                {
                    success = true;
                }
                else if (UpdateRooms(path))
                {
                    success = true;
                }

                if (success)
                {
                    await Task.Yield();
                }
            }
        }

        void SortData()
        {
            if (!sortData) return;
            sortData = false;

            Buffs = Buffs.OrderBy(buff => buff._id).ToList();
            Entities = Entities.OrderBy(entity => entity._id).ToList();
            Items = Items.OrderBy(item => item._id).ToList();
        }

        bool UpdateBuffs(GameObject prefab)
        {
            var buff = prefab.GetComponent<BuffInfo>();

            if (buff == null) return false;

            if (Buffs.Contains(buff))
            {
                return true;
            }


            Buffs.Add(buff);


            return true;

        }

        public bool UpdateEntities(GameObject prefab)
        {
            var entity = prefab.GetComponent<EntityInfo>();


            if (entity == null) return false;

            if (Entities.Contains(entity)) return true;

            Entities.Add(entity);

            return true;
        }

        bool UpdateAbilities(GameObject prefab)
        {
            var ability = prefab.GetComponent<AbilityInfo>();
            if (ability == null) return false;

            if (Abilities.Contains(ability)) return true;

            Abilities.Add(ability);

            return true;
        }

        bool UpdateCatalysts(GameObject prefab)
        {
            var catalyst = prefab.GetComponent<CatalystInfo>();
            if (catalyst == null) return false;

            if (Catalysts.Contains(catalyst)) return true;

            Catalysts.Add(catalyst);

            return true;

        }

        bool UpdateRooms(GameObject prefab)
        {
            var room = prefab.GetComponent<RoomInfo>();

            if (room == null) return false;

            if (Rooms.Contains(room)) return true;

            Rooms.Add(room);

            return true;


        }
    }

}
