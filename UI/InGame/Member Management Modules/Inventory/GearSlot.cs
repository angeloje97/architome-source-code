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
        public EntityInfo entityInfo;
        public CharacterInfo characterInfo;
        public EquipmentSlot equipmentSlot;
        public EquipmentSlotType slotType;
        GearSlotManager manager;
        //Update Trigger Handlers



        new void GetDependencies()
        {
            base.GetDependencies();

            manager = GetComponentInParent<GearSlotManager>();
        }

        public void Start()
        {
            GetDependencies();
            
        }

        public void Update()
        {
            HandleEvents();
        }

        public bool CanEquip(Item item)
        {
            if (!Item.IsWeapon(item) && !Item.IsEquipment(item)) return false;
            if (entityInfo == null) return false;

            var equipment = (Equipment)item;

            if (slotType != equipment.equipmentSlotType && equipment.secondarySlotType != slotType) return false;

            var archClass = entityInfo.archClass;


            if (!IsCorrectEquipment())
            {
                manager.IncorrectEquipmentType(this);
                return false;
                
            }

            if (!IsCorrectWeapon())
            {
                manager.IncorrectEquipmentType(this);
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

    }

}