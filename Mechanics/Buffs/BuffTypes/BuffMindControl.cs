using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;
using Architome.Enums;
public class BuffMindControl : BuffStateChanger
{
    // Start is called before the first frame update

    public AIBehaviorType originalBehaviorType;
    public NPCType originalNPCType;
    public AbilityManager abilityManager;
    public Movement movement;
    public CombatBehavior combatBehavior;
    public EntityInfo originalFocus;
    

    new void GetDependencies()
    {
        base.GetDependencies();
        if(!GetComponent<BuffInfo>()) return;

        buffInfo = GetComponent<BuffInfo>();
        buffInfo.OnBuffEnd += OnBuffEnd;

        abilityManager = buffInfo.hostInfo.AbilityManager();
        
        combatBehavior = buffInfo.hostInfo.CombatBehavior();
        
        ApplyBuff();
    }
    new void ApplyBuff()
    {
        var host = buffInfo.hostInfo;
        var source = buffInfo.sourceInfo;

        host.OnDamageDone += OnDamageDone;

        base.ApplyBuff();

        bool changedState = applied;
        applied = changedState;

        if (changedState)
        {
            CleanseTaunts();
            originalBehaviorType = host.AIBehavior().behaviorType;
            originalNPCType = host.npcType;
            host.ChangeBehavior(source.npcType, Entity.IsPlayer(source.gameObject) ? AIBehaviorType.HalfPlayerControl : AIBehaviorType.NoControl);
            host.Movement().StopMoving();

            movement = host.Movement();
            buffInfo.sourceAbility.recastProperties.OnRecast += OnRecast;

            if (combatBehavior.GetFocus() != null)
            {
                originalFocus = combatBehavior.GetFocus();
            }


        }
    }

    public override string Description()
    {

        
        var descriptions = new List<string>() {
            base.Description(),
            $"Will change the target's threat type to the same as the source's threat type."
        };


        if (buffInfo.sourceAbility)
        {
            if (buffInfo.sourceAbility.recastProperties.enabled)
            {
                descriptions.Add($"Can recast the source ability to move the target or have the target attack/heal another target.");
            }
        }

        return ArchString.NextLineList(descriptions);
    }

    public override string GeneralDescription()
    {
        return "Control an enemy to switch them over to the caster's side.";
    }

    void OnDamageDone(CombatEventData eventData)
    {
        buffInfo.sourceInfo.OnDamageDone?.Invoke(eventData);
    }

    void CleanseTaunts()
    {
        var buffsManager = GetComponentInParent<BuffsManager>();
        



        foreach (var buff in buffsManager.GetComponentsInChildren<BuffTaunt>())
        {
            buff.GetComponent<BuffInfo>().Cleanse();
        }
    }
    public void OnRecast(AbilityInfo ability)
    {


        var target = ContainerTargetables.active.currentHover;

        var currentObject = Mouse.CurrentHoverObject();

        if (currentObject && currentObject.GetComponent<Clickable>() && !Mouse.IsMouseOverUI())
        {
            currentObject.GetComponent<Clickable>().Click(buffInfo.hostInfo);
            return;
        }

        if (target != null)
        {
            var info = target.GetComponent<EntityInfo>();

            
            if (abilityManager.attackAbility.CanCastAt(target))
            {
                if (combatBehavior)
                {
                    combatBehavior.SetFocus(info);
                }
                abilityManager.target = info;
                abilityManager.Attack();
                abilityManager.target = null;
                return;
            
            }
        }
        MoveToMouseLocation();



    }

    public void MoveToMouseLocation()
    {
        if (movement == null) return;
        var layer = GMHelper.LayerMasks().walkableLayer;
        var location = Mouse.CurrentPositionLayer(layer);

        if (location == new Vector3(0, 0, 0)) return;

        _= movement.MoveToAsync(location);
    }

    void Start()
    {
        GetDependencies();
    }

    // Update is called once per frame
    public void OnBuffEnd(BuffInfo buffInfo)
    {
        if (!applied) return;
        HandleRemoveState(buffInfo);
        buffInfo.hostInfo.ChangeBehavior(originalNPCType, originalBehaviorType);
        buffInfo.hostInfo.OnDamageDone -= OnDamageDone;
        combatBehavior.SetFocus(originalFocus);

        ArchAction.Delay(() => 
        {
            if (buffInfo.sourceInfo.isAlive)
            {
                buffInfo.hostInfo.ThreatManager().IncreaseThreat(buffInfo.sourceInfo, 15);
            }
        }, .125f);

    }
}
