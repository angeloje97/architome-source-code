using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{

    [RequireComponent(typeof(CanvasGroup))]
    public class ItemSlotHandler : MonoBehaviour
    {
        // Start is called before the first frame update
        public CanvasGroup canvasGroup;
        public Action<ItemEventData> OnChangeItem { get; set; }
        public Action<bool> OnActiveChange;

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

        async void HandleNullModule()
        {
            var module = GetComponentInParent<ModuleInfo>();
            if (module != null) return;


            while (Application.isPlaying)
            {
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

        void GetDependencies()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }
        public bool SlotHandlerActive()
        {
            return canvasGroup.interactable;
        }
        
    }

    public struct ItemEventData
    {
        public ItemInfo previousItem { get; set; }
        public ItemInfo newItem;
        public InventorySlot itemSlot;
    }
}