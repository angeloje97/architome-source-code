using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Architome
{
    public class ToolTipElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        ToolTipManager manager;
        ContextMenu activeMenu;
        public ToolTipType type;
        public ToolTipData data;


        public bool isShowing;
        public bool hideToolTip;
        public bool followCursor;

        public bool hideOnActiveContextMenu;

        public Action<ToolTipElement> BeforeShowToolTip;
        public Action<ToolTipElement> OnCanShowCheck;
        
        [HideInInspector]public List<bool> checks;

        void Start()
        {
            GetDependenciess();
        }

        void GetDependenciess()
        {
            manager = ToolTipManager.active;
            activeMenu = ContextMenu.current;


            if (!manager)
            {
                gameObject.SetActive(false);
            }

        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnMouseEnter()
        {
            HandleShow();
        }

        void OnMouseExit()
        {
            StopShowing();
        }

        public bool CanShow()
        {
            checks = new List<bool>();

            OnCanShowCheck?.Invoke(this);

            foreach (var check in checks)
            {

                if (!check) return false;
            }

            return true;
        }

        public async void HandleShow()
        {
            if (isShowing) return;
            if (hideToolTip) return;
            if (!CanShow()) return;
            isShowing = true;

            var toolTip = manager.ToolTip(type);

            BeforeShowToolTip?.Invoke(this);

            if (followCursor)
            {
                toolTip.followMouse = true;
            }

            toolTip.SetToolTip(data);

            HandleContextMenu(toolTip);


            while (isShowing)
            {
                await Task.Yield();
            }

            toolTip.DestroySelf();

            isShowing = false;
        }


        async void HandleContextMenu(ToolTip tooltip)
        {
            if (!hideOnActiveContextMenu) return;
            if (!activeMenu) return;

            activeMenu.OnChoosingChange += HandleChoosingChange;
            tooltip.SetVisibility(!activeMenu.isChoosing);

            while (isShowing)
            {
                if (tooltip == null) break;
                await Task.Yield();
            }

            activeMenu.OnChoosingChange -= HandleChoosingChange;

            void HandleChoosingChange(ContextMenu contextMenu, bool isActive)
            {
                tooltip.SetVisibility(!isActive);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            HandleShow();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StopShowing();
        }

        public void StopShowing()
        {
            isShowing = false;
        }
    }
}
