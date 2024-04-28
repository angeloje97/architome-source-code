using Architome.Enums;
using Architome.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    #region ScenEvents
    public class EntitySceneEventData
    {
        public string sceneName;
    }

    public enum eEntitySceneTrigger
    {
        OnTransferScene,
    }
    #endregion

    public struct InfoEvents
    {
        EntityInfo entity;
        #region Initiation
        public void Initiate(EntityInfo entity)
        {
            this.entity = entity;
            movementEvents = new(entity);
            sceneEvents = new(entity);
            this.socialEvents = new(entity);
            portalEvents = new(entity);
        }
        #endregion

        public enum EventType
        {
            IsMaleCheck,
            IsFemaleCheck,
        }

        #region Movement Events
        public ArchEventHandler<eMovementEvent, MovementEventData> movementEvents { get; set; }

        #endregion

        #region Social Events

        ArchEventHandler<eSocialEvent, SocialEventData> socialEvents;

        public void InvokeSocial(eSocialEvent trigger, SocialEventData data) => socialEvents.Invoke(trigger, data);

        public Action AddListenerSocial(eSocialEvent trigger, Action<SocialEventData> action, MonoActor listener) => socialEvents.AddListener(trigger, action, listener);


        #endregion

        #region SceneEvents

        ArchEventHandler<eEntitySceneTrigger, EntitySceneEventData> sceneEvents { get; set; }

        public void InvokeScene(eEntitySceneTrigger trigger, EntitySceneEventData eventData) => sceneEvents.Invoke(trigger, eventData);

        public Action AddListenerScene(eEntitySceneTrigger trigger, Action<EntitySceneEventData> action, MonoActor listener) => sceneEvents.AddListener(trigger, action, listener);

        #endregion

        #region Portal Events

        ArchEventHandler<ePortalEvent, PortalEventData> portalEvents;

        public void InvokePortal(ePortalEvent trigger, PortalEventData eventData) => portalEvents.Invoke(trigger, eventData);

        public void AddListenerPortal(ePortalEvent trigger, Action<PortalEventData> action, MonoActor listener) => portalEvents.AddListener(trigger, action, listener);

        #endregion

        

        public Dictionary<EventType, Action<EntityInfo, object, List<bool>>> flagCheck;


        public Action<List<string>> OnUpdateObjectives;
        public Action<Quest> OnQuestComplete;

        #region Loot Events

        public Action<Inventory.LootEventData> OnLootItem { get; set; }

        public Action<Inventory.LootEventData> OnDropItem { get; set; }

        public Action<EntityInfo, Consumable> OnConsumeStart, OnConsumeEnd, OnConsumeComplete;

        public Action<Inventory.LootEventData, List<bool>> OnLootItemCheck { get; set; }
        public Action<Inventory.LootEventData> OnLootItemFromWorld { get; set; }
        public Action<ItemInfo, EntityInfo> OnTryEquip { get; set; }

        #endregion
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
