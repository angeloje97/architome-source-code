using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject entityObject;

    public EntityInfo entityInfo;
    public Movement movement;
    public AbilityManager abilityManager;
    public ContainerTargetables targetManager;
    public KeyBindings keyBindings;
    public AIBehavior behavior;


    LayerMask walkableLayer;

    public Action<EntityInfo, AbilityInfo> OnPlayerTargetting;
    public void GetDependencies()
    {
        
        entityInfo = GetComponentInParent<EntityInfo>();

        if (entityInfo)
        {
            entityObject = entityInfo.gameObject;
            movement = entityInfo.Movement();
            abilityManager = entityInfo.AbilityManager();
            behavior = entityInfo.AIBehavior();

            entityInfo.sceneEvents.OnTransferScene += OnTransferScene;
        }

        var layerMasksData = LayerMasksData.active;

        if (layerMasksData)
        {
            walkableLayer = layerMasksData.walkableLayer;
        }
        targetManager = ContainerTargetables.active;

        if (GMHelper.KeyBindings())
        {
            keyBindings = GMHelper.KeyBindings();
        }
    }
    void Start()
    {
        GetDependencies();
    }
    void Update()
    {
        
    }
    
    public void OnTransferScene(string sceneName)
    {
        targetManager = ContainerTargetables.active;
        keyBindings = GMHelper.KeyBindings();
    }

    public void OnAction()
    {
        if (entityInfo.entityControlType == EntityControlType.EntityControl)
        {
            HandleActionButton();
        }

        if (entityInfo.entityControlType == EntityControlType.PartyControl)
        {
            if (targetManager.selectedTargets.Contains(entityObject))
            {
                HandleActionButton();
            }
        }
    }

    async void MoveTo(Vector3 location)
    {
        if (movement)
        {
            await movement.MoveToAsync(location);
        }
    }
    public void Cast(int value)
    {
        if(!abilityManager)
        {
            return;
        }
        Debugger.InConsole(1114, $"Your code made it this far");

        abilityManager.Cast(value);
    }
    public void Attack()
    {
        if(!abilityManager)
        {
            return;
        }

        if(behavior && targetManager.currentHover)
        {
            var hoverInfo = targetManager.currentHover.GetComponent<EntityInfo>();
            behavior.CombatBehavior().SetFocus(hoverInfo);
        }

        abilityManager.Attack();
    }
    public void HandlePlayerTargetting(AbilityInfo ability = null)
    {
        HandleTargetting();
        HandleDirection();

        void HandleTargetting()
        {
            if(!entityInfo)
            {
                return;
            }

            if(abilityManager)
            {
                EntityInfo target;
                if (targetManager.currentHover)
                {
                    target = targetManager.currentHover.GetComponent<EntityInfo>();
                }
                else if(targetManager.selectedTargets.Count > 0)
                {
                    target = targetManager.selectedTargets[0].GetComponent<EntityInfo>();
                    OnPlayerTargetting?.Invoke(target, ability);
                }
                else
                {
                    target = null;
                }
                

                abilityManager.target = target;


            }
        }

        void HandleDirection()
        {

            abilityManager.location = RelativeMouseLocation();
        }
    }

    public Vector3 RelativeMouseLocation()
    {
        var location = Mouse.CurrentPositionLayer(walkableLayer);

        Debugger.Combat(1040, $"Original Location: {location}");

        var heightFromGround = V3Helper.HeightFromGround(entityObject.transform.position, walkableLayer);

        Debugger.Combat(1041, $"Height from ground: {heightFromGround}");

        if (location == new Vector3(0, 0, 0))
        {
            location = Mouse.RelativePosition(entityInfo.transform.position);
            Debugger.Combat(1041, $"Relative Location: {location}");
            location.y = entityInfo.transform.position.y;
        }
        else
        {
            location.y += heightFromGround;

        }

        return location;
    }

    public void HandleActionButton(bool isFromPartyControl = false)
    {
        var currentObject = Mouse.CurrentHoverObject();
        
        if (currentObject && currentObject.GetComponent<Clickable>() && !Mouse.IsMouseOverUI())
        {
            if (!isFromPartyControl) return;
            currentObject.GetComponent<Clickable>().Click(entityInfo);
            return;
        }

        var currentTarget = targetManager.CurrentTarget();
        if (currentTarget)
        {
            abilityManager.target = currentTarget;
            Attack();
            if (entityInfo.AIBehavior().behaviorType == AIBehaviorType.HalfPlayerControl)
            {
                entityInfo.CombatBehavior().SetFocus(currentTarget);

            }
        }
        else
        {
            if (Mouse.IsMouseOverUI()) { return; }
            var location = Mouse.CurrentPositionLayer(entityInfo.walkableLayer);
            if (location == new Vector3(0, 0, 0)) { return; }
            MoveTo(location);
            if (isFromPartyControl)
            {
                movement.Invoke(new(eMovementEvent.OnQuickMove, movement, movement.Target()));
            }
        }
    }
}
