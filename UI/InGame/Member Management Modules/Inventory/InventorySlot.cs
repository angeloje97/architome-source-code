using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Architome;
using System;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    // Start is called before the first frame update
    [Header("Slot Properties")]
    public ItemInfo currentItemInfo;
    public Item item { get { return currentItemInfo ? currentItemInfo.item : null; } }
    public ModuleInfo module;
    public ItemSlotHandler itemSlotHandler;
    
    [Serializable]
    public struct Info
    {
        public Image slotIcon;
    }

    public struct Events
    {
        public Action<InventorySlot, Item, Item> OnItemChange { get; set; }
        public Action<InventorySlot> OnSetSlot { get; set; }
    }

    public bool interactable = true;

    public Events events;
    public Info info;

    [Header("Inventory Properties")]
    
    public Inventory inventory;

    public EntityInventoryUI InventoryUI()
    {
        return GetComponentInParent<EntityInventoryUI>() ? GetComponentInParent<EntityInventoryUI>() : null;

    }

    public Item previousItem;


    public void GetDependenencies()
    {
        module = GetComponentInParent<ModuleInfo>();
        itemSlotHandler = GetComponentInParent<ItemSlotHandler>();
    }
    void Start()
    {
        GetDependenencies();
    }
    
    void Update()
    {
        HandleEvents();
    }

    public void HandleEvents()
    {
        if (module == null) return;
        if (!module.isActive) return;
        if (previousItem != item)
        {
            events.OnItemChange?.Invoke(this, previousItem, item);
            itemSlotHandler?.OnChangeItem(new() { itemSlot = this, newItem = currentItemInfo }); 
            previousItem = item;
        }
    }

    //Event Handlers
    public void OnDrop(PointerEventData eventData)
    {
        if (!interactable) return;
        if (eventData.pointerDrag == null) { return; }
        if (!eventData.pointerDrag.GetComponent<ItemInfo>()) { return; }

        var droppedItem = eventData.pointerDrag.GetComponent<ItemInfo>();

        droppedItem.HandleNewSlot(this);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) { return; }
        if (!eventData.pointerDrag.GetComponent<ItemInfo>()) { return; }

        var draggingItem = eventData.pointerDrag.GetComponent<ItemInfo>();

        draggingItem.currentSlotHover = this;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) { return; }
        if (!eventData.pointerDrag.GetComponent<ItemInfo>()) { return; }

        var draggingItem = eventData.pointerDrag.GetComponent<ItemInfo>();

        draggingItem.currentSlotHover = null;
    }
    public int SlotNum()
    {
        if(InventoryUI())
        {
            return InventoryUI().inventorySlots.IndexOf(this);
        }
        return -1;
    }
}
