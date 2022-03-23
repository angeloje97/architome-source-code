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

    public new void GetDependencies()
    {
        base.GetDependencies();

        entityInfo.targetableEvents.OnHold += OnHold;
        entityInfo.targetableEvents.OnHover += OnHover;
        entityInfo.targetableEvents.OnSelect += OnSelect;
    }

    void Start()
    {
        GetDependencies();
        SetValues(false);
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
