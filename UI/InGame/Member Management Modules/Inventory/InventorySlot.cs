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

    Transform parent;
    
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
    public Item previousItem;
    public ItemInfo previousItemInfo;


    protected void GetDependencies()
    {
        module = GetComponentInParent<ModuleInfo>();
        itemSlotHandler = GetComponentInParent<ItemSlotHandler>();
    }
    void Start()
    {
        GetDependencies();
    }
    
    public int Index()
    {
        if (parent == null)
        {
            parent = transform.parent;
            if (parent == null) return -1;
        }

        var slots = parent.GetComponentsInChildren<InventorySlot>();

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == this)
            {
                return i;
            }
        }

        return -1;
    }

    void Update()
    {
        if (module == null) return;
        if (!module.isActive) return;
        HandleEvents();
    }

    public void HandleEvents()
    {

        if (previousItemInfo != currentItemInfo)
        {
            //if (currentItemInfo)
            //{
            //    item = currentItemInfo.item;
            //}

            events.OnItemChange?.Invoke(this, previousItem, item);
            itemSlotHandler.OnChangeItem?.Invoke(new() {
                itemSlot = this,
                newItem = currentItemInfo,
                previousItem = previousItemInfo
            });

            previousItemInfo = currentItemInfo;
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
}
