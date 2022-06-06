using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System;
using System.Threading.Tasks;

namespace Architome
{
    public class PartyManager : MonoBehaviour
    {
        public DungeoneerManager manager;

        public List<EntitySlot> partySlots;
        public List<EntityCard> rosterCards;

        public EntityInfo selectedEntity;
        

        [Serializable]
        public struct Info
        {
            public Transform rosterCardParents;
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

            if (manager)
            {
                manager.OnNewEntity += OnNewEntity;
                manager.BeforeCheckCondition += BeforeCheckCondition;
                manager.OnLoadSave += OnLoadSave;
            }

            ArchSceneManager.active.BeforeLoadScene += BeforeLoadScene;
            SaveSystem.active.BeforeSave += BeforeSave;
        }

        private void Start()
        {
            AcquireSlots(); 
        }

        void BeforeLoadScene(ArchSceneManager sceneManager)
        {
            SaveEntities();
        }

        void BeforeSave(SaveGame save)
        {
            SaveEntities();
        }

        void SaveEntities()
        {
            var currentSave = Core.currentSave;
            if (currentSave == null) return;

            foreach (var entity in manager.entities.pool)
            {
                currentSave.SaveEntity(entity);
            }

            currentSave.selectedEntitiesIndex = new();

            foreach (var slot in partySlots)
            {
                if (slot.entity == null) continue;
                currentSave.selectedEntitiesIndex.Add(slot.entity.SaveIndex);
            }
        }

        void AcquireSlots()
        {
            foreach (var slot in partySlots)
            {
                slot.OnSlotAction += OnPartySlotAction;
                slot.OnSlotSelect += OnPartySlotSelect;
            }
        }
        public void BeforeCheckCondition(List<bool> conditions)
        {
            if (slotMap == null)
            {
                conditions.Add(false);
                return;
            }

            conditions.Add(slotMap.Count == 5);
        }

        async public void OnCardAction(EntityCard card)
        {
            var slot = FirstAvailablePartySlot(card.entity.role);

            if (slot == null) return;

            var userOption = await ContextMenu.current.UserChoice(new()
            {
                title = $"{card.entity.entityName}",
                options = new()
                {
                    "Move to Party",
                    "Stats",
                    "Banish"
                }
            });

            HandleOption1();

            void HandleOption1()
            {
                if (userOption != 0) return;

                RosterToParty(slot, card);
                UpdatePartyManager();
            }

            //RosterToParty(slot, card);
            //UpdatePartyManager(true);
        }


        public void OnPartySlotSelect(EntitySlot slot)
        {

        }

        public void OnPartySlotAction(EntitySlot slot)
        {
            if (slot.entity == null) return;

            slot.entity = null;

            UpdatePartyManager(true);
        }

        public void OnSelectCard(EntityCard slotIcon)
        {
            

        }

        public void OnRightClickIcon(EntityCard slotIcon)
        {
            //var slot = slotIcon.currentSlot;

            //HandlePartySlot();
            //HandleRosterSlot();


            //void HandlePartySlot()
            //{
            //    if (slot.slotType != EntitySlotType.Party) return;
            //    slot.currentIcon = null;


            //}

            //void HandleRosterSlot()     //Create a new icon from the roster icon.
            //{
            //    if (slot.slotType != EntitySlotType.Roster) return;

            //    var newSlot = FirstAvailablePartySlot(slotIcon.entity.role);

            //    Debugger.InConsole(39052, $"{newSlot != null}");

            //    if (newSlot == null) return;

            //    RosterToParty(newSlot, slotIcon);

            //}

            //ArchAction.Yield(() => UpdatePartyManager());
        }

        public void UpdatePartyManager(bool updateDungeoneerManager = true)
        {
            UpdateDictionary();
            UpdateRoster();

            if (updateDungeoneerManager)
            {
                UpdateManager();
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
                foreach (var card in rosterCards)
                {
                    if (card.entity == null) continue;
                    if (slotMap.ContainsKey(card.entity))
                    {
                        card.SetCard(false);
                    }
                    else
                    {
                        card.SetCard(true);
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

            }

        }

        public void RosterToParty(EntitySlot slot, EntityCard card)
        {

            slot.entity = card.entity;

        }

        public void OnLoadSave(SaveGame save)
        {
            UpdatePartyManager();
        }

        public void OnNewEntity(EntityInfo entity)
        {
            var rosterTemplate = prefabs.rosterSlotTemplate;
            if (rosterTemplate == null) return;
            var parent = info.rosterCardParents;
            if (parent == null) return;
            var iconTemplate = manager.prefabs.entityCard;
            if (iconTemplate == null) return;

            var card = Instantiate(iconTemplate, parent).GetComponent<EntityCard>();
            card.SetEntity(entity);


            card.OnSelect += OnSelectCard;
            card.OnAction += OnCardAction;
            rosterCards.Add(card);

            var save = manager.currentSave;

            if (save == null) return;

            var entityIndex = entity.SaveIndex;

            if (save.selectedEntitiesIndex == null) return;
            if (!save.selectedEntitiesIndex.Contains(entityIndex)) return;

            var selectedIndex = save.selectedEntitiesIndex.IndexOf(entityIndex);

            var partySlot = partySlots[selectedIndex];

            RosterToParty(partySlot, card);


        }
        public EntitySlot FirstAvailableSlot(EntitySlotType slotType)
        {
            //var slots = slotType == EntitySlotType.Party ? partySlots : rosterSlots;

            //foreach (var slot in slots)
            //{
            //    if (slot.currentIcon == null)
            //    {
            //        return slot;
            //    }
            //}

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
