using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Architome;

public class ContainerTargetables : MonoBehaviour
{
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

        if(GMHelper.KeyBindings())
        {
            keyBindings = GMHelper.KeyBindings();
        }

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
            if (hoverTargets.Count > 0) { hoverTargets.Clear(); }
            currentHover = null;
        }

        bool IsOverTarget()
        {
            if (Mouse.IsMouseOverUI()) { return false; }
            if (Mouse.CurrentHover(targetLayer))
            {
                var result = Mouse.CurrentHover(targetLayer);

                var hoverObject = Mouse.CurrentHoverObject();

                if(hoverObject.GetComponent<Clickable>())
                {
                    return false;
                }

                if (result.GetComponent<EntityInfo>())
                {
                    if (!Player.HasLineOfSight(result)) { return false; }
                    if (!hoverTargets.Contains(result))
                    {
                        hoverTargets.Add(result);
                        currentHover = result;
                    }

                    return true;
                }
            }
            

            return false;
        }
        bool IsOverPortrait()
        {
            var rayCastObjects = Mouse.RayCastResultObjects();

            foreach(GameObject castObject in rayCastObjects)
            {
                if(castObject.transform.parent)
                {
                    if(castObject.GetComponentInParent<PortraitBehavior>() && castObject.GetComponentInParent<PortraitBehavior>().entity)
                    {
                        if(!hoverTargets.Contains(castObject.GetComponentInParent<PortraitBehavior>().entity.gameObject))
                        {
                            currentHover = castObject.GetComponentInParent<PortraitBehavior>().entity.gameObject;
                            hoverTargets.Add(castObject.GetComponentInParent<PortraitBehavior>().entity.gameObject);
                        }

                        

                        return true;
                    }

                    if(castObject.transform.parent.parent)
                    {
                        if(castObject.transform.parent.GetComponentInParent<PortraitBehavior>() && castObject.transform.parent.GetComponentInParent<PortraitBehavior>().entity)
                        {
                            if(!hoverTargets.Contains(castObject.transform.parent.GetComponentInParent<PortraitBehavior>().entity.gameObject))
                            {
                                currentHover = castObject.transform.parent.GetComponentInParent<PortraitBehavior>().entity.gameObject;
                                hoverTargets.Add(castObject.transform.parent.GetComponentInParent<PortraitBehavior>().entity.gameObject);
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
            
            if(results.Count == 0) { return false; }
            if (Mouse.IsMouseOverUI()) { return false; }
            foreach(var result in results)
            {
                if(result.GetComponentInParent<ProgressBarsBehavior>() && result.GetComponentInParent<EntityInfo>())
                {
                    if(!hoverTargets.Contains(result.GetComponentInParent<EntityInfo>().gameObject))
                    {
                        hoverTargets.Add(result.GetComponentInParent<EntityInfo>().gameObject);
                        currentHover = result.GetComponentInParent<EntityInfo>().gameObject;
                    }
                    return true;
                }
            }
            return false;
        }
    }
    public void HandleUserInputs()
    {
        if (Input.GetKeyDown(keyBindings.keyBinds[selectKey]))
        {
            //Clear if the selectMultiple button is not pressed

            if(!Input.GetKey(keyBindings.keyBinds[selectMultiple]))
            {
                selectedTargets.Clear();
            }

            if(currentHover)
            {
                currentHold = currentHover;
                clicked = true;
            }
        }

        if (Input.GetKeyUp(keyBindings.keyBinds[selectKey]))
        {
            if(currentHover == currentHold && !selectedTargets.Contains(currentHold))
            {
                if(!left)
                {
                    if(currentHold != null)
                    {
                        selectedTargets.Add(currentHold);
                    }
                    
                }
            }

            clicked = false;
            left = false;
            currentHold = null;
        }
    }
    public void HandleNullMouseOver()
    {
        foreach(GameObject hoverTarget in hoverTargets)
        {
            if(hoverTarget != currentHover)
            {
                hoverTargets.Remove(hoverTarget);
                return;
            }
        }
    }

    public void HandleEnter(GameObject check)
    {

    }

    public void HandleExit(GameObject check)
    {

    }
    
}
