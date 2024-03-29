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
            [Serializable]
            public struct SlotFX
            {
                public InventorySlotEvent trigger;
                public AudioClip audioClip;
            }

            public float hoverSizeAdditive;
            public Color idleShade;
            public Color hoverShade;
            public Color availableColor;
            public Color unavailableColor;


            public List<SlotFX> effects;

        }

        public FXSettings settings;

        public InventorySlot slot;
        public Image targetImage { get; set; }
        public RectTransform rectTransform;
        public TaskQueueHandler taskHandler;
        public AudioManager audioManager;

        Color defaultColor;

        bool hoveringEffect;

        void Start()
        {
            GetDependencies();
            HandleListeners();
        }

        void GetDependencies()
        {
            taskHandler = new(TaskType.Sequential);
            slot = GetComponent<InventorySlot>();
            rectTransform = GetComponent<RectTransform>();
            if (slot == null) return;
            defaultColor = settings.idleShade;
            UpdateSlotBackground(false, 0f);

            ItemEventHandler.AddListener(ItemEvents.OnStartDrag, HandleGlobalItemDrag, this);
        }

        void HandleListeners()
        {
            slot.AddListener(InventorySlotEvent.OnHoverWithItem, HandleHoverWithItem, this);


            if (settings.effects == null) return;

            foreach(var fx in settings.effects)
            {
                slot.AddListener(fx.trigger, ((InventorySlot, ItemInfo) data) => {
                    PlayAudioClip(fx.audioClip);
                }, this);
            }
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

        void PlayAudioClip(AudioClip clip)
        {
            if (audioManager == null) return;
            audioManager.PlayAudioClip(clip);
        }
        void HandleHoverWithItem((InventorySlot, ItemInfo) data)
        {
            var item = data.Item2;
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
