using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using Architome;
public class ItemInfo : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    // Start is called before the first frame update
    public Item item;
    public int maxStacks;
    public int currentStacks = 1;
    
    [Header("UI Properties")]
    public InventorySlot currentSlot;
    public InventorySlot currentSlotHover;
    public ModuleInfo moduleHover;
    public Image itemIcon;
    public bool isInInventory;
    public TextMeshProUGUI amountText;

    public Action<InventorySlot> OnNewSlot { get; set; }
    public Action<ItemInfo> OnUpdate { get; set; }

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

    async public void OnPointerUp(PointerEventData eventData)
    {
        ArchAction.Yield(() => ReturnToSlot());

        if (moduleHover == null)
        {

            var choice = await PromptHandler.active.GeneralPrompt(new()
            { 
                question = $"{item.itemName}", 
                option1 = "Destroy", 
                option2 = "Cancel", 
                icon = item.itemIcon
            });

            if (choice == 0)
            {
                Destroy(gameObject);
            }
        }
        
    }

    public void ReturnToSlot()
    {
        if (isInInventory == false) { return; }

        transform.position = currentSlot.transform.position;

        GetComponent<RectTransform>().sizeDelta = currentSlot.GetComponent<RectTransform>().sizeDelta;
        transform.localScale = new(1, 1, 1);

        if (currentSlot.GetComponentInParent<ModuleInfo>() && currentSlot.GetComponentInParent<ModuleInfo>().itemBin)
        {
            transform.SetParent(currentSlot.transform);
            
            //transform.SetParent(currentSlot.GetComponentInParent<ModuleInfo>().itemBin);
        }
    }

    public void HandleNewSlot(InventorySlot slot)
    {
        var previousSlot = currentSlot;
        var changedSlot = false;

        if (slot == currentSlot) return;

        HandleInventorySlot();
        //HandleGearSlot();
        HandlePreviousSlot(changedSlot);
        ReturnToSlot();

        void HandleGearSlot()
        {
            if(slot.GetType() != typeof(GearSlot)) { return; }
            


            var gearSlot = (GearSlot)slot;

            if (!gearSlot.CanEquip(item))
            {
                return;
            }

            var equipment = (Equipment)item;

            if(equipment.equipmentSlotType != gearSlot.slotType  && equipment.secondarySlotType != gearSlot.slotType) { return; }


            //gearSlot.item = item;
            //gearSlot.equipmentSlot.equipment = equipment;
            gearSlot.currentItemInfo = this;
            currentSlot = gearSlot;
            changedSlot = true;
        }

        void HandleInventorySlot()
        {
            if (slot.GetType() == typeof(GearSlot))
            {
                var gearSlot = (GearSlot)slot;

                if (gearSlot.CanEquip(item))
                {
                    slot.currentItemInfo = this;
                    currentSlot = slot;
                    changedSlot = true;
                }

                return;
            }
            slot.currentItemInfo = this;
            currentSlot = slot;
            changedSlot = true;
        }

        void HandlePreviousSlot(bool val)
        {
            if (!val) { return; }
            if(previousSlot == null) { return; }

            previousSlot.currentItemInfo = null;
        }
    }
    public void HandleItem(ItemInfo item)
    {
        if (SameItem())
        {

        }
        else
        {

        }

        bool SameItem()
        {
            if (item.item.itemID != this.item.itemID) return false;
            return true;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        var dragging = eventData.pointerDrag;
        if (dragging == null) return;
        var itemInfo = dragging.GetComponent<ItemInfo>();
        if (itemInfo == null) return;

        HandleItem(itemInfo);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(isInInventory == false) { return; }

        var module = GetComponentInParent<ModuleInfo>();

        if (module && module.itemBin)
        {
            transform.SetParent(module.itemBin);
        }
        currentSlotHover = currentSlot;
        moduleHover = module;
    }

    void OnValidate()
    {
        if (item == null) { return; }

        UpdateItemInfo();
    }

    public void UpdateItemInfo()
    {
        if(item == null) { return; }

        currentStacks = Mathf.Clamp(currentStacks, 1, maxStacks);

        UpdateStackText();
        UpdateItemIcon();

        OnUpdate?.Invoke(this);

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

    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }
}
