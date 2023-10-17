
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.WSA;

namespace Architome
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {

        #region Static Properties
        public static List<CanvasController> activeCanvases;

        static bool staticEscape;

        static async void HandleActiveCanvasesEscape()
        {
            if (staticEscape) return;
            staticEscape = true;

            var size = activeCanvases.Count;

            if(size != 0)
            {
                var top = activeCanvases[size - 1];

                top.SetCanvas(false);
            }


            await Task.Delay(125);

            staticEscape = false;
        }

        #endregion



        CanvasGroup canvasGroup;
        public bool isActive { get; set; }

        public bool interactable = true;
        public bool blocksRaycast = true;
        public bool hideOnStart;

        [Header("Self Escape Properties")]
        [SerializeField] bool selfEscape;

        public List<CanvasGroup> otherCanvasGroups;

        public Action<CanvasController> OnCanCloseCheck;
        public Action<CanvasController> OnCanOpenCheck;
        public Action<bool> OnCanvasChange;

        public UnityEvent<bool> OnMouseOverChange;
        public List<bool> checks { get; private set; }
        public bool haltChange { get; set; }

        public bool mouseOver;

        bool mouseOverCheck;

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

            HandleActiveChange();
        }

        void Awake()
        {
            activeCanvases ??= new();
        }
        private void Update()
        {
            OnEscape();
            HandleEvents();
        }

        void HandleEvents()
        {


            if (!isActive) return;

            if(mouseOver != mouseOverCheck)
            {
                mouseOverCheck = mouseOver;
                OnMouseOverChange?.Invoke(mouseOver);

            }
        }

        async void HandleActiveChange()
        {
            if (!selfEscape) return;
            bool currentActive = false;
            while (this)
            {
                await Task.Yield();
                if(currentActive != isActive)
                {
                    currentActive = isActive;
                    HandleCanvasStack();

                }
            }

            void HandleCanvasStack()
            {
                if(isActive && !activeCanvases.Contains(this))
                {
                    activeCanvases.Add(this);
                }

                else if(isActive && activeCanvases.Contains(this))
                {
                    activeCanvases.Remove(this);
                }
                
            }
        }

        void OnEscape()
        {
            if (!selfEscape) return;
            if (!isActive) return;
            if (!Input.GetKeyUp(KeyCode.Escape)) return;
            HandleActiveCanvasesEscape();
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

        #region PointerEvents
        

        public void OnPointerEnter(PointerEventData eventData)
        {
            mouseOver = true;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (activeCanvases.Count <= 0) return;
            int target = 0;
            float maxZ = 0;
            bool foundTarget = false;
            for(int i = 0; i < activeCanvases.Count; i++)
            {
                var current = activeCanvases[i];

                var rectTransform = current.GetComponent<RectTransform>();
                var position = rectTransform.position;
                if(maxZ <= 0)
                {
                    maxZ = position.z;
                }
                else
                {
                    maxZ += 1;
                    rectTransform.position = new Vector3(position.x, position.y, maxZ);

                }

                if (current == this)
                {
                    target = i;
                    foundTarget = true;
                }
            }

            if (foundTarget)
            {
                var lastPos = activeCanvases.Count - 1;
                var last = activeCanvases[lastPos];
                var targetCanvas = activeCanvases[target];                

                var temp = activeCanvases[lastPos];
                activeCanvases[lastPos] = activeCanvases[target];
                activeCanvases[target] = temp;

                var pos1 = targetCanvas.transform.position;
                var pos2 = last.transform.position; 

                last.transform.position = new(pos2.x, pos2.y, pos1.z);
                targetCanvas.transform.position = new (pos1.x, pos1.y, pos2.z);


                activeCanvases.Add(this);
            }

        }

        public void OnPointerExit(PointerEventData eventData)
        {
            mouseOver = false;
        }

        #endregion


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
