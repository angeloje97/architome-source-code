using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System;

namespace Architome
{
    public class EntitySlotManager : MonoBehaviour
    {
        public DungeoneerManager manager;

        public List<EntitySlot> partySlots;
        public List<EntitySlot> rosterSlots;



        [Serializable]
        public struct Info
        {
            public Transform rosterSlotParent;
            public Transform partySlotParent;
        }
        public Info info;

        [Serializable]
        public struct Prefabs
        {
            public GameObject rosterSlotTemplate;
            public GameObject partySlotTemplate;
        }

        public Prefabs prefabs;

        public Dictionary<EntityInfo, EntitySlot> slotMap;

        private void Awake()
        {
            manager = GetComponentInParent<DungeoneerManager>();

            manager.OnNewEntity += OnNewEntity;
        }
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
        
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

            void HandleRosterSlot()
            {
                if (slot.slotType != EntitySlotType.Roster) return;

                var newSlot = FirstAvailablePartySlot(slotIcon.entity.role);

                Debugger.InConsole(39052, $"{newSlot != null}");

                if (newSlot == null) return;

                var newIcon = Instantiate(slotIcon, transform);

                newIcon.SetIcon(slotIcon.entity, newSlot);

                newIcon.OnRightClick += OnRightClickIcon;
            }

            ArchAction.Yield(() => {
                UpdateDictionary();
                UpdateRoster();
            });
            
        }

        public void OnNewIcon(EntitySlotIcon slotIcon, EntitySlot slot)
        {
            if (slot.slotType != EntitySlotType.Party) return;

            slotIcon.OnRightClick += OnRightClickIcon;


            

            UpdateDictionary();
            UpdateRoster();
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


            icon.OnNewIcon += OnNewIcon;
            icon.OnRightClick += OnRightClickIcon;
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
