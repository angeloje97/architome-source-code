using JetBrains.Annotations;
using System;
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

        [Header("Self Escape Properties")]
        [SerializeField] bool selfEscape;

        public List<CanvasGroup> otherCanvasGroups;

        public Action<CanvasController> OnCanCloseCheck;
        public Action<CanvasController> OnCanOpenCheck;
        public List<bool> checks { get; private set; }
        public bool haltChange { get; set; }

        private void OnValidate()
        {
            UpdateCanvasGroup();
        }
        void Start()
        {
            GetDepdendencies();
            HandleSelfEscape();

            if (hideOnStart)
            {
                isActive = false;
                UpdateCanvasGroup();
            }
        }

        private void Update()
        {
            OnEscape();
        }

        void OnEscape()
        {
            if (!selfEscape) return;
            if (!isActive) return;
            if (!Input.GetKeyUp(KeyCode.Escape)) return;
            SetCanvas(false);


        }

        void GetDepdendencies()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        void HandleSelfEscape()
        {
            if (!selfEscape) return;

            var iggui = IGGUIInfo.active;
            var menu = PauseMenu.active;

            if (iggui)
            {
                iggui.OnClosingModulesCheck += HandleClosingModuleCheck;
            }

            if (menu)
            {
                menu.OnTryOpenPause += HandlePauseMenu;
            }

            void HandlePauseMenu(PauseMenu menu)
            {
                if (isActive)
                {
                    menu.pauseBlocked = true;
                }
            }

            void HandleClosingModuleCheck(IGGUIInfo iggui, List<bool> checks)
            {
                if (isActive)
                {
                    checks.Add(false);
                }
            }
        }


        bool CanOpen()
        {
            if (haltChange) return false;
            checks = new();

            OnCanOpenCheck?.Invoke(this);

            foreach (var check in checks)
            {
                if (!check) return false;
            }

            return true;
        }

        bool CanClose()
        {
            if (haltChange) return false;
            checks = new();

            OnCanCloseCheck?.Invoke(this);

            foreach (var check in checks)
            {
                if (!check) return false;
            }

            return true;
        }



        public void ToggleCanvas()
        {
            if (isActive && !CanClose()) return;
            if (!isActive && !CanOpen()) return;
            isActive = !isActive;
            UpdateCanvasGroup();
        }

        public void SetCanvas(bool val)
        {
            if (isActive == val) return;
            if (val && !CanOpen()) return;
            if (!val && !CanClose()) return;
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

            if (otherCanvasGroups != null)
            {
                foreach(var group in otherCanvasGroups)
                {
                    ArchUI.SetCanvas(group, isActive);
                    if (isActive)
                    {
                        group.interactable = interactable;
                        group.blocksRaycasts = blocksRaycast;
                    }
                }
            }
        }


    }
}
