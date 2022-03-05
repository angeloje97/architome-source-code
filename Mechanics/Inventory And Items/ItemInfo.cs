using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using Architome;
public class ItemInfo : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    // Start is called before the first frame update
    public Item item;
    public int maxStacks;
    public int currentStacks = 1;
    
    [Header("UI Properties")]
    public InventorySlot currentSlot;
    public InventorySlot currentSlotHover;
    public Image itemIcon;
    public bool isInInventory;
    public TextMeshProUGUI amountText;

    public Action<InventorySlot> OnNewSlot;

    //3d World Trigger
    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<EntityInfo>() && GMHelper.GameManager().playableEntities.Contains(other.GetComponent<EntityInfo>()))
        {
            if(other.GetComponent<EntityInfo>().Inventory())
            {
                if(other.GetComponent<EntityInfo>().Inventory().PickUpItem(item))
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //if(currentSlot && currentSlotHover == null)
        //{
        //    ReturnToSlot();
        //}
        ReturnToSlot();
    }

    public void ReturnToSlot()
    {
        if (isInInventory == false) { return; }

        transform.position = currentSlot.transform.position;
        GetComponent<RectTransform>().sizeDelta = currentSlot.GetComponent<RectTransform>().sizeDelta;
        if (currentSlot.GetComponentInParent<ModuleInfo>() && currentSlot.GetComponentInParent<ModuleInfo>().itemBin)
        {
            transform.SetParent(currentSlot.GetComponentInParent<ModuleInfo>().itemBin);
        }
    }

    public void HandleNewSlot(InventorySlot slot)
    {
        var previousSlot = currentSlot;
        var changedSlot = false;

        HandleInventorySlot();
        HandleGearSlot();
        HandlePreviousSlot(changedSlot);
        ReturnToSlot();

        void HandleGearSlot()
        {
            if(slot.GetType() != typeof(GearSlot)) { return; }
            if (Item.IsEquipment(item) && Item.IsWeapon(item)) { return; }


            var gearSlot = (GearSlot)slot;

            var equipment = (Equipment)item;

            if(equipment.equipmentSlotType != gearSlot.slotType  && equipment.secondarySlotType != gearSlot.slotType) { return; }


            gearSlot.item = item;
            gearSlot.equipmentSlot.equipment = equipment;
            gearSlot.currentItemInfo = this;
            currentSlot = gearSlot;
            changedSlot = true;
        }

        void HandleInventorySlot()
        {
            if(slot.GetType() != typeof(InventorySlot)) {
                return; }

            slot.item = item;
            slot.currentItemInfo = this;
            currentSlot = slot;
            currentSlot.InventoryUI().UpdateInventory();
            changedSlot = true;
        }

        void HandlePreviousSlot(bool val)
        {
            if (!val) { return; }
            if(previousSlot == null) { return; }

            if(previousSlot.GetType() == typeof(GearSlot))
            {
                var gearSlot = (GearSlot)previousSlot;

                gearSlot.currentItemInfo = null;
                gearSlot.item = null;
                gearSlot.equipmentSlot.equipment = null;
            }
            
            if(previousSlot.GetType() == typeof(InventorySlot))
            {
                previousSlot.item = null;
                previousSlot.currentItemInfo = null;
                previousSlot.InventoryUI().UpdateInventory();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(isInInventory == false) { return; }

        currentSlotHover = null;
    }

    //UI Triggers

    void OnValidate()
    {
        if (item == null) { return; }

        UpdateItemInfo();
    }

    public void UpdateItemInfo()
    {
        if(item == null) { return; }
        

        UpdateStackText();
        UpdateItemIcon();

        void UpdateItemIcon()
        {
            if(itemIcon == null) { return; }
            if(item && item.itemIcon == null) { return; }

            itemIcon.sprite = item.itemIcon;
        }

        void UpdateStackText()
        {
            if(amountText == null) { return; }
            maxStacks = item.maxStacks;
            if (maxStacks == 1)
            { 
                amountText.gameObject.SetActive(false);
                return;
            }
            
            amountText.text = $"{currentStacks}";
        }
    }
}
