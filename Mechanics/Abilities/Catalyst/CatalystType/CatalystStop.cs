using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;

public class CatalystStop : MonoBehaviour
{
    // Start is called before the first frame update
    CatalystInfo catalystInfo;
    AbilityInfo abilityInfo;
    CatalystHit catalystHit;

    public GameObject catalying;
    public List<EntityInfo> targets;
    public EntityInfo nextTarget;
    public int targetIndex;

    public LayerMask targetLayerMask;
    public LayerMask obstructionLayerMask;

    public float startSpeed;
    public float deceleration;
    public float distance;

    public bool stopped;

    


    void GetDependencies()
    {
        if(GetComponent<CatalystInfo>())
        {
            catalystInfo = GetComponent<CatalystInfo>();
            catalystHit = GetComponent<CatalystHit>();
            if (catalystInfo.abilityInfo)
            {
                abilityInfo = catalystInfo.abilityInfo;
                targetLayerMask = abilityInfo.targetLayer;
                obstructionLayerMask = abilityInfo.obstructionLayer;
            }

            if (abilityInfo && abilityInfo.cataling)
            {
                catalying = abilityInfo.cataling;
            }
        }
    }

    void DetermineDeceleration()
    {
        if(catalystInfo.target)
        {
            distance = V3Helper.Distance(catalystInfo.target.transform.position, transform.position);
        }
        else
        {
            distance = V3Helper.Distance(catalystInfo.location, transform.position);
        }

        if(distance > catalystInfo.range)
        {
            distance = catalystInfo.range;
        }

        deceleration = -((abilityInfo.speed * abilityInfo.speed) / (2*(distance)));
    }
    void Start()
    {
        GetDependencies();
        DetermineDeceleration();
        
        if (catalying) 
        {
            targets = new List<EntityInfo>();
            StartCoroutine(FindTargetsWithInRange()); 
        }
    }
    // Update is called once per frame
    void Update()
    {
        UpdateSpeed();
    }
    void UpdateSpeed()
    {
        if(catalystInfo.speed > 0)
        {
            catalystInfo.speed += deceleration*Time.deltaTime;
        }
        if(catalystInfo.speed < 0)
        {
            catalystInfo.speed = 0;
            HandleCatalyngs();
            stopped = true;
        }
        
    }
    void HandleCatalyngs()
    {
        if(!GetComponent<CatalingActions>()) { return; }
        var catalingActions = GetComponent<CatalingActions>();

        if(abilityInfo && abilityInfo.radiateCatalyngsOnStop)
        {
            if(abilityInfo.catalingAbilityType == AbilityType.LockOn)
            {
                StartCoroutine(Cycle());
            }
            else if(abilityInfo.catalingAbilityType == AbilityType.SkillShot)
            {

            }
        }

        IEnumerator Cycle()
        {
            
            while(catalystInfo.Ticks() > 0)
            {
                yield return new WaitForSeconds(abilityInfo.radiateIntervals);

                HandleLockOnCatalings();
            }

            void HandleLockOnCatalings()
            {
                if(abilityInfo.catalingAbilityType != AbilityType.LockOn) { return; }
                
                nextTarget = NextTarget();
                if (nextTarget != null)
                {
                    catalingActions.CastAt(nextTarget);
                    catalystInfo.ReduceTicks();
                }
            }
            //void HandleFreeShotCatalings()
            //{

            //}
        }

        

        EntityInfo NextTarget()
        {
            if (!ContainsTargets()) { return null; }
            if (targets.Count == 0) { return null; }
            if (targetIndex >= targets.Count) { targetIndex = 0; }
            for(int i = targetIndex; i < targets.Count; i++)
            {
                if(catalystHit.CanHit(targets[i]))
                {
                    targetIndex = i + 1;
                    return targets[i];
                }
            }
            targetIndex = 0;
            return NextTarget();
        }
    }
    IEnumerator FindTargetsWithInRange()
    {
        while (true)
        {
            yield return new WaitForSeconds(.25f);
            var range = catalying.GetComponent<CatalystInfo>().range;

            Collider[] entityInRange = Physics.OverlapSphere(gameObject.transform.position, range*2, abilityInfo.targetLayer);

            foreach (Collider collide in entityInRange)
            {
                var isObstructed = V3Helper.IsObstructed(collide.transform.position, gameObject.transform.position, GMHelper.LayerMasks().structureLayerMask);
                
                if(collide.GetComponent<EntityInfo>() && !targets.Contains(collide.GetComponent<EntityInfo>()))
                {
                    if (collide.GetComponent<EntityInfo>().isAlive &&
                        !isObstructed)
                    {
                        targets.Add(collide.GetComponent<EntityInfo>());
                    }
                }
            }

            RemoveOutOfRange(range);
            RemoveDead();
        }

        void RemoveOutOfRange(float catalyingRange)
        {
            foreach(EntityInfo target in targets)
            {
                if(V3Helper.Distance(target.transform.position, transform.position) > catalyingRange)
                {
                    targets.Remove(target);
                    RemoveOutOfRange(catalyingRange);
                    return;
                }
            }
        }

        void RemoveDead()
        {
            foreach(EntityInfo target in targets)
            {
                if(!target.isAlive)
                {
                    targets.Remove(target);
                    RemoveDead();
                    return;
                }
            }
        }
    }
    public bool ContainsTargets()
    {
        foreach (EntityInfo target in targets)
        {
            if(catalystHit.CanHit(target))
            {
                return true;
            }
        }

        return false;
    }
}
