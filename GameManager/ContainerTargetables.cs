using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;


namespace Architome
{
    public class ContainerTargetables : MonoBehaviour
    {
        public static ContainerTargetables active;
        // Start is called before the first frame update
        public List<GameObject> hoverTargets;
        public List<GameObject> selectedTargets;

        public GameObject currentHover;
        public GameObject currentHold;

        private KeyBindings keyBindings;

        public LayerMask targetLayer;

        public string selectKey = "Select";
        public string selectMultiple = "SelectMultiple";

        public bool clicked;
        public bool left;

        void Start()
        {
            hoverTargets = new List<GameObject>();
            selectedTargets = new List<GameObject>();

            if (GMHelper.KeyBindings())
            {
                keyBindings = GMHelper.KeyBindings();
            }

        }

        public void Awake()
        {
            active = this;
        }

        // Update is called once per frame
        void Update()
        {
            HandleUserMouseOvers();
            HandleUserInputs();
            HandleNullMouseOver();
        }

        public void HandleUserMouseOvers()
        {
            if (IsOverHealthBar()) { }
            else if (IsOverTarget()) { }
            else if (IsOverPortrait()) { }
            else
            {
                ClearHovers();
                //if (hoverTargets.Count > 0) { hoverTargets.Clear(); }
                //currentHover = null;
            }

            bool IsOverTarget()
            {
                if (Mouse.IsMouseOverUI()) { return false; }
                if (Mouse.CurrentHover(targetLayer))
                {
                    var result = Mouse.CurrentHover(targetLayer);

                    var hoverObject = Mouse.CurrentHoverObject();

                    if (hoverObject.GetComponent<Clickable>())
                    {
                        return false;
                    }

                    if (result.GetComponent<EntityInfo>())
                    {
                        if (!Player.HasLineOfSight(result)) { return false; }
                        if (!hoverTargets.Contains(result))
                        {
                            Hover(result);
                            //hoverTargets.Add(result);
                            //currentHover = result;
                        }

                        return true;
                    }
                }


                return false;
            }


        }
        bool IsOverPortrait()
        {
            var rayCastObjects = Mouse.RayCastResultObjects();

            foreach (GameObject castObject in rayCastObjects)
            {
                if (castObject.transform.parent)
                {

                    if (castObject.GetComponentInParent<PortraitBehavior>())
                    {
                        if (castObject.GetComponentInParent<PortraitBehavior>() && castObject.GetComponentInParent<PortraitBehavior>().entity)
                        {
                            if (!hoverTargets.Contains(castObject.transform.parent.GetComponentInParent<PortraitBehavior>().entity.gameObject))
                            {
                                //currentHover = castObject.transform.parent.GetComponentInParent<PortraitBehavior>().entity.gameObject;
                                //hoverTargets.Add(castObject.transform.parent.GetComponentInParent<PortraitBehavior>().entity.gameObject);

                                Hover(castObject.GetComponentInParent<PortraitBehavior>().entity.gameObject);
                            }

                            return true;
                        }
                    }


                }
            }

            return false;
        }
        bool IsOverHealthBar()
        {
            var results = Mouse.RayCastResultObjects();

            if (results.Count == 0) { return false; }
            if (Mouse.IsMouseOverUI()) { return false; }
            foreach (var result in results)
            {
                if (result.GetComponentInParent<ProgressBarsBehavior>() && result.GetComponentInParent<EntityInfo>())
                {
                    if (!hoverTargets.Contains(result.GetComponentInParent<EntityInfo>().gameObject))
                    {
                        //hoverTargets.Add(result.GetComponentInParent<EntityInfo>().gameObject);
                        //currentHover = result.GetComponentInParent<EntityInfo>().gameObject;

                        Hover(result.GetComponentInParent<EntityInfo>().gameObject);
                    }
                    return true;
                }
            }
            return false;
        }


        public void HandleUserInputs()
        {



            if (Input.GetKeyDown(keyBindings.keyBinds[selectKey]))
            {
                //Clear if the selectMultiple button is not pressed

                if (!Input.GetKey(keyBindings.keyBinds[selectMultiple]))
                {
                    if (Mouse.IsMouseOverUI() && !IsOverPortrait())
                    {
                        if (!IsOverHealthBar())
                        {
                            return;
                        }
                    }

                    //selectedTargets.Clear();
                    ClearSelected();
                }



                if (currentHover)
                {
                    //currentHold = currentHover;
                    Hold(currentHover);
                    clicked = true;
                }
            }

            if (Input.GetKeyUp(keyBindings.keyBinds[selectKey]))
            {
                if (currentHover == currentHold && !selectedTargets.Contains(currentHold))
                {
                    

                    if (!left)
                    {
                        if (currentHold != null)
                        {
                            //selectedTargets.Add(currentHold);
                            AddSelected(currentHold);
                        }

                    }
                }

                clicked = false;
                left = false;


                //currentHold = null;
                UnHold();
            }
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
            selectedTargets.Add(target);
            
        }

        public void ClearSelected()
        {
            for(int i = 0; i < selectedTargets.Count; i++)
            {
                selectedTargets[i].GetComponent<EntityInfo>().targetableEvents.OnSelect?.Invoke(false);
                selectedTargets.RemoveAt(i);
                i--;
            }
        }

        public void ClearHovers(bool clearCurrentHover = true)
        {
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
        public void Hover(GameObject target)
        {
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


    }

    public struct TargetableEvents
    {
        public Action<bool> OnHover;
        public Action<bool> OnSelect;
        public Action<bool> OnHold;
    }

}
