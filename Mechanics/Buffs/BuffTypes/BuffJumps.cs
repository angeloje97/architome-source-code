using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
public class BuffJumps : MonoBehaviour
{
    // Start is called before the first frame update
    public BuffInfo buffInfo;
    public bool expired = false;
    public bool jumpsWhenTimerRunsOut;
    void GetDependencies()
    {
        if(GetComponent<BuffInfo>())
        {
            buffInfo = GetComponent<BuffInfo>();
            buffInfo.OnBuffCompletion += OnBuffCompletion;
        }
    }
    void Start()
    {
        GetDependencies();
        expired = false;
    }

    // Update is called once per frame

    public void OnBuffCompletion(BuffInfo buff)
    {
        if (!jumpsWhenTimerRunsOut) { return; }

        if (NewTarget().GetComponent<EntityInfo>())
        {
            //buffInfo.buffTimeComplete = false;
            NewTarget().GetComponent<EntityInfo>().Buffs().ApplyBuff(gameObject, buffInfo.sourceInfo, buffInfo.sourceAbility, buffInfo.sourceCatalyst);
        }
    }

    public GameObject NewTarget()
    {
        Collider[] entitiesInRange = Physics.OverlapSphere(transform.position, buffInfo.properties.radius, buffInfo.sourceAbility.targetLayer);
        List<Transform> potentialTargets = new List<Transform>();
        LayerMask obstructionLayer = buffInfo.sourceAbility.obstructionLayer;

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
