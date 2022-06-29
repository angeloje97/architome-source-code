using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    [RequireComponent(typeof(ItemSlotHandler))]
    public class GearSlotManager : MonoBehaviour
    {
        // Start is called before the first frame update
        public EntityInfo entityInfo;
        public GearModuleManager moduleManager;
        public ModuleInfo module;
        public PartyManager partyManager;

        [Header("Gear Slot Manager Properties")]
        public Transform equipmentBin;
        public List<GearSlot> gearSlots;



        public Action<GearSlot> OnIncorrectEquipmentInsertion;



    //Update Triggers
        private EntityInfo currentEntity;



        public void GetDependencies()
        {
            module = GetComponentInParent<ModuleInfo>();
            partyManager = GetComponentInParent<PartyManager>();

            moduleManager = GetComponentInParent<GearModuleManager>();

            if (moduleManager)
            {
                moduleManager.OnEquipItem += OnEquipItem;
            }

            if (module)
            {
                module.OnSelectEntity += OnSelectEntity;
            }

            if (partyManager)
            {
                partyManager.OnSelectEntity += OnSelectEntity;
            }


            GetComponent<ItemSlotHandler>().OnChangeItem += OnChangeItem;
        }
        void Start()
        {
            GetDependencies();
        }

        public void IncorrectEquipmentType(GearSlot slot, string reason = "")
        {
            OnIncorrectEquipmentInsertion?.Invoke(slot);
        }

        void OnSelectEntity(EntityInfo entity)
        {
            if (entity == null) return;

            entityInfo = entity;
            SetGearSlots();
            DestroyItems();
            CreateItems();
        }

        void OnEquipItem(ItemInfo info, EntityInfo entity)
        {
            if (entityInfo != entity)
            {
                module.OnSelectEntity?.Invoke(entity);
            }

            var equipment = (Equipment)info.item;

            var previousSlot = info.currentSlot;

            var newSlot = Slot(equipment.equipmentSlotType);
            var currentlyEquipped = newSlot.currentItemInfo;
            //if (newSlot.currentItemInfo != null)
            //{
            //    newSlot.currentItemInfo.HandleNewSlot(currentSlot);
            //}

            info.HandleNewSlot(newSlot);

            bool success = info.currentSlot == newSlot;

            if (success && currentlyEquipped)
            {
                currentlyEquipped.HandleNewSlot(previousSlot);
            }
        }

        void SetGearSlots()
        {
            if (entityInfo == null || entityInfo.CharacterInfo() == null) { return; }

            foreach (GearSlot slot in gearSlots)
            {
                if (slot.equipmentSlot != null)
                {
                    slot.equipmentSlot.OnLoadEquipment -= OnLoadEquipment;
                }

                slot.entityInfo = entityInfo;
                slot.equipmentSlot = entityInfo.CharacterInfo().EquipmentSlot(slot.slotType);
                slot.characterInfo = entityInfo.CharacterInfo();
                slot.events.OnSetSlot?.Invoke(slot);

                slot.equipmentSlot.OnLoadEquipment += OnLoadEquipment;
            }
        }

        void OnLoadEquipment(Equipment equip)
        {
            DestroyItems();
            CreateItems();
        }

        void DestroyItems()
        {
            foreach (var itemInfo in module.GetComponentsInChildren<ItemInfo>())
            {
                Destroy(itemInfo.gameObject);
            }
        }

        public GearSlot Slot(EquipmentSlotType slotType)
        {
            foreach (var slot in gearSlots)
            {
                if (slot.slotType != slotType) continue;

                return slot;
            }

            return null;
        }

        void OnChangeItem(ItemEventData eventData)
        {
            var gearSlot = (GearSlot)eventData.itemSlot;

            gearSlot.equipmentSlot.equipment = (Equipment)gearSlot.item ? (Equipment)gearSlot.item : null;
        }


        void CreateItems()
        {
            for (int i = 0; i < gearSlots.Count; i++)
            {
                var current = gearSlots[i];

                if (current.equipmentSlot == null ||
                    current.equipmentSlot.equipment == null) { continue; }

                CreateEquipment(current.equipmentSlot.equipment, current);
            }
        }

        public GameObject CreateEquipment(Equipment equipment, GearSlot slot)
        {
            var itemTemplate = module.prefabs.item;

            if (equipmentBin != null)
            {
                var newEquipment = Instantiate(itemTemplate, equipmentBin);

                var itemInfo = newEquipment.GetComponent<ItemInfo>();

                itemInfo.item = equipment;
                itemInfo.UpdateItemInfo();
                itemInfo.isInInventory = true;
                itemInfo.HandleNewSlot(slot);

                return itemInfo.gameObject;
            }

            return null;

        }

    }

}