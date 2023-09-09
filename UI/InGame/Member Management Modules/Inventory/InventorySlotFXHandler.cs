using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Architome
{
    public class InventorySlotFXHandler : MonoBehaviour
    {
        [Serializable]
        public struct FXSettings
        {
            public float hoverSizeAdditive;
            public Color idleShade;
            public Color hoverShade;
            public Color availableColor;
            public Color unavailableColor;
        }

        public FXSettings settings;

        public InventorySlot slot;
        public Image targetImage;
        public RectTransform rectTransform;

        public TaskQueueHandler taskHandler;

        void Start()
        {
            GetDependencies();
        }

        void GetDependencies()
        {
            taskHandler = new(TaskType.Sequential);
            slot = GetComponent<InventorySlot>();
            rectTransform = GetComponent<RectTransform>();
            if (slot == null) return;
            UpdateSlotBackground(false, 0f);

            slot.events.OnHoverWithItem += HandleHoverWithItem;
        }

        Task UpdateSlotBackground(bool isHovering, float transitionTime)
        {
            if (targetImage == null) return Task.Run(() => { });
            var startColor = targetImage.color;

            return ArchCurve.Smooth((float lerpValue) => {
                var targetColor = isHovering ? settings.hoverShade : settings.idleShade;

                targetImage.color = Color.Lerp(startColor, targetColor, lerpValue);
            }, CurveType.EaseIn, transitionTime);
        }




        void HandleHoverWithItem(InventorySlot slot, ItemInfo item, bool enter)
        {
            if (!enter) return;
            if (!slot.CanInsert(item)) return;

            var originalSize = rectTransform.sizeDelta;
            var targetSize = originalSize + (originalSize * settings.hoverSizeAdditive);

            Debugger.UI(5491, $"Hovering over slot {slot} with item {item}");

            taskHandler.AddTask(HandleEnter);
            taskHandler.AddTask(slot.FinishHovering);
            taskHandler.AddTask(HandleExit);


            async Task HandleEnter()
            {
                Debugger.UI(5492, $"Handling Enter");
                var tasks = new List<Task>()
                {
                    UpdateSlotBackground(true, .25f),
                    ArchCurve.Smooth((float lerpValue) => {
                        rectTransform.sizeDelta = Vector2.Lerp(originalSize, targetSize, lerpValue);
                    }, CurveType.EaseIn, .25f),
                };
                Debugger.UI(5493, $"Handling Enter Finished");

                await Task.WhenAll(tasks);

            }

            async Task HandleExit()
            {

                Debugger.UI(5493, $"Handling Exit");
                var tasks = new List<Task>() {
                    UpdateSlotBackground(false, .25f),
                    ArchCurve.Smooth((float lerpValue) => {
                        rectTransform.sizeDelta = Vector2.Lerp(targetSize, originalSize, lerpValue);
                    }, CurveType.EaseIn, .25f),
                    
                };
                await Task.WhenAll(tasks);
                Debugger.UI(5493, $"Handling Exit Finished");

            }
        }

        void Update()
        {
        
        }



        // Update is called once per frame


    }
}
