using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using UnityEngine.EventSystems;
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
