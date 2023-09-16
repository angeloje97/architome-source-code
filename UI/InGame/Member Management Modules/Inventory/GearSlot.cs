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
        public ToolTip currentToolTip;


        new void GetDependencies()
        {
            base.GetDependencies();

            manager = GetComponentInParent<GearSlotManager>();

            AddListenerPredicate(InventorySlotEvent.OnCanInsertCheck, HandleCanInsert, this);
        }

        public void Start()
        {
            GetDependencies();
            
        }

        public void Update()
        {
            HandleEvents();
        }

        bool HandleCanInsert((InventorySlot, ItemInfo) data)
        {
            var info = data.Item2;
            var item = info.item;

            if (!entityInfo)
            {
                Debugger.Environment(7651, $"Entity is null for gear slot {this}");
            }

            if (entityInfo.isInCombat)
            {
                itemSlotHandler.HandleCantInsert(this, info, "Cannot equip anything during combat");
                return false;
            }

            if (!Item.Equipable(item))
            {
                //manager.IncorrectEquipmentType(this, "Not an equipable item.");
                itemSlotHandler.HandleCantInsert(this, info, "That is not an equipable item.");
                return false;
            }

            if (entityInfo == null) return false;

            var equipment = (Equipment)item;

            if (slotType != equipment.equipmentSlotType && equipment.secondarySlotType != slotType)
            {
                //manager.IncorrectEquipmentType(this, "Incorrect slot type for weapon.");
                
                itemSlotHandler.HandleCantInsert(this, info, $"{info.item.itemName} cannot be inserted into {ArchString.CamelToTitle(equipment.equipmentSlotType.ToString())} slot.");

                return false;
            }

            var archClass = entityInfo.archClass;

            if (archClass)
            {
                if (!archClass.CanEquip(item, out string reason))
                {
                    //manager.IncorrectEquipmentType(this, reason);
                    itemSlotHandler.HandleCantInsert(this, info, reason);

                    return false;
                }

            }

            return true;
        }

        public override bool CanRemoveFromSlot(ItemInfo item)
        {
            if (!base.CanRemoveFromSlot(item)) return false;

            if (entityInfo.isInCombat)
            {
                itemSlotHandler.HandleCantInsert(this, item, "Cannot remove gear during combat.");
                return false;
            }

            return true;
        }


        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (currentItemInfo != null) return;

            if (currentToolTip != null) return;

            currentToolTip = ToolTipManager.active.Label();

            currentToolTip.followMouse = true;

            currentToolTip.SetToolTip(new()
            {
                name = ArchString.CamelToTitle(slotType.ToString())
            });
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            if (currentToolTip != null)
            {
                currentToolTip.DestroySelf();
                currentToolTip = null;
            }
        }

    }

}