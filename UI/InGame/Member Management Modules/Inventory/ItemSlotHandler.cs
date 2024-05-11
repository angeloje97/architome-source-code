using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Architome
{

    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(ItemFXHandler))]
    public class ItemSlotHandler : MonoBehaviour
    {
        // Start is called before the first frame update
        public CanvasGroup canvasGroup;
        public event Action<ItemEventData> OnChangeItem;
        public ItemFXHandler fxHandler;
        public Action<bool> OnActiveChange;
        public Action<ItemInfo> OnItemAction { get; set; }
        public Action<ItemInfo> OnNullHover;
        public Action<InventorySlot, ItemInfo, string> OnCantInsertToSlot { get; set; }
        public Action<InventorySlot, ItemInfo, List<bool>> OnCanInsertToSlotCheck { get; set; }
        public Action<InventorySlot, ItemInfo, List<bool>> OnCanRemoveFromSlotCheck { get; set; }
        public Action UpdateActions;

        public List<InventorySlot> inventorySlots;

        public bool active;
        bool lockItems = false;
        public float alpha;
        private void OnValidate()
        {
            GetDependencies();
        }

        private void Awake()
        {
            inventorySlots = new();
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

            //foreach (Transform child in transform)
            //{
            //    var slot = child.GetComponent<InventorySlot>();

            //    if (slot == null) continue;

            //    inventorySlots.Add(slot);
            //}
        }

        public void HandleNewSlot(InventorySlot slot)
        {

            if (inventorySlots.Contains(slot)) return;

            inventorySlots.Add(slot);
        }

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

        public void ItemAction(ItemInfo item)
        {
            OnItemAction?.Invoke(item);
        }

        public void NullHover(ItemInfo item)
        {
            OnNullHover?.Invoke(item);
        }

        public bool SlotHandlerActive()
        {
            return canvasGroup.interactable;
        }

        public void HandleChangeItem(ItemEventData eventData)
        {
            OnChangeItem?.Invoke(eventData);
        }

        public void HandleCantInsert(InventorySlot slot, ItemInfo item, string reason)
        {
            OnCantInsertToSlot?.Invoke(slot, item, reason);
        }

        public void SetLockItems(bool lockItems)
        {
            if (this.lockItems == lockItems) return;
            this.lockItems = lockItems;

            if (lockItems)
            {
                OnCanRemoveFromSlotCheck += HandleLockItems;
            }
            else
            {
                OnCanRemoveFromSlotCheck -= HandleLockItems;
            }


        }

        void HandleLockItems(InventorySlot slot, ItemInfo item, List<bool> checks)
        {
            checks.Add(false);
        }

        public int AvailableSlotCount => inventorySlots.Where(slot => slot.item == null).Count();
        public List<InventorySlot> AvailableSlots => inventorySlots.Where(slot => slot.currentItemInfo == null).ToList(); 

    }

    public struct ItemEventData
    {
        public ItemInfo previousItem { get; set; }
        public ItemInfo newItem;
        public InventorySlot itemSlot;
    }
}