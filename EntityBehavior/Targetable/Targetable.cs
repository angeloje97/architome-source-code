using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Architome;
using System.Threading.Tasks;
using System;

public class Targetable : EntityProp
{
    #region Common Data
    public GameObject entityObject;

    public GraphicsInfo graphics;
    public ContainerTargetables targetManager;

    public GameObject graphicsHover;
    public GameObject graphicsSelected;
    public GameObject graphicsHolding;

    public List<GameObject> hoverTargets;

    bool ignoreDead;
    #endregion

    #region Initiation
    public override void GetDependencies()
    {

            targetManager = ContainerTargetables.active;

            if (targetManager == null) return;

            entityInfo.targetableEvents.OnHold += OnHold;
            entityInfo.targetableEvents.OnHover += OnHover;
            entityInfo.targetableEvents.OnSelect += OnSelect;
            //entityInfo.infoEvents.OnMouseHover += OnMouseHover;

            if (!Entity.IsPlayer(entityInfo))
            {
                ignoreDead = true;
            }

        SetValues(false);
    }
    #endregion
    public void OnValidate()
    {
        SetValues(false);
    }
    #region Event Listeners
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
    #endregion

    public void SetValues(bool val)
    {
        graphicsHolding.SetActive(val);
        graphicsSelected.SetActive(val);
        graphicsHover.SetActive(val);
    }
    
}
