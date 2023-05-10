using Architome.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    
    public struct InfoEvents
    {
        EntityInfo entity;
        public void Initiate(EntityInfo entity)
        {
            this.entity = entity;
        }

        public enum EventType
        {
            IsMaleCheck,
            IsFemaleCheck,
        }

        public Dictionary<EventType, Action<EntityInfo, object, List<bool>>> flagCheck;


        public Action<List<string>> OnUpdateObjectives;
        public Action<Quest> OnQuestComplete;
        public Action<Inventory.LootEventData> OnLootItem { get; set; }

        public Action<EntityInfo, Consumable> OnConsumeStart, OnConsumeEnd, OnConsumeComplete;

        public Action<Inventory.LootEventData, List<bool>> OnLootItemCheck { get; set; }
        public Action<Inventory.LootEventData> OnLootItemFromWorld { get; set; }
        public Action<ItemInfo, EntityInfo> OnTryEquip { get; set; }

        public Action<EntityInfo, bool, GameObject> OnMouseHover { get; set; }
        public Action<EntityInfo, List<bool>> OnIsPlayerCheck { get; set; }
        public Action<Vector3> OnSignificantMovementChange { get; set; }
        public Action<EntityRarity, EntityRarity> OnRarityChange { get; set; }
        public Action<EntityInfo> OnNullPortraitCheck;
        public Action<Currency, int, List<bool>> OnCanSpendCheck { get; set; }
        public Action<ItemData, List<bool>> OnCanPickUpCheck { get; set; }
        public Action<ItemData, List<bool>> OnCanDropCheck { get; set; }
        public Action<EntityInfo> OnDestroy { get; set; }

        public Action<EntityInfo, Transform> OnCanSeeCheck;
        public Action<EntityInfo, List<bool>> OnMaleCheck { get; set; }
        public Action<EntityInfo, List<bool>> OnFemaleCheck { get; set; }

        public Action<EntityInfo, List<bool>> OnCanMoveCheck { get; set; }

        public bool IsOneTrue(EventType type, object obj)
        {
            flagCheck ??= new();
            if (!flagCheck.ContainsKey(type)) return false;
            var flag = flagCheck[type];
            var checks = new List<bool>();

            flag?.Invoke(entity, obj, checks);

            foreach(var check in checks)
            {
                if (check) return true;
            }

            return false;
        }

        public void AddOneTrue(EventType type, Action<EntityInfo, object, List<bool>> action)
        {
            flagCheck ??= new();

            if (!flagCheck.ContainsKey(type))
            {
                flagCheck.Add(type, action);
            }
            else
            {
                flagCheck[type] += action;
            }
            
        }

    }
}
