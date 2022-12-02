using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;
using System.Threading.Tasks;
using System;

public class CurrentTargetManager : MonoBehaviour
{
    // Start is called before the first frame update
    public PortraitBehavior targetPortrait;
    public CanvasGroup portraitCanvasGroup;
    public GameManager gameManager;
    public ContainerTargetables targetManager;
    public KeyBindings binds;

    EntityInfo currentTarget;
    EntityInfo previousTarget;

    public Action<EntityInfo, EntityInfo> OnNewTarget;
    void GetDependencies()
    {
        binds = GMHelper.KeyBindings();
        if(GMHelper.GameManager() && GMHelper.TargetManager())
        {
            gameManager = GMHelper.GameManager();
            targetManager = GMHelper.TargetManager();
        }

        targetManager.OnSelectTarget += OnSelectTarget;
        targetManager.OnClearSelected += OnClearSelected;
        targetManager.OnClearFromEscape += OnClearFromEscape;

    }
    void Start()
    {
        GetDependencies();
    }

    // Update is called once per frame
    void Update()
    {
        HandleEvents();
    }

    void HandleEvents()
    {
        if(currentTarget != previousTarget)
        {
            OnCurrentTargetChange(previousTarget, currentTarget);
            previousTarget = currentTarget;

        }
    }

    void OnCurrentTargetChange(EntityInfo previous, EntityInfo newEntity)
    {
        if (newEntity)
        {
            targetPortrait.SetEntity(newEntity);

            if(newEntity == targetManager.currentHover)
            {
                targetPortrait.OnNewHoverTarget(null, newEntity);
            }

            newEntity.infoEvents.OnDestroy += HandleDestroy;
        }

        SetCanvas(newEntity != null);

        if (previous)
        {
            previous.infoEvents.OnDestroy -= HandleDestroy;
        }
    }

    void HandleDestroy(EntityInfo entity)
    {
        SetCanvas(false);
    }


    void OnSelectTarget(GameObject target)
    {
        if (target)
        {
            currentTarget = target.GetComponent<EntityInfo>();
        }
    }


    void SetCanvas(bool val)
    {
        ArchUI.SetCanvas(portraitCanvasGroup, val);
    }

    public void OnClearFromEscape()
    {
        SetCanvas(false);
    }

    public void OnClearSelected()
    {
        currentTarget = null;
    }



}
