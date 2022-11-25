using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.EventSystems;

namespace Architome
{
    public class ItemToolTipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        ItemInfo info;
        bool hovering;

        void Start()
        {
            GetDependencies();
        }

        void GetDependencies()
        {
            info = GetComponent<ItemInfo>();
        }

        void OnMouseEnter()
        {
            HandleHover();
        }

        void OnMouseExit()
        {
            hovering = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            HandleHover();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovering = false;

        }

        async void HandleHover()
        {
            if (hovering) return;
            var toolTipManager = ToolTipManager.active;
            if (toolTipManager == null) return;

            hovering = true;

            var toolTip = toolTipManager.GeneralHeader();
            toolTip.adjustToMouse = true;

            toolTip.SetToolTip(info.item.ToolTipData(info.currentStacks));


            while (hovering)
            {
                if (this == null) break;
                await Task.Yield();
            }

            toolTip.DestroySelf();
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
