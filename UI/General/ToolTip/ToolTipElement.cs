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
        public ToolTipType type;
        public ToolTipData data;


        public bool isShowing;
        public bool hideToolTip;
        public bool followCursor;

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

            if (!manager)
            {
                gameObject.SetActive(false);
            }
        }

        // Update is called once per frame
        void Update()
        {

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


        public async void OnPointerEnter(PointerEventData eventData)
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
            

            while (isShowing)
            {
                await Task.Yield();
            }

            toolTip.DestroySelf();

            isShowing = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isShowing = false;
        }

        
    }
}
