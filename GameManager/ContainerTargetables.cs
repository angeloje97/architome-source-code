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
        public GameObject currentHold;

        private KeyBindings keyBindings;

        public LayerMask targetLayer;

        public string selectKey = "Select";
        public string selectMultiple = "SelectMultiple";

        bool isSelecting;
        public bool IsSelecting
        {
            get { return isSelecting; }
            protected set { isSelecting = value; }
        }

        public Action<GameObject, GameObject> OnNewHoverTarget { get; set; }
        public Action<GameObject> OnSelectTarget;
        public Action OnClearSelected;

        GameObject hoverTargetCheck;

        void GetDependencies()
        {
            ArchInput.active.OnSelectMultiple += OnSelectMultiple;
            ArchInput.active.OnSelect += OnSelect;

            input = ArchInput.active;
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
        }
        public void Awake()
        {
            active = this;
        }
        void Update()
        {
            if (input.Mode == ArchInputMode.Inactive) return;
            HandleUserMouseOvers();
            HandleNullMouseOver();
            HandleEvents();
        }
        public void HandleEvents()
        {
            if (hoverTargetCheck != currentHover)
            {
                OnNewHoverTarget?.Invoke(hoverTargetCheck, currentHover);
                hoverTargetCheck = currentHover;
                
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
                if (Mouse.IsMouseOverUI()) { return false; }
                if (ClickableManager.active.currentClickableHover) return false;

                var entity = Mouse.CurrentHover(targetLayer);

                if (entity)
                {

                    Hover(entity.gameObject);

                    return true;
                    //if (result)
                    //{
                    //    if (!Player.HasLineOfSight(result.gameObject)) { return false; }
                    //    Hover(result.gameObject);

                    //    return true;
                    //}
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
        void OnSelect()
        {
            if (Mouse.IsMouseOverUI() && !IsOverPortraitOrHealthBar())
            {
                return;
            }


            ClearSelected();
            SelectEvent(currentHover);
        }
        void OnSelectMultiple()
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
                AddMouseOver(hoverTargets[0].GetComponent<EntityInfo>());
            }
            else
            {
                currentHover = null;
            }
            
        }
    }

    public struct TargetableEvents
    {
        public Action<bool> OnHover;
        public Action<bool> OnSelect;
        public Action<bool> OnHold;
    }

}
