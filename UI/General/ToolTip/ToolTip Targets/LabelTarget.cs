using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Architome
{
    public class LabelTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string label;
        public bool followMouse = true;

        ToolTip currentToolTip;
        ToolTipManager manager;
        void Start()
        {
            GetDependencies();
        }
        void Update()
        {
        
        }

        void GetDependencies()
        {
            manager = ToolTipManager.active;
        }

        // Update is called once per frame

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentToolTip != null) return;

            currentToolTip = manager.Label();

            if (followMouse)
            {
                currentToolTip.followMouse = true;
            }
            else
            {
                currentToolTip.adjustToMouse = true;
            }

            currentToolTip.SetToolTip(new() { name = label });
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (currentToolTip == null) return;

            currentToolTip.DestroySelf();
        }
    }
}
