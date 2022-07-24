using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

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

        public Action<PauseMenu> OnTryOpenPause;

        public bool pauseBlocked;

        private void OnValidate()
        {
            UpdateMenu();
        }

        private void Update()
        {
            if (!selfEscape) return;
            if (!Input.GetKeyDown(KeyCode.Escape)) return;

            PauseMenuBack();

        }

        private void Awake()
        {
            active = this;
        }
        
        bool ContextMenuActive()
        {
            var contextMenu = ContextMenu.current;
            if (contextMenu == null) return false;
            if (!contextMenu.isChoosing) return false;

            return true;
        }

        public void PauseMenuBack()
        {
            pauseBlocked = false;

            if (ContextMenuActive()) return;

            OnTryOpenPause?.Invoke(this);

            if (pauseBlocked) return;


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

        public void LoadScene(string sceneName)
        {
            ArchSceneManager.active.LoadScene(sceneName);
            //SceneManager.LoadScene(sceneName);
        }

        public void SetCanvas(CanvasGroup canvas, bool val)
        {
            canvas.alpha = val ? 1 : 0;
            canvas.interactable = val;
            canvas.blocksRaycasts = val;
        }
    }
}
