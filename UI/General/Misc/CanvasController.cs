using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasController : MonoBehaviour
    {
        CanvasGroup canvasGroup;
        public bool isActive;

        public bool interactable = true;
        public bool blocksRaycast = true;
        public bool hideOnStart;

        private void OnValidate()
        {
            UpdateCanvasGroup();
        }
        void Start()
        {
            GetDepdendencies();

            if (hideOnStart)
            {
                isActive = false;
                UpdateCanvasGroup();
            }
        }

        
        void GetDepdendencies()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void ToggleCanvas()
        {
            isActive = !isActive;
            UpdateCanvasGroup();
        }

        public void SetCanvas(bool val)
        {
            if (isActive == val) return;
            isActive = val;
            UpdateCanvasGroup();
        }

        void UpdateCanvasGroup()
        {
            if (canvasGroup == null) GetDepdendencies();
            ArchUI.SetCanvas(canvasGroup, isActive);

            if (isActive)
            {
                canvasGroup.interactable = interactable;
                canvasGroup.blocksRaycasts = blocksRaycast;
            }
        }


    }
}
