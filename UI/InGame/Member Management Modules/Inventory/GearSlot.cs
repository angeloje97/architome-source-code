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
            if (!Item.Equipable(item))
            {
                manager.IncorrectEquipmentType(this, "Not an equipable item.");
                return false;
            }

            if (entityInfo == null) return false;

            var equipment = (Equipment)item;

            if (slotType != equipment.equipmentSlotType && equipment.secondarySlotType != slotType)
            {
                manager.IncorrectEquipmentType(this, "Incorrect slot type for weapon.");
                return false;
            }

            var archClass = entityInfo.archClass;

            if (archClass)
            {
                if (!archClass.CanEquip(item, out string reason))
                {
                    manager.IncorrectEquipmentType(this, reason);

                    return false;
                }

            }

            return true;
        }

    }

}