using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Architome
{
    public class PauseMenu : MonoBehaviour
    {
        public static PauseMenu active;

        public bool isActive;
        public bool selfEscape;

        public Action<PauseMenu, bool> OnActiveChange;

        bool activeCheck;

        public List<CanvasGroup> items;
        public CanvasGroup router;

        private void OnValidate()
        {
            UpdateMenu();
        }

        private void Update()
        {
            if (!selfEscape) return;
            if (!Input.GetKeyDown(KeyCode.Escape)) return;

            OnEscape();

        }

        private void Awake()
        {
            active = this;
        }

        public void OnEscape()
        {
            bool changed = false;
            foreach (var item in items)
            {
                if (item.alpha == 1)
                {
                    SetCanvas(item, false);
                    changed = true;
                }
            }

            if (changed)
            {
                SetCanvas(router, true);
                return;
            }

            ToggleMenu();
        }

        public void ToggleMenu()
        {
            isActive = !isActive;
            UpdateMenu();
        }

        public void SetMenu(bool active)
        {
            this.isActive = active;

            UpdateMenu();
        }

        

        void UpdateMenu()
        {
            if (router == null) return;

            if (activeCheck != isActive)
            {
                activeCheck = isActive;
                OnActiveChange?.Invoke(this, isActive);
            }

            SetCanvas(router, isActive);

        }

        public void SetCanvas(CanvasGroup canvas, bool val)
        {
            canvas.alpha = val ? 1 : 0;
            canvas.interactable = val;
            canvas.blocksRaycasts = val;
        }
    }
}
