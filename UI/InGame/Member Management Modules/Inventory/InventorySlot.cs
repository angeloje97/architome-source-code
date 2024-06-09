using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.Experimental.AI;
using Architome.Events;

namespace Architome
{
    public enum InventorySlotEvent
    {
        OnTakeItem,
        OnDropItem,
        OnHoverWithItem,
        OnCanInsertCheck,
        OnCanRemoveCheck,
        OnSetSlot,
    }
    public class InventorySlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
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
        }

        public bool interactable = true;

        public Events events;
        ArchEventHandler<InventorySlotEvent, (InventorySlot, ItemInfo)> eventHandler;
        public Info info;

        [Header("Inventory Properties")]
        public Item previousItem;
        public ItemInfo previousItemInfo;

        public bool isHovering { get; private set; }


        #region Event Handler Invokes
        #endregion


        protected virtual void GetDependencies()
        {
            try
            {
                module = GetComponentInParent<ModuleInfo>();
                itemSlotHandler = GetComponentInParent<ItemSlotHandler>();

                itemSlotHandler.HandleNewSlot(this);

            }
            catch (Exception e)
            {
                Defect.CreateIndicator(transform, "InventorySlot Defect in InventorySlot::GetDependencies()" , e);
            }
        }
        void Start()
        {
            GetDependencies();
        }


        void OnDestroy()
        {
        }
        void Update()
        {

            HandleEvents();
        }

        void Awake()
        {
            eventHandler = new(this);
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

        public void HandleEvents()
        {
            if (previousItemInfo != currentItemInfo)
            {

                events.OnItemChange?.Invoke(this, previousItem, item);
                itemSlotHandler.HandleChangeItem(new()
                {
                    itemSlot = this,
                    newItem = currentItemInfo,
                    previousItem = previousItemInfo
                });

                if (currentItemInfo)
                {
                    InvokeEvent(InventorySlotEvent.OnDropItem, (this, currentItemInfo));
                }

                previousItemInfo = currentItemInfo;
                previousItem = item;

            }
        }


        //Event Handlers
        public virtual void OnDrop(PointerEventData eventData)
        {
            if (!interactable) return;
            if (eventData.pointerDrag == null) { return; }
            if (!eventData.pointerDrag.GetComponent<ItemInfo>()) { return; }

            var droppedItem = eventData.pointerDrag.GetComponent<ItemInfo>();

            droppedItem.HandleNewSlot(this);
        }
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null) { return; }
            if (!eventData.pointerDrag.GetComponent<ItemInfo>()) { return; }

            isHovering = true;

            var draggingItem = eventData.pointerDrag.GetComponent<ItemInfo>();
            InvokeEvent(InventorySlotEvent.OnHoverWithItem, (this, draggingItem));

            draggingItem.currentSlotHover = this;
        }
        public virtual void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;
            if (eventData.pointerDrag == null) { return; }
            if (!eventData.pointerDrag.GetComponent<ItemInfo>()) { return; }

            var draggingItem = eventData.pointerDrag.GetComponent<ItemInfo>();

            draggingItem.currentSlotHover = null;
        }

        public async Task FinishHovering()
        {
            while (true)
            {
                if (this == null) break;
                if (!isHovering) break;
                await Task.Yield();
            }
        }

        public virtual bool CanInsert(ItemInfo item)
        {
            if (itemSlotHandler)
            {
                var passedItemSlotHandler = itemSlotHandler.InvokeCheck(eItemEvent.OnCanInsertIntoSlot, new() { itemSlot = this, newItem = item });
                if (!passedItemSlotHandler) return false;
            }

            return eventHandler.InvokeCheck(InventorySlotEvent.OnCanInsertCheck, (this, item));
            
        }

        public virtual bool CanRemoveFromSlot(ItemInfo item)
        {

            var selfSuccess = true;
            
            var slotHandlerSuccess  =  itemSlotHandler != null && itemSlotHandler.InvokeCheck(eItemEvent.OnCanRemoveFromSlot, new() { 
                itemSlot = this,
                newItem = item,
            });

            return selfSuccess && slotHandlerSuccess;
        }

        public Action AddListener(InventorySlotEvent trigger, Action<(InventorySlot, ItemInfo)> action, Component listener)
        {
            eventHandler ??= new(this);

            return eventHandler.AddListener(trigger, action, listener);
        }

        public void InvokeEvent(InventorySlotEvent trigger, (InventorySlot, ItemInfo) data)
        {
            eventHandler ??= new(this);
            eventHandler.Invoke(trigger, data);
        }

        public Action AddListenerCheck(InventorySlotEvent trigger, Action<(InventorySlot, ItemInfo), List<bool>> action, Component listener)
        {
            eventHandler ??= new(this);
            return eventHandler.AddListenerCheck(trigger, action, listener);
        }


        public Action AddListenerPredicate(InventorySlotEvent trigger, Predicate<(InventorySlot, ItemInfo)> data, Component component)
        {
            eventHandler ??= new(this);
            return eventHandler.AddListenerPredicate(trigger, data, component);
        }
    }
}
