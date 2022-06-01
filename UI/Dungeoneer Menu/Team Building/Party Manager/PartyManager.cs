using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System;

namespace Architome
{
    public class PartyManager : MonoBehaviour
    {
        public DungeoneerManager manager;

        public List<EntitySlot> partySlots;
        public List<EntitySlot> rosterSlots;

        public EntityInfo selectedEntity;
        

        [Serializable]
        public struct Info
        {
            public Transform rosterSlotParent;
            public Transform partySlotParent;
        }

        [Serializable]
        public struct Prefabs
        {
            public GameObject rosterSlotTemplate;
            public GameObject partySlotTemplate;
        }

        public Info info;
        public Prefabs prefabs;

        public Dictionary<EntityInfo, EntitySlot> slotMap;

        public Action<EntityInfo> OnSelectEntity;

        private void Awake()
        {
            manager = GetComponentInParent<DungeoneerManager>();

            manager.OnNewEntity += OnNewEntity;
            manager.BeforeCheckCondition += BeforeCheckCondition;
        }

        public void BeforeCheckCondition(List<bool> conditions)
        {
            if (slotMap == null)
            {
                conditions.Add(false);
                return;
            }

            conditions.Add(slotMap.Count == 5f);

        }

        public void OnRightClickIcon(EntitySlotIcon slotIcon)
        {
            var slot = slotIcon.currentSlot;

            HandlePartySlot();
            HandleRosterSlot();


            void HandlePartySlot()
            {
                if (slot.slotType != EntitySlotType.Party) return;
                slot.currentIcon = null;

            }

            void HandleRosterSlot()     //Create a new icon from the roster icon.
            {
                if (slot.slotType != EntitySlotType.Roster) return;

                var newSlot = FirstAvailablePartySlot(slotIcon.entity.role);

                Debugger.InConsole(39052, $"{newSlot != null}");

                if (newSlot == null) return;

                var newIcon = Instantiate(slotIcon, transform);

                newIcon.SetIcon(slotIcon.entity, newSlot);

                newIcon.OnRightClick += OnRightClickIcon;
                newIcon.OnLeftClick += OnLeftClickIcon;
            }

            ArchAction.Yield(() => {
                UpdateDictionary();
                UpdateRoster();
                UpdateManager();
            });
        }

        public void OnLeftClickIcon(EntitySlotIcon slotIcon)
        {
            if (slotIcon.entity == null) return;

            selectedEntity = slotIcon.entity;

            OnSelectEntity?.Invoke(selectedEntity);
        }



        void UpdateDictionary()
        {
            slotMap = new();

            foreach (var slot in partySlots)
            {
                if (slot.entity == null) continue;
                slotMap.Add(slot.entity, slot);
            }
        }

        void UpdateRoster()
        {
            foreach (var slot in rosterSlots)
            {
                if (slot.entity == null) continue;
                if (slotMap.ContainsKey(slot.entity))
                {
                    slot.currentIcon.SetAvailable(false);
                }
                else
                {
                    slot.currentIcon.SetAvailable(true);
                }
            }
        }

        void UpdateManager()
        {
            if (manager == null) return;

            var newList = new List<EntityInfo>();

            foreach (var keyValuePair in slotMap)
            {
                var entity = keyValuePair.Key;

                newList.Add(entity);
            }

            manager.SetSelectedEntities(newList);
            manager.CheckCondition();

        }

        

        public void OnNewEntity(EntityInfo entity)
        {
            var rosterTemplate = prefabs.rosterSlotTemplate;
            if (rosterTemplate == null) return;
            var parent = info.rosterSlotParent;
            if (parent == null) return;
            var iconTemplate = manager.prefabs.entityIcon;
            if (iconTemplate == null) return;

            var itemSlot = Instantiate(rosterTemplate, parent).GetComponent<EntitySlot>();
            var icon = Instantiate(iconTemplate, itemSlot.transform).GetComponent<EntitySlotIcon>();


            icon.OnRightClick += OnRightClickIcon;
            icon.OnLeftClick += OnLeftClickIcon;
            icon.info.dragAndDropScope = transform;
            icon.SetIcon(entity, itemSlot);

            rosterSlots.Add(itemSlot);
        }

        public EntitySlot FirstAvailableSlot(EntitySlotType slotType)
        {
            var slots = slotType == EntitySlotType.Party ? partySlots : rosterSlots;

            foreach (var slot in slots)
            {
                if (slot.currentIcon == null)
                {
                    return slot;
                }
            }

            return null;
        }

        public EntitySlot FirstAvailablePartySlot(Role role)
        {
            foreach (var slot in partySlots)
            {
                if (slot.entity != null) continue;
                if (slot.slotRole != role) continue;

                return slot;
                
            }

            return null;
        }

    }
}
