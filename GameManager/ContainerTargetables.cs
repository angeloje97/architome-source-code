using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using System.Threading.Tasks;
using Architome.Enums;


namespace Architome
{
    public class ContainerTargetables : MonoBehaviour
    {
        public static ContainerTargetables active;
        public ArchInput input;
        // Start is called before the first frame update
        public List<GameObject> hoverTargets;
        public List<GameObject> selectedTargets;

        public GameObject currentHover;
        public GameObject hoverObject;
        public GameObject currentHold;

        private KeyBindings keyBindings;

        public LayerMask targetLayer;

        public string selectKey = "Select";
        public string selectMultiple = "SelectMultiple";

        [SerializeField] bool isOverUI;
        [SerializeField] bool isOverClickable;
        [SerializeField] bool nullCheck;

        bool isSelecting;
        public bool IsSelecting
        {
            get { return isSelecting; }
            protected set { isSelecting = value; }
        }

        public Action<GameObject, GameObject> OnNewHoverTarget { get; set; }
        public Action<GameObject> OnSelectTarget;
        public Action OnClearSelected { get; set; }

        public Action OnSelectNothing { get; set; }

        public Action OnClearFromEscape;

        GameObject hoverTargetCheck;

        void GetDependencies()
        {
            

            input = ArchInput.active;

            input.OnEscape += OnEscape;
            input.OnSelectMultiple += OnSelectMultiple;
            input.OnSelect += OnSelect;

            var cursorBehavior = CursorBehavior.active;

            if (cursorBehavior)
            {
                cursorBehavior.WhileCursorMove += WhileCursorMove;
            }
        }
        void Start()
        {
            hoverTargets = new List<GameObject>();
            selectedTargets = new List<GameObject>();

            if (GMHelper.KeyBindings())
            {
                keyBindings = GMHelper.KeyBindings();
            }

            GetDependencies();

            HandlePauseMenu();
            HandleIGGUI();
        }
        public void Awake()
        {
            active = this;
        }

        void HandleIGGUI()
        {
            var gui = IGGUIInfo.active;
            if (gui == null) return;

            gui.OnClosingModulesCheck += HandleCloseModulesCheck;

            void HandleCloseModulesCheck(IGGUIInfo iggui, List<bool> checks)
            {
                if(selectedTargets.Count > 0)
                {
                    checks.Add(false);
                }
            }
        }

        void HandlePauseMenu()
        {
            var pauseMenu = PauseMenu.active;
            if (pauseMenu == null) return;
            pauseMenu.OnTryOpenPause += HandleTryOpen;
            pauseMenu.OnActiveChange += HandleActiveChange;


            void HandleActiveChange(PauseMenu pauseMenu, bool isActive)
            {
                if (isActive)
                {
                    ClearHovers();
                    ClearSelected();
                }
            }
            void HandleTryOpen(PauseMenu menu)
            {
                if(selectedTargets.Count != 0)
                {
                    menu.pauseBlocked = true;
                    return;
                }

                ArchAction.Yield(() => {
                    if (menu.isActive)
                    {
                        ClearHovers();
                    }
                });

                //if (currentHover)
                //{
                //    ClearHovers();
                //}
            }


        }

        void WhileCursorMove(Vector3 mousePosition)
        {
           
            //if (input.Mode == ArchInputMode.Inactive) return;
            
        }
        void FixedUpdate()
        {
            HandleEvents();

            if (input.Mode == ArchInputMode.Inactive) return;

            //hoverObject = Mouse.CurrentHover(targetLayer);
            //HandleUserMouseOvers();
            //HandleNullMouseOver();

        }
        public void HandleEvents()
        {
            if (hoverTargetCheck != currentHover)
            {
                OnNewHoverTarget?.Invoke(hoverTargetCheck, currentHover);

                if(currentHover)
                {
                    nullCheck = true;
                }
                else
                {
                    nullCheck = false;
                }
                
                hoverTargetCheck = currentHover;
            }

            if(nullCheck && !currentHover && !hoverTargetCheck)
            {
                OnNewHoverTarget?.Invoke(null, null);
                nullCheck = false;
            }



        }
        public void HandleUserMouseOvers()
        {
            if(IsOverTarget()) { }
            else if(IsOverPortraitOrHealthBar()) { }
            else
            {
                if(hoverTargets.Count > 0)
                {
                    ClearHovers();
                }
            }

            bool IsOverTarget()
            {
                if (Mouse.IsMouseOverUI())
                {
                    isOverUI = true;
                    return false;
                }
                else
                {
                    isOverUI = false;
                }
                if (ClickableManager.active.currentClickableHover)
                {
                    isOverClickable = true;
                    return false;
                }
                else
                {
                    isOverClickable = false;
                }

                //var entity = Mouse.CurrentHover(targetLayer);
                var entity = hoverObject;

                if (entity)
                {

                    Hover(entity);

                    return true;
                }


                return false;
            }


        }

        bool IsOverPortraitOrHealthBar()
        {
            var results = Mouse.RayCastResults();

            foreach (var result in results)
            {
                var castObject = result.gameObject;
                if (IsOverPortrait(castObject)) return true;
                if (IsOverHealthBar(castObject)) return true;
            }


            return false;

            bool IsOverPortrait(GameObject target)
            {
                var portraitBehavior = target.GetComponentInParent<PortraitBehavior>();

                if (portraitBehavior == null) return false;
                if (portraitBehavior.entity == null) return false;

                Hover(portraitBehavior.entity.gameObject);

                return true;
            }

            bool IsOverHealthBar(GameObject target)
            {
                var progressBar = target.GetComponentInParent<ProgressBarsBehavior>();

                if (progressBar == null) return false;
                if (progressBar.entityInfo == null) return false;

                Hover(progressBar.entityInfo.gameObject);

                return true;
            }
        }
        
        public EntityInfo CurrentTarget(bool useSelectedTargets = false)
        {
            if (currentHover) return currentHover.GetComponent<EntityInfo>();
            if(selectedTargets.Count > 0 && useSelectedTargets)
            {
                return selectedTargets[^1].GetComponent<EntityInfo>();
            }
            var mouseOver = Mouse.CurrentHover(targetLayer);

            if (mouseOver)
            {
                Hover(mouseOver);
                return mouseOver.GetComponent<EntityInfo>();
            }

            return null;
        }

        void OnEscape()
        {
            if (selectedTargets.Count <= 0) return;
            ClearSelected();
            OnClearFromEscape?.Invoke();
        }
        public void OnSelect()
        {
            ClearSelected();
            SelectEvent(currentHover);

            if(currentHover == null)
            {
                OnSelectNothing?.Invoke();
            }
        }
        public void OnSelectMultiple()
        {
            SelectEvent(currentHover);
        }
        async void SelectEvent(GameObject target)
        {
            if (target == null) return;
            if (isSelecting) return;

            isSelecting = true;

            Hold(target);
            bool isHovering = true;

            while (Input.GetKey(keyBindings.keyBinds["Select"]))
            {
                await Task.Yield();
                if (currentHover != currentHold)
                {
                    isHovering = false;
                }
            }

            UnHold();

            if (isHovering)
            {
                AddSelected(target);
            }

            isSelecting = false;

        }
        public void Hold(GameObject target)
        {
            target.GetComponent<EntityInfo>().targetableEvents.OnHold?.Invoke(true);
            currentHold = target;
        }
        public void UnHold()
        {
            if(currentHold != null)
            {
                currentHold.GetComponent<EntityInfo>().targetableEvents.OnHold?.Invoke(false);
                currentHold = null;
            }

        }
        public void AddSelected(GameObject target)
        {
            if(selectedTargets.Contains(target)) return;   
            target.GetComponent<EntityInfo>().targetableEvents.OnSelect?.Invoke(true);
            OnSelectTarget?.Invoke(target);
            selectedTargets.Add(target);
            
        }
        public void ClearSelected()
        {
            ClearNullSelected();
            for(int i = 0; i < selectedTargets.Count; i++)
            {
                selectedTargets[i].GetComponent<EntityInfo>().targetableEvents.OnSelect?.Invoke(false);
                selectedTargets.RemoveAt(i);
                i--;
            }

            OnClearSelected?.Invoke();
        }
        public void ClearHovers(bool clearCurrentHover = true)
        {
            ClearNullHovers();

            for(int i = 0; i < hoverTargets.Count; i++)
            {
                hoverTargets[i].GetComponent<EntityInfo>().targetableEvents.OnHover?.Invoke(false);
                hoverTargets.RemoveAt(i);
                i--;
            }

            if (clearCurrentHover)
            {
                HoverRemove();
            }

        }
        public void ClearNullSelected()
        {
            selectedTargets = selectedTargets.Where(target => target != null).ToList();
        }
        public void ClearNullHovers()
        {
            hoverTargets = hoverTargets.Where(target => target != null).ToList();
        }
        public void Hover(GameObject target)
        {
            if (hoverTargets.Contains(target)) return;
            var info = target.GetComponent<EntityInfo>();
            if (info == null) return;
            if (info.currentRoom && !info.currentRoom.isRevealed) return;


            hoverTargets.Add(target);
            currentHover = target;

            Debugger.InConsole(6482, $"{target.GetComponent<EntityInfo>().targetableEvents}");
            target.GetComponent<EntityInfo>().targetableEvents.OnHover?.Invoke(true);
        }
        public void HoverRemove()
        {
            if(currentHover != null)
            {
                currentHover.GetComponent<EntityInfo>().targetableEvents.OnHover?.Invoke(false);
                currentHover = null;
            }

        }
        public void HandleNullMouseOver()
        {
            for(int i = 0; i< hoverTargets.Count; i++)
            {
                if (hoverTargets[i] == null)
                {
                    hoverTargets.RemoveAt(i);
                    i--;
                }
                else if(hoverTargets[i] != currentHover)
                {
                    hoverTargets[i].GetComponent<EntityInfo>().targetableEvents.OnHover?.Invoke(false);
                    hoverTargets.RemoveAt(i);
                    i--;
                }

            }
        }
        public void AddMouseOver(EntityInfo target)
        {
            if (!hoverTargets.Contains(target.gameObject))
            {
                hoverTargets.Add(target.gameObject);
            }

            currentHover = target.gameObject;

            target.targetableEvents.OnHover?.Invoke(true);

        }
        public void RemoveMouseOver(EntityInfo target)
        {
            if (!hoverTargets.Contains(target.gameObject))
            {
                return;
            }

            hoverTargets.Remove(target.gameObject);
            target.targetableEvents.OnHover?.Invoke(false);

            if (hoverTargets.Count > 0)
            {
                ClearHovers();
            }
            else
            {
                currentHover = null;
            }
            
        }
    }

    public struct TargetableEvents
    {
        public Action<bool> OnHover { get; set; }
        public Action<bool> OnSelect { get; set; }
        public Action<bool> OnHold { get; set; }
    }

}
