using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
public class CatalystHit : MonoBehaviour
{
    // Start is called before the first frame update

    public AbilityInfo abilityInfo;
    public CatalystInfo catalystInfo;


    public bool isHealing;
    public bool isHarming;
    public bool isAssisting;

    public bool canSelfCast;
    public bool isSplashing = false;

    public float value;

    public float tankThreatMultiplier = 4;
    public void GetDependencies()
    {
        if(catalystInfo == null)
        {
            if(gameObject.GetComponent<CatalystInfo>())
            {
                catalystInfo = gameObject.GetComponent<CatalystInfo>();
                catalystInfo.OnCloseToTarget += OnCloseToTarget;
            }
        }

        if(abilityInfo == null)
        {
            if (catalystInfo && catalystInfo.abilityInfo)
            {
                abilityInfo = catalystInfo.abilityInfo;


                isHealing = abilityInfo.isHealing;
                isHarming = abilityInfo.isHarming;
                isAssisting = abilityInfo.isAssisting;
                canSelfCast = abilityInfo.canCastSelf;
                value = catalystInfo.value;
            }
        }

    }
    void Start()
    {
        GetDependencies();
    }
    // Update is called once per frame
    void Update()
    {
        GetDependencies();
        HandleSelfCastLock();
    }

    private void OnValidate()
    {
        if(GetComponent<CatalystInfo>())
        {
            catalystInfo = GetComponent<CatalystInfo>();
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        Debugger.InConsole(5123, $"Your projectile hit an object of {other}");
        if (!other.CompareTag("Entity") || other.GetComponent<EntityInfo>() == null)
        {
            return;
        }

        if (!CorrectLockOn(other)) { return; }

        EntityInfo targetHit = other.GetComponent<EntityInfo>();

        try
        {
            HandleTargetHit(targetHit);
            HandleSplash(targetHit);
        }
        catch
        {
            Debugger.InConsole(1094, $"{targetHit}, {catalystInfo.entityInfo}, {abilityInfo}");
        }
        
    }

    public bool IsDeadTargetable(EntityInfo target)
    {
        if(!target.isAlive)
        {
            if (abilityInfo && abilityInfo.targetsDead)
            { 
                return true; 
            }
        }
        else
        {
            if(abilityInfo && !abilityInfo.targetsDead)
            {
                return true;
            }
        }

        
        return false;
    }

    public void OnCloseToTarget(CatalystInfo catalyst, GameObject target)
    {
        HandleTargetHit(target.GetComponent<EntityInfo>());
    }

    public void HandleTargetHit(EntityInfo targetHit)
    {
        if (!IsDeadTargetable(targetHit)) { return; }
        if(catalystInfo.Ticks() <= 0 && !isSplashing) { return; }
        HandleMainTarget(targetHit.gameObject);
        HandleEvent();
        HandleHeal();
        HandleDamage();
        HandleAssist();


        void HandleHeal()
        {
            if (!CanHeal(targetHit)) { return; }
            if (targetHit.gameObject == catalystInfo.entityObject)
            {
                if (!canSelfCast)
                {
                    return;
                }
            }

            if (catalystInfo.entityInfo.CanHelp(targetHit.gameObject))
            {
                var combatData = new CombatEventData(catalystInfo, catalystInfo.entityInfo, value);
                targetHit.Heal(combatData);
                catalystInfo.OnHeal?.Invoke(targetHit.gameObject);
                AddAlliesHealed(targetHit);
                if(isHealing && !isAssisting)
                {
                    if (!isSplashing) { catalystInfo.ReduceTicks(); }
                }
                
            }

        }
        void HandleDamage()
        {
            if(!CanHarm(targetHit)) { return; }

            if (catalystInfo.entityInfo.CanAttack(targetHit.gameObject))
            {
                var combatData= new CombatEventData(catalystInfo, catalystInfo.entityInfo, value);
                targetHit.Damage(combatData);
                catalystInfo.OnDamage?.Invoke(targetHit.gameObject);
                ApplyBuff(targetHit, BuffTargetType.Harm);
                AddEnemyHit(targetHit);
                if (!isSplashing) { catalystInfo.ReduceTicks(); }
            }
        }
        void HandleAssist()
        {
            if (!CanAssist(targetHit))
            {
                return;
            }

            if (targetHit.gameObject == catalystInfo.entityObject)
            {
                if (!canSelfCast)
                {
                    return;
                }
            }

            if (catalystInfo.entityInfo.CanHelp(targetHit.gameObject))
            { 
                
                //Change these instructions to that of assisting instead of healing
                ApplyBuff(targetHit, BuffTargetType.Assist);
                catalystInfo.OnAssist?.Invoke(targetHit.gameObject);
                AddAlliesAssisted(targetHit);
                AddAlliesHealed(targetHit);

                if(!isSplashing) {
                    catalystInfo.ReduceTicks();
                }
                

            }
        }
        void HandleEvent()
        {
            if (CanHit(targetHit))
            {
                catalystInfo.OnHit?.Invoke(targetHit.gameObject);
            }
        }
    }

    public void HandleMainTarget(GameObject target)
    {
        if(catalystInfo.target != target) { return; }
        if(abilityInfo.isHarming && abilityInfo.isHealing && abilityInfo.isAssisting) { return; }

        if(abilityInfo.isHarming && !abilityInfo.isAssisting && !abilityInfo.isHealing)
        {
            if(!catalystInfo.entityInfo.CanAttack(target))
            {
                catalystInfo.OnWrongTargetHit?.Invoke(catalystInfo, target);
            }
        }

        if((abilityInfo.isAssisting || abilityInfo.isHealing) && !abilityInfo.isHarming)
        {
            if(!catalystInfo.entityInfo.CanHelp(target))
            {
                catalystInfo.OnWrongTargetHit?.Invoke(catalystInfo, target);
            }
        }
    }
    public bool CorrectLockOn(Collider targetCol)
    {
        if (catalystInfo && !catalystInfo.requiresLockOnTarget)
        {
            return true;
        }

        if (catalystInfo && catalystInfo.target == targetCol.gameObject)
        {
            return true;
        }

        if (abilityInfo && abilityInfo.canBeIntercepted && targetCol.gameObject.CompareTag("Entity"))
        {
            return true;
        }

        return false;
    }
    public bool CanHarm(EntityInfo targetInfo)
    {
        if (!isHarming){ return false;}
        if (!catalystInfo.entityInfo.CanAttack(targetInfo.gameObject)) { return false; }
        if (!EnemiesHitContains(targetInfo) || isSplashing)
        {
            return true;
        }

        if (EnemiesHitContains(targetInfo) && abilityInfo && abilityInfo.canHitSameTarget)
        {
            return true;
        }

        return false;
    }
    public bool CanHeal(EntityInfo targetInfo)
    {
        if(!isHealing) { return false; }
        if (!catalystInfo.entityInfo.CanHelp(targetInfo.gameObject)) { return false; }
        if (!HealedContains(targetInfo) || isSplashing)
        {
            return true;
        }

        if(HealedContains(targetInfo) && abilityInfo && abilityInfo.canHitSameTarget)
        {
            return true;
        }

        return false;
    }
    public bool CanAssist(EntityInfo targetInfo)
    {
        if(!isAssisting) { return false; }
        if (!catalystInfo.entityInfo.CanHelp(targetInfo.gameObject)) { return false; }
        if (!AssistContains(targetInfo))
        {
            return true;
        }

        if (AssistContains(targetInfo) && abilityInfo && abilityInfo.canAssistSameTarget)
        {
            Debugger.InConsole(9418, $"Can assist same target");
            return true;
        }

        return false;
    }
    public bool CanHit(EntityInfo targetInfo)
    {
        if(targetInfo == catalystInfo.entityInfo && !canSelfCast)
        {
            return false;
        }

        if(CanHeal(targetInfo) || CanHarm(targetInfo) || CanAssist(targetInfo))
        {
            return true;
        }

        return false;
    }
    public void AddEnemyHit(EntityInfo target)
    {
        if(isSplashing) { return; }
        if(catalystInfo)
        {
            if(!catalystInfo.enemiesHit.Contains(target))
            {
                catalystInfo.enemiesHit.Add(target);
            }
        }
    }
    public void AddAlliesHealed(EntityInfo target)
    {
        if (isSplashing) { return; }
        if (catalystInfo)
        {
            if(!catalystInfo.alliesHealed.Contains(target))
            {
                catalystInfo.alliesHealed.Add(target);
            }
        }
    }
    public void AddAlliesAssisted(EntityInfo target)
    {
        if (isSplashing) { return; }
        if (catalystInfo)
        {
            if (!catalystInfo.alliesAssisted.Contains(target))
            {
                catalystInfo.alliesAssisted.Add(target);
            }
        }
    }
    public bool Contains(EntityInfo target)
    {
        foreach(EntityInfo check in catalystInfo.enemiesHit)
        {
            if (target == check)
            {
                return true;
            }
        }

        foreach (EntityInfo check in catalystInfo.alliesHealed)
        {
            if(target == check)
            {
                return true;
            }
        }
        return false;
    }
    public bool EnemiesHitContains(EntityInfo target)
    {
        foreach (EntityInfo check in catalystInfo.enemiesHit)
        {
            if (target == check)
            {
                return true;
            }
        }

        return false;
    }
    public bool HealedContains(EntityInfo target)
    {
        foreach (EntityInfo check in catalystInfo.alliesHealed)
        {
            if (target == check)
            {
                return true;
            }
        }
        return false;
    }
    public bool AssistContains(EntityInfo target)
    {
        foreach(EntityInfo check in catalystInfo.alliesAssisted)
        {
            if(target == check)
            {
                return true;
            }
        }

        return false;
    }
    public void HandleSelfCastLock()
    {
        if (catalystInfo == null) return;
        if (abilityInfo == null) return;
        if (abilityInfo.entityInfo == null) return;

        if (catalystInfo.target != catalystInfo.entityObject)
        {
            return;
        }
        else
        {
            if (canSelfCast)
            {
                if (V3Helper.Distance(gameObject.transform.position, abilityInfo.entityObject.transform.position) < .1f)
                {
                    HandleTargetHit(catalystInfo.entityInfo);
                }
            }
        }
    }
    public void ApplyBuff(EntityInfo targetInfo, BuffTargetType buffType)
    {
        if(catalystInfo.entityInfo == null || abilityInfo == null) { return; }
        if (abilityInfo && abilityInfo.buffs.Count > 0)
        {
            foreach (GameObject buff in abilityInfo.buffs)
            {
                if (buff.GetComponent<BuffInfo>() && buff.GetComponent<BuffInfo>().buffTargetType == buffType)
                {
                    if(targetInfo.Buffs())
                    {
                        targetInfo.Buffs().ApplyBuff(buff, catalystInfo.entityInfo, abilityInfo, catalystInfo);
                    }
                }
            }
        }
    }
    public void HandleSplash(EntityInfo targetInfo)
    {
        if(targetInfo == null) { return; }
        if(abilityInfo == null) { return; }
        if (!abilityInfo.splashes) { return; }


        //Collider[] entitiesWithinRange = Physics.OverlapSphere(targetInfo.transform.position, abilityInfo.splashRadius, abilityInfo.targetLayer);

        var entitiesWithinRange = catalystInfo.EntitiesWithinRadius(abilityInfo.splashRadius);
        int maxSplashTargets = abilityInfo.maxSplashTargets;
        var originalValue = value;
        var originalAssist = isAssisting;

        HandleOriginalValues();
        HandleSplashTargets();
        ResetToOriginalValues();

        void HandleOriginalValues()
        {
            value = abilityInfo.valueContributionToSplash * value;
            isAssisting = abilityInfo.splashAppliesBuffs;
            isSplashing = true;
        }
        void HandleSplashTargets()
        {
            foreach (var target in entitiesWithinRange)
            {
                if (maxSplashTargets == 0)
                {
                    break;
                }
                else
                {
                    if (target.GetComponent<EntityInfo>().npcType == targetInfo.npcType && target.GetComponent<EntityInfo>() != targetInfo)
                    {
                        if (HasLineOfSight(target))
                        {
                            HandleTargetHit(target.GetComponent<EntityInfo>());
                            maxSplashTargets--;
                        }
                    }
                }
            }

            foreach (var target in entitiesWithinRange)
            {
                if (maxSplashTargets == 0)
                {
                    break;
                }
                if (target.GetComponent<EntityInfo>())
                {
                    if (CanHit(target.GetComponent<EntityInfo>()) && target.GetComponent<EntityInfo>() != targetInfo)
                    {
                        if (HasLineOfSight(target))
                        {
                            HandleTargetHit(target.GetComponent<EntityInfo>());
                        }
                        maxSplashTargets--;
                    }
                }
            }
        }
        void ResetToOriginalValues()
        {
            value = originalValue;
            isAssisting = originalAssist;
            isSplashing = false;
        }

        bool HasLineOfSight(GameObject target)
        {
            if(!abilityInfo.splashRequiresLOS)
            {
                return true;
            }
            var distance = V3Helper.Distance(target.transform.position, transform.position);
            var direction = V3Helper.Direction(target.transform.position, transform.position);

            if(!Physics.Raycast(transform.position, direction, distance, abilityInfo.obstructionLayer))
            {
                return true;
            }

            return false;
        }
    }
}
