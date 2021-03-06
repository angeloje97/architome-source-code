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

        targetManager = ContainerTargetables.active;

        if (GMHelper.KeyBindings())
        {
            keyBindings = GMHelper.KeyBindings();
        }

        if (ArchInput.active)
        {
            ArchInput.active.OnAction += OnAction;
        }

        
    }
    void Start()
    {
        GetDependencies();
    }
    // Update is called once per frame
    void Update()
    {
        
        //HandleEntityControl();
        //HandlePartyControl();
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

    void MoveTo(Vector3 location)
    {
        if (movement)
        {
            movement.MoveTo(location);
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
            behavior.CombatBehavior().SetFocus(targetManager.currentHover);
        }

        abilityManager.Attack();
    }
    public void HandlePlayerTargetting ()
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
                GameObject target;
                if (targetManager.currentHover)
                {
                    target = targetManager.currentHover;
                }
                else if(targetManager.selectedTargets.Count > 0)
                {
                    target = targetManager.selectedTargets[0];
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
            if(!entityInfo)
            {
                return;
            }
            if(abilityManager)
            {
                var layer = GMHelper.LayerMasks().walkableLayer;
                var location = Mouse.CurrentPositionLayer(layer);

                Debugger.Combat(1040, $"Original Location: {location}");

                var heightFromGround = V3Helper.HeightFromGround(entityObject.transform.position, layer);

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


                abilityManager.location = location;
            }
        }
    }
    public void HandleActionButton(bool isFromPartyControl = false)
    {
        var currentObject = Mouse.CurrentHoverObject();

        if (currentObject && currentObject.GetComponent<Clickable>() && !Mouse.IsMouseOverUI())
        {
            if (!isFromPartyControl) return;
            currentObject.GetComponent<Clickable>().Click(entityInfo);
        }
        else if (targetManager.currentHover)
        {
            abilityManager.target = targetManager.currentHover;
            Attack();
            if (entityInfo.AIBehavior().behaviorType == AIBehaviorType.HalfPlayerControl)
            {
                entityInfo.CombatBehavior().SetFocus(targetManager.currentHover);

            }
        }
        else
        {
            if (Mouse.IsMouseOverUI()) { return; }
            var location = Mouse.CurrentPositionLayer(entityInfo.walkableLayer);
            if (location == new Vector3(0, 0, 0)) { return; }
            MoveTo(location);
        }
    }
}
