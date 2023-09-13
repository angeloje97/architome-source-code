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

            public AudioClip mouseOverClip;
            public AudioClip dropItemClip;
            public AudioClip grabItemClip;

        }

        public FXSettings settings;

        public InventorySlot slot;
        public Image targetImage;
        public RectTransform rectTransform;
        public TaskQueueHandler taskHandler;
        public AudioManager audioManager;

        Color defaultColor;

        bool hoveringEffect;

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
            defaultColor = settings.idleShade;
            UpdateSlotBackground(false, 0f);

            slot.events.OnHoverWithItem += HandleHoverWithItem;
            slot.events.OnGrabItem += HandleGrabItem;
            ItemEventHandler.AddListener(ItemEvents.OnStartDrag, HandleGlobalItemDrag, this);
        }
        Task UpdateSlotBackground(bool isHovering, float transitionTime)
        {
            if (targetImage == null) return Task.Run(() => { });
            var startColor = targetImage.color;

            return ArchCurve.Smooth((float lerpValue) => {
                var targetColor = isHovering ? settings.hoverShade : defaultColor;

                targetImage.color = Color.Lerp(startColor, targetColor, lerpValue);
            }, CurveType.EaseIn, transitionTime);
        }

        public async void HandleGlobalItemDrag(ItemInfo item)
        {
            defaultColor = slot.CanInsert(item) ? settings.availableColor : settings.unavailableColor;
            await UpdateSlotBackground(false, .25f);
            await item.EndDragging();

            while (hoveringEffect) await Task.Yield();

            defaultColor = settings.idleShade;
            await UpdateSlotBackground(false, .25f);
        }

        void HandleGrabItem(InventorySlot slot, ItemInfo item)
        {
            PlayAudioClip(settings.grabItemClip);
        }

        void PlayAudioClip(AudioClip clip)
        {
            if (audioManager == null) return;
            audioManager.PlayAudioClip(clip);
        }
        void HandleHoverWithItem(InventorySlot slot, ItemInfo item, bool enter)
        {
            if (!enter) return;
            if (!slot.CanInsert(item)) return;
            if (hoveringEffect) return;
            hoveringEffect = true;

            var originalSize = rectTransform.sizeDelta;
            var targetSize = originalSize + (originalSize * settings.hoverSizeAdditive);

            Debugger.UI(5491, $"Hovering over slot {slot} with item {item}");

            taskHandler.ClearTasks();
            taskHandler.AddTask(HandleEnter);
            taskHandler.AddTask(slot.FinishHovering);
            taskHandler.AddTask(HandleExit);

            hoveringEffect = false;


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

                PlayAudioClip(settings.mouseOverClip);
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
