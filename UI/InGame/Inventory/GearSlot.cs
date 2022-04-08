using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using UnityEngine.EventSystems;

namespace Architome
{
    public class GearSlot : InventorySlot, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        // Start is called before the first frame update
        [Header("Gear Slot Properties")]
        public CharacterInfo characterInfo;
        public EquipmentSlot equipmentSlot;
        public EquipmentSlotType slotType;

        //Update Trigger Handlers

        public GearSlotManager GearManager()
        {
            return GetComponentInParent<GearSlotManager>() ? GetComponentInParent<GearSlotManager>() : null;
        }

        public void Start()
        {
            GetDependenencies();
        }

        public void Update()
        {
            HandleEvents();
        }

        public bool CanEquip(Item item)
        {
            if (entityInfo == null) return false;
            if (!Item.IsWeapon(item) && !Item.IsEquipment(item)) return false;
            if (entityInfo == null) return false;

            var archClass = entityInfo.archClass;


            if (!IsCorrectEquipment())
            {
                return false;
            }

            if (!IsCorrectWeapon())
            {
                return false;
            }


            return true;

            bool IsCorrectEquipment()
            {
                if (archClass == null) return true;
                if (!Item.IsEquipment(item)) return true;

                var equipment = (Equipment)item;

                if (!archClass.equipableArmor.Contains(equipment.armorType))
                {
                    return false;
                }


                return true;
            }

            bool IsCorrectWeapon()
            {
                if (archClass == null) return true;
                if (!Item.IsWeapon(item)) return true;

                var weapon = (Weapon)item;

                if (!archClass.equipableWeapons.Contains(weapon.weaponType))
                {
                    return false;
                }

                return true;
            }
        }

        //public new void OnDrop(PointerEventData eventData)
        //{
        //    if (!eventData.pointerDrag ||
        //        !eventData.pointerDrag.GetComponent<ItemInfo>() ||
        //        (!Item.IsEquipment(eventData.pointerDrag.GetComponent<ItemInfo>().item) && !Item.IsWeapon(eventData.pointerDrag.GetComponent<ItemInfo>().item))) { return; }

        //    var currentItem = eventData.pointerDrag.GetComponent<ItemInfo>();
        //    var currentEquipment = (Equipment)currentItem.item;

        //    currentItem.HandleNewSlot(this);


        //    //if(currentEquipment.equipmentSlotType != slotType &&
        //    //    currentEquipment.secondarySlotType != slotType) 
        //    //{
        //    //    if(currentItem.currentSlot)
        //    //    {
        //    //        currentItem.ReturnToSlot();
        //    //    }
        //    //    return; 
        //    //}

        //    //InsertToGearSlot(currentItem);
        //}

        //public void InsertToGearSlot(ItemInfo itemInfo)
        //{
        //    var itemRect = itemInfo.GetComponent<RectTransform>();

        //    if(itemInfo.currentSlot)
        //    {
        //        var currentSlot = itemInfo.currentSlot;
        //        currentSlot.item = null;
        //        if(currentSlot.InventoryUI())
        //        {
        //            currentSlot.InventoryUI().UpdateInventory();
        //        }

        //        if(itemInfo.currentSlot.GetType() == typeof(GearSlot) && item == null)
        //        {
        //            var gearSlot = (GearSlot)itemInfo.currentSlot;
        //            gearSlot.equipmentSlot.equipment = null;
        //        }

        //    }

        //    var equipment = (Equipment)itemInfo.item;

        //    itemInfo.currentSlot = this;
        //    itemInfo.OnNewSlot?.Invoke(this);
        //    item = equipment;
        //    currentItemInfo = itemInfo;
        //    equipmentSlot.equipment = equipment;
        //    itemInfo.transform.position = transform.position;
        //    itemRect.sizeDelta = GetComponent<RectTransform>().sizeDelta;


        //}

    }

}