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
    public int maxStacks
    {
        get
        {
            if (item == null) return -1;
            return item.maxStacks;
        }
    }
    public int currentStacks = 1;
    
    [Header("UI Properties")]
    public InventorySlot currentSlot;
    public InventorySlot currentSlotHover;
    public ModuleInfo moduleHover;
    public Image itemIcon;
    public bool isInInventory;
    public TextMeshProUGUI amountText;

    ToolTip toolTip;

    public Action<InventorySlot> OnNewSlot { get; set; }
    public Action<ItemInfo> OnUpdate { get; set; }

    public Action<ItemInfo> OnItemAction;

    public Action<ItemInfo> OnDepleted;

    //3d World Trigger
    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<EntityInfo>() && GMHelper.GameManager().playableEntities.Contains(other.GetComponent<EntityInfo>()))
        {
            if(other.GetComponent<EntityInfo>().Inventory())
            {
                if(other.GetComponent<EntityInfo>().Inventory().PickUpItem(this))
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    
    async public void OnPointerUp(PointerEventData eventData)
    {
        ArchAction.Yield(() => ReturnToSlot());

        if (moduleHover == null && currentSlotHover == null)
        {

            var choice = await PromptHandler.active.GeneralPrompt(new()
            { 
                icon = item.itemIcon,
                title = $"{item.itemName}",
                question = $"Are you sure you want to destroy {item.itemName}", 
                option1 = "Destroy", 
                option2 = "Cancel", 
            });

            if (choice.optionString == "Destroy")
            {
                DestroySelf();
            }
        }
        
    }

    public void ReturnToSlot()
    {
        if (isInInventory == false) { return; }

        transform.position = currentSlot.transform.position;

        if (currentSlot)
        {
            transform.SetParent(currentSlot.transform);
        }

        GetComponent<RectTransform>().sizeDelta = currentSlot.GetComponent<RectTransform>().sizeDelta;
        transform.localScale = new(1, 1, 1);


        //if (currentSlot.GetComponentInParent<ModuleInfo>() && currentSlot.GetComponentInParent<ModuleInfo>().itemBin)
        //{
        //    transform.SetParent(currentSlot.transform);
            
        //    //transform.SetParent(currentSlot.GetComponentInParent<ModuleInfo>().itemBin);
        //}
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
            if (item.item._id != this.item._id) return false;
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

    public bool ReduceStacks(int amount = 1)
    {
        if (currentStacks < amount) return false;

        currentStacks -= amount;

        if (currentStacks <= 0)
        {
            OnDepleted?.Invoke(this);

            DestroySelf();
        }

        
        UpdateItemInfo();

        return true;
    }

    public void DestroySelf()
    {
        if (currentSlot)
        {
            currentSlot.currentItemInfo = null;
        }

        ArchAction.Yield(() => { Destroy(gameObject); });
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

            amountText.gameObject.SetActive(maxStacks > 1);
            //if (maxStacks == 1)
            //{ 
            //    amountText.gameObject.SetActive(false);
            //    return;
            //}
            
            amountText.text = $"{currentStacks}";
        }
    }

    public void ActivateAction()
    {
        OnItemAction?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var toolTipManager = ToolTipManager.active;

        if (toolTipManager == null) return;

        toolTip = toolTipManager.GeneralHeader();

        toolTip.adjustToMouse = true;

        toolTip.SetToolTip(item.ToolTipData());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (toolTip == null) return;

        toolTip.DestroySelf();
    }
}
