using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

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
        public Action<ItemInfo> OnItemAction;
        public Action<ItemInfo> OnNullHover;
        public Action UpdateActions;

        public bool active;
        public float alpha;
        private void OnValidate()
        {
            GetDependencies();
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
            if (eventData.newItem)
            {
                eventData.newItem.fxHandler = fxHandler;
            }

            OnChangeItem?.Invoke(eventData);
        }
        
    }

    public struct ItemEventData
    {
        public ItemInfo previousItem { get; set; }
        public ItemInfo newItem;
        public InventorySlot itemSlot;
    }
}