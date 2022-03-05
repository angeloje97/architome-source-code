using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Architome;
public class Targetable : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject entityObject;

    public GraphicsInfo graphics;
    public EntityInfo entityInfo;
    public ContainerTargetables targetManager;

    public GameObject graphicsHover;
    public GameObject graphicsSelected;
    public GameObject graphicsHolding;

    public void GetDependencies()
    {
        if(entityInfo == null)
        {
            if(GetComponentInParent<GraphicsInfo>() != null)
            {
                graphics = GetComponentInParent<GraphicsInfo>();

                if(graphics.entityInfo)
                {
                    entityObject = graphics.entityObject;
                    entityInfo = graphics.entityInfo;
                }
            }

            if(GMHelper.TargetManager())
            {
                targetManager = GMHelper.TargetManager();
            }
        }
    }


    void Start()
    {
        
    }

    void Update()
    {
        GetDependencies();
        UpdateTargetManagerIcons();
    }

    public void UpdateTargetManagerIcons()
    {
        if(targetManager.hoverTargets.Contains(entityObject))
        {
            graphicsHover.SetActive(true);
        }
        else
        {
            graphicsHover.SetActive(false);
        }

        if(targetManager.selectedTargets.Contains(entityObject))
        {
            graphicsSelected.SetActive(true);
        }
        else
        {
            graphicsSelected.SetActive(false);
        }

        if(targetManager.currentHold == entityObject)
        {
            graphicsHolding.SetActive(true);
        }
        else
        {
            graphicsHolding.SetActive(false);
        }
    }

    // Update is called once per frame
    
}
