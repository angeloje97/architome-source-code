using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Architome.Events;

namespace Architome
{

    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(ItemFXHandler))]
    public class ItemSlotHandler : MonoActor
    {
        #region Common Data
        public CanvasGroup canvasGroup;
        public event Action<ItemEventData> OnChangeItem;
        public ItemFXHandler fxHandler;

        public Action<bool> OnActiveChange { get; set; }
        public Action<InventorySlot, ItemInfo, string> OnCantInsertToSlot { get; set; }

        public Action UpdateActions { get; set; }

        public List<InventorySlot> inventorySlots;

        public bool active;
        bool lockItems = false;
        public float alpha;

        #region EventHandler

        ArchEventHandler<eItemEvent, ItemEventData> itemEventHandler;

        public Action AddListener(eItemEvent trigger, Action<ItemEventData> action, MonoActor actor) => itemEventHandler.AddListener(trigger, action, actor);

        public void Invoke(eItemEvent trigger, ItemEventData eventData) => itemEventHandler.Invoke(trigger, eventData);


        public bool InvokeCheck(eItemEvent trigger, ItemEventData eventData) => itemEventHandler.InvokeCheck(trigger, eventData);

        public Action AddListenerCheck(eItemEvent trigger, Action<ItemEventData, List<bool>> action, MonoActor actor) => itemEventHandler.AddListenerCheck(trigger, action, actor);

        #endregion

        #endregion
        #region Initiation
        private void OnValidate()
        {
            GetDependencies();
        }
        protected override void Awake()
        {
            base.Awake();
            inventorySlots = new();
            itemEventHandler = new(this);
        }
        private void Start()
        {
            GetDependencies();
            HandleNullModule();
        }
        void GetDependencies()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            fxHandler = GetComponent<ItemFXHandler>();

            OnChangeItem += (ItemEventData data) => {
                if (data.newItem == null) return;
                data.newItem.fxHandler = fxHandler;
            };
        }
        #endregion

        #region Slot Handling

        public void HandleNewSlot(InventorySlot slot)
        {

            if (inventorySlots.Contains(slot)) return;

            inventorySlots.Add(slot);
        }

        public (Action<ItemInfo, InventorySlot>, Action) LockInventorySlots()
        {
            bool paused = true;


            var unlock = AddListenerCheck(eItemEvent.OnCanInsertIntoSlot, OnCanInsertIntoSlot, this);

            return (InsertItemInfo, unlock);

            void InsertItemInfo(ItemInfo itemInfo, InventorySlot slot)
            {
                paused = false;

                itemInfo.HandleNewSlot(slot);

                paused = true;
            }

            void OnCanInsertIntoSlot(ItemEventData eventData, List<bool> checks)
            {
                if (paused) 
                {
                    checks.Add(false);
                    OnCantInsertToSlot(eventData.itemSlot, eventData.newItem, "Inventory is currently in locked state. Check ItemSlotHandler::LockInventorySlots()");
                }
            }
        }
        public bool SlotHandlerActive()
        {
            return canvasGroup.interactable;
        }

        #endregion
        async void HandleNullModule()
        {
            var module = GetComponentInParent<ModuleInfo>();
            if (module != null) return;

            while (Application.isPlaying)
            {
                if (canvasGroup == null) break;
                if (alpha == canvasGroup.alpha)
                {
                    await Task.Yield();
                    continue;
                }

                alpha = canvasGroup.alpha;

                if (alpha == 0f)
                {
                    active = false;
                    OnActiveChange?.Invoke(active);
                }

                if (alpha == 1f)
                {
                    active = true;
                    OnActiveChange?.Invoke(active);
                }

                await Task.Yield();
            }
        }

        #region Item Functions

        public void ItemAction(ItemInfo item)
        {
            Invoke(eItemEvent.OnItemAction, new() { newItem = item });
        }

        public void NullHover(ItemInfo item, InventorySlot slot)
        {
            Invoke(eItemEvent.OnNullHover, new() {
                newItem = item,
                itemSlot = slot,
            });
        }

        public void InsertItemIntoSlots(ItemInfo item)
        {

        }

        public bool CanInsertIntoSlots(List<ItemInfo> items, int countOffset = 0)
        {
            var availableCounts = countOffset;

            foreach(var slot in inventorySlots)
            {
                if (slot.item == null)
                {
                    availableCounts++;
                    continue;
                }


                if (items.Exists((itemInfo) => {

                    if (itemInfo.Equals(slot.item))
                    {
                        var currentStacks = slot.currentItemInfo.currentStacks;
                        var stacksToAdd = itemInfo.currentStacks;

                        if (currentStacks + stacksToAdd <= itemInfo.item.MaxStacks)
                        {
                            return true;
                        }
                    }

                    return false;
                }))
                {
                    availableCounts++;
                }
            }

            return availableCounts >= items.Count;
        }

        #endregion

        public void HandleChangeItem(ItemEventData eventData)
        {
            OnChangeItem?.Invoke(eventData);
        }

        public void HandleCantInsert(InventorySlot slot, ItemInfo item, string reason)
        {
            var eventData = new ItemEventData(item) { itemSlot = slot };
            eventData.SetMessage(reason);
            Invoke(eItemEvent.OnCantInsert, eventData);
        }

        Action OnUnlock;

        public void SetLockItems(bool lockItems)
        {
            if (this.lockItems == lockItems) return;
            this.lockItems = lockItems;

            if (lockItems)
            {
                OnUnlock += AddListenerCheck(eItemEvent.OnCanRemoveFromSlot, HandleLockItems, this);
                
            }
            else
            {
                OnUnlock?.Invoke();
            }


        }

        void HandleLockItems(ItemEventData eventData, List<bool> checks)
        {
            checks.Add(false);
        }

        public int AvailableSlotCount => inventorySlots.Where(slot => slot.item == null).Count();
        public List<InventorySlot> AvailableSlots => inventorySlots.Where(slot => slot.currentItemInfo == null).ToList(); 

    }

    public class ItemEventData
    {
        public ItemInfo previousItem { get; set; }
        public ItemInfo newItem;

        public InventorySlot itemSlot;
        public ItemSlotHandler slotHandler => itemSlot.itemSlotHandler;

        public string message { get; private set; }

        public void SetMessage(string message)
        {
            this.message = message;
        }

        public ItemEventData()
        {

        }

        public ItemEventData(ItemInfo item)
        {
            this.newItem = item;
        }
    }

    public enum eItemEvent
    {
        OnItemAction,
        OnCanInsertIntoSlot,
        OnCanRemoveFromSlot,
        OnNullHover,
        OnCantInsert,
    }
}