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

    [SerializeField]
    [Header("User Key Bindings")]
    public string actionButton = "Action";
    public string selectMultiple = "SelectMultiple";

    [Header("Ability Bindings")]
    public string ability1 = "Ability1";
    public string ability2 = "Ability2";
    public string ability3 = "Ability3";
    public string ability4 = "Ability4";

    public void GetDependencies()
    {
        if (entityObject == null && transform.parent.gameObject.CompareTag("Entity"))
        {
            entityObject = transform.parent.gameObject;
        }
        if(entityObject && entityInfo == null)
        {
            entityInfo = Entity.EntityInfo(transform.parent.gameObject);
        }
        if(movement == null)
        {    
            if(entityInfo && entityInfo.Movement())
            {
                movement = entityInfo.Movement();
            }
        }

        if(abilityManager == null)
        {
            if(entityInfo && entityInfo.AbilityManager())
            {
                abilityManager = entityInfo.AbilityManager();
            }
        }

        if(targetManager == null)
        {
            if(GMHelper.TargetManager())
            {
                targetManager = GMHelper.TargetManager();
            }
        }

        if(behavior == null)
        {
            if(entityInfo && entityInfo.AIBehavior())
            {
                behavior = entityInfo.AIBehavior();
            }
        }
        
    }
    void Start()
    {
        if(GMHelper.KeyBindings())
        {
            keyBindings = GMHelper.KeyBindings();
        }
    }
    // Update is called once per frame
    void Update()
    {
        GetDependencies();
        HandleEntityControl();
        HandlePartyControl();
    }
    public void HandleEntityControl()
    {
        if(entityInfo.entityControlType != EntityControlType.EntityControl)
        {
            return;
        }

        HandleUserInputs();

        void HandleUserInputs()
        {
            if(Input.GetKeyDown(keyBindings.keyBinds[actionButton]))
            {
                HandleActionButton();
            }
            if (Input.GetKeyDown(keyBindings.keyBinds[ability1]))
            {
                HandlePlayerTargetting();
                Cast(0); 
            }
            if (Input.GetKeyDown(keyBindings.keyBinds[ability2])) 
            {
                HandlePlayerTargetting();
                Cast(1); 
            }
            if (Input.GetKeyDown(keyBindings.keyBinds[ability3]))
            {
                HandlePlayerTargetting();
                Cast(2); 
            }
            if (Input.GetKeyDown(keyBindings.keyBinds[ability4])) 
            {
                HandlePlayerTargetting();
                Cast(3); 
            }

        }
    }
    public void HandlePartyControl()
    {
        if(entityInfo.entityControlType != EntityControlType.PartyControl)
        {
            return;
        }

        if(targetManager)
        {
            if(!targetManager.selectedTargets.Contains(entityObject))
            {
                return;
            }  
        }

        if(Input.GetKey(keyBindings.keyBinds[selectMultiple])) { return; }

        HandleUserInputs();

        void HandleUserInputs()
        {
            if (Input.GetKeyDown(keyBindings.keyBinds[actionButton]))
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
                var location = Mouse.CurrentPositionLayer(entityInfo.walkableLayer);
                location.y = entityObject.transform.position.y;

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
