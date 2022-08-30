using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
public class BuffJumps : BuffType
{
    // Start is called before the first frame update
    
    public bool expired = false;
    public bool jumpsWhenTimerRunsOut;
    public int maxJumpTargets;
    new void GetDependencies()
    {
        base.GetDependencies();

        if (buffInfo)
        {
            buffInfo.OnBuffCompletion += OnBuffCompletion;
        }
    }
    void Start()
    {
        GetDependencies();
        expired = false;
    }


    public override string Description()
    {
        var additive = buffInfo.buffTargetType == BuffTargetType.Assist ? "enemy" : "ally";
        return $"Once the buff timer is complete, the buff will be applied to an {additive} that's within a {buffInfo.properties.radius} meter radius";
    }

    public override string GeneralDescription()
    {
        return "Will be applied to a new target within a the radius when the buff timer is complete";
    }

    public void OnBuffCompletion(BuffInfo buff)
    {
        if (!jumpsWhenTimerRunsOut) { return; }
        HandleJump();


        //if (NewTarget().GetComponent<EntityInfo>())
        //{
        //    //buffInfo.buffTimeComplete = false;
        //    NewTarget().GetComponent<EntityInfo>().Buffs().ApplyBuff(new(gameObject, buffInfo.sourceAbility));
        //}
    }

    public void HandleJump()
    {
        var entitiesInRange = Physics.OverlapSphere(transform.position, buffInfo.properties.radius, GMHelper.LayerMasks().entityLayerMask);

        var obstructionLayer = GMHelper.LayerMasks().structureLayerMask;

        var sourceInfo = buffInfo.sourceInfo;
        var targetType = buffInfo.buffTargetType;
        int count = 0;
        
        foreach (var entity in entitiesInRange)
        {
            if (count == maxJumpTargets) break;
            var info = entity.GetComponent<EntityInfo>();

            if (info == null) continue;

            var distance = Vector3.Distance(entity.transform.position, transform.position);
            var direction = V3Helper.Direction(entity.transform.position, transform.position);

            if (sourceInfo && targetType == BuffTargetType.Harm && !sourceInfo.CanAttack(info.gameObject)) continue;
            if (sourceInfo && targetType == BuffTargetType.Assist && !sourceInfo.CanHelp(info.gameObject)) continue;
            if (Physics.Raycast(transform.position, direction, distance, obstructionLayer)) continue;


            info.Buffs().ApplyBuff(new(gameObject, buffInfo.sourceAbility));
        }

    }

    public GameObject NewTarget()
    {
        Collider[] entitiesInRange = Physics.OverlapSphere(transform.position, buffInfo.properties.radius, GMHelper.LayerMasks().entityLayerMask);
        List<Transform> potentialTargets = new List<Transform>();
        LayerMask obstructionLayer = GMHelper.LayerMasks().structureLayerMask;

        for (int i = 0; i < entitiesInRange.Length; i++)
        {
            var currentTarget = entitiesInRange[i];
            if(currentTarget.GetComponent<EntityInfo>() == buffInfo.hostInfo) { continue; }
            if(currentTarget.GetComponent<EntityInfo>())
            {
                
                if(CanApply(currentTarget.GetComponent<EntityInfo>()))
                {
                    var direction = V3Helper.Direction(currentTarget.transform.position, transform.position);
                    var distance = V3Helper.Distance(currentTarget.transform.position, transform.position);

                    if(!Physics.Raycast(transform.position, direction, distance, obstructionLayer))
                    {
                        potentialTargets.Add(currentTarget.transform);
                    }

                }
            }
        }
        if (V3Helper.ClosestObject(transform, potentialTargets))
        {
            
            return V3Helper.ClosestObject(transform, potentialTargets).gameObject;
        }

        return null;

        bool CanApply(EntityInfo entity)
        {
            if(CanAttack(entity) || CanAssist(entity))
            {
                return true;
            }
            return false;
        }

        bool CanAttack(EntityInfo entity)
        {
            if (buffInfo.sourceInfo.CanAttack(entity.gameObject) && buffInfo.buffTargetType == BuffTargetType.Harm)
            {
                return true;
            }
            return false;
        }
        bool CanAssist(EntityInfo entity)
        {
            if(buffInfo.sourceInfo.CanHelp(entity.gameObject) && buffInfo.buffTargetType == BuffTargetType.Assist)
            {
                return true;
            }

            return false;
        }


        
    }
}
