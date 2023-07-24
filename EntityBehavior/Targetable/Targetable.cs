using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Architome;
public class Targetable : EntityProp
{
    // Start is called before the first frame update
    public GameObject entityObject;

    public GraphicsInfo graphics;
    public ContainerTargetables targetManager;

    public GameObject graphicsHover;
    public GameObject graphicsSelected;
    public GameObject graphicsHolding;

    public List<GameObject> hoverTargets;

    bool ignoreDead;
    public new void GetDependencies()
    {
        base.GetDependencies();

        targetManager = ContainerTargetables.active;

        if (targetManager == null) return;

        entityInfo.targetableEvents.OnHold += OnHold;
        entityInfo.targetableEvents.OnHover += OnHover;
        entityInfo.targetableEvents.OnSelect += OnSelect;
        entityInfo.infoEvents.OnMouseHover += OnMouseHover;

        if (!Entity.IsPlayer(entityInfo))
        {
            ignoreDead = true;
        }
    }

    void Start()
    {
        GetDependencies();
        SetValues(false);
    }

    void OnMouseHover(EntityInfo entity, bool isHovering, GameObject source)
    {
        hoverTargets ??= new();

        var canTarget = entityInfo.isAlive || (!entityInfo.isAlive && !ignoreDead);

        if (isHovering && canTarget)
        {
            if (!hoverTargets.Contains(source))
            {
                hoverTargets.Add(source);
            }
        }
        else
        {
            if (hoverTargets.Contains(source))
            {
                hoverTargets.Remove(source);
            }
        }

        if (hoverTargets.Count > 0)
        {
            targetManager.AddMouseOver(entity);
        }
        else
        {
            targetManager.RemoveMouseOver(entity);
        }
    }

    public void OnHover(bool val)
    {
        graphicsHover.SetActive(val);
    }

    public void OnSelect(bool val)
    {
        graphicsSelected.SetActive(val);
    }

    public void OnHold(bool val)
    {
        graphicsHolding.SetActive(val);
    }

    public void OnValidate()
    {
        SetValues(false);
    }

    public void SetValues(bool val)
    {
        graphicsHolding.SetActive(val);
        graphicsSelected.SetActive(val);
        graphicsHover.SetActive(val);
    }
    
}
