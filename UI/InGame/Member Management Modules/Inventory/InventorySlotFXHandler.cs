using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Architome
{
    public class InventorySlotFXHandler : MonoBehaviour
    {
        [SerializeField]
        public struct FXSettings
        {
            public float hoverSizeAdditive;
            public Image targetImage;
            public Color idleShade;
            public Color hoverShade;
        }

        public FXSettings settings;

        public InventorySlot slot;
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

            slot.events.OnHoverWithItem += HandleHoverWithItem;
        }

        void HandleHoverWithItem(InventorySlot slot, ItemInfo item, bool enter)
        {
            if (!enter) return;
            if (!slot.CanInsert(item)) return;

            var originalSize = rectTransform.sizeDelta;
            var targetSize = originalSize + (originalSize * settings.hoverSizeAdditive);

            taskHandler.AddTask(HandleEnter);
            taskHandler.AddTask(slot.FinishHovering);
            taskHandler.AddTask(HandleExit);


            async Task HandleEnter()
            {

                await ArchCurve.Smooth((float lerpValue) => {
                    rectTransform.sizeDelta = Vector2.Lerp(originalSize, targetSize, lerpValue);
                }, CurveType.EaseIn, .25f);
            }

            async Task HandleExit()
            {
                await ArchCurve.Smooth((float lerpValue) => {
                    rectTransform.sizeDelta = Vector2.Lerp(targetSize, originalSize, lerpValue);
                }, CurveType.EaseIn, .25f);
            }
        }

        void Update()
        {
        
        }



        // Update is called once per frame


    }
}
