using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
using System.Threading;
public class CatalystHit : MonoBehaviour
{
    // Start is called before the first frame update

    public AbilityInfo abilityInfo;
    public CatalystInfo catalystInfo;


    public bool isHealing;
    public bool isHarming;
    public bool isAssisting;

    public bool canSelfCast;
    public bool appliesBuff = true;
    public bool splashing { get; set; }

    public float value { get { return catalystInfo.value; } }

    public float tankThreatMultiplier = 4;
    public void GetDependencies()
    {
        if(GetComponent<CatalystInfo>())
        {
            catalystInfo = GetComponent<CatalystInfo>();
            catalystInfo.OnCloseToTarget += OnCloseToTarget;
        }

        if (catalystInfo && catalystInfo.abilityInfo)
        {
            abilityInfo = catalystInfo.abilityInfo;


            isHealing = abilityInfo.isHealing;
            isHarming = abilityInfo.isHarming;
            isAssisting = abilityInfo.isAssisting;
            canSelfCast = abilityInfo.canCastSelf;
            //value = catalystInfo.value;
        }

    }
    void Start()
    {
        GetDependencies();
    }
    // Update is called once per frame
    void Update()
    {
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
        //Debugger.InConsole(5123, $"Your projectile hit an object of {other}");
        //if (!other.CompareTag("Entity") || other.GetComponent<EntityInfo>() == null)
        //{
        //    return;
        //}

        var info = other.GetComponent<EntityInfo>();
        if (info == null) return;

        if (!CorrectLockOn(other)) { return; }


        try
        {
            HandleTargetHit(info);
        }
        catch
        {
            //Debugger.InConsole(1094, $"{targetHit}, {catalystInfo.entityInfo}, {abilityInfo}");
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

    public void HandleTargetHit(EntityInfo targetHit, bool forceHit = false)
    {
        if (!forceHit)
        {
            if (!IsDeadTargetable(targetHit)) { return; }
            if (catalystInfo.Ticks() == 0) return;
            if (catalystInfo.isDestroyed) return;
        }

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
                catalystInfo.OnHeal?.Invoke(catalystInfo ,targetHit);
                AddAlliesHealed(targetHit);
                if(isHealing && !isAssisting)
                {
                    catalystInfo.ReduceTicks();
                }
                
            }

        }

        

        void HandleDamage()
        {
            if(!CanHarm(targetHit)) { return; }

            if (catalystInfo.entityInfo.CanAttack(targetHit.gameObject))
            {
                var combatData = new CombatEventData(catalystInfo, catalystInfo.entityInfo, value);
                targetHit.Damage(combatData);
                catalystInfo.OnDamage?.Invoke(catalystInfo, targetHit);
                ApplyBuff(targetHit, BuffTargetType.Harm);
                AddEnemyHit(targetHit);
                catalystInfo.ReduceTicks();
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
                catalystInfo.OnAssist?.Invoke(catalystInfo, targetHit);
                AddAlliesAssisted(targetHit);
                AddAlliesHealed(targetHit);

                catalystInfo.ReduceTicks();
                

            }
        }
        void HandleEvent()
        {
            if (splashing) return;
            if (CanHit(targetHit))
            {
                ArchAction.Yield(() => {
                    catalystInfo.lastTargetHit = targetHit;
                    catalystInfo.OnHit?.Invoke(catalystInfo, targetHit);
                });
                
            }
        }

        void HandleDestroySummons()
        {
            if (!abilityInfo.destroysSummons) return;
            if (!IsSummon(targetHit.gameObject)) return;

            var combatData = new CombatEventData(catalystInfo, catalystInfo.entityInfo, targetHit.maxHealth);
            targetHit.Damage(combatData);
            catalystInfo.OnDamage?.Invoke(catalystInfo, targetHit);
            AddEnemyHit(targetHit);
            catalystInfo.ReduceTicks();
            targetHit.Die();
        }
    }

    public bool IsSummon(GameObject target)
    {
        var info = target.GetComponent<EntityInfo>();

        if (info == null) return false;
        if (!info.summon.isSummoned) return false;
        if (info.summon.master == null) return false;
        if (info.summon.master != catalystInfo.entityInfo) return false;

        return true;
    }

    public void HandleMainTarget(GameObject target)
    {
        if(catalystInfo.target != target) { return; }
        if (abilityInfo.abilityType != AbilityType.LockOn) return;
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

        

        if (abilityInfo && abilityInfo.canBeIntercepted && targetCol.GetComponent<EntityInfo>())
        {
            if (abilityInfo.entityInfo != targetCol.GetComponent<EntityInfo>())
            {
                catalystInfo.OnIntercept?.Invoke(targetCol.gameObject);
            }
            return true;
        }

        return false;
    }
    public bool CanHarm(EntityInfo targetInfo)
    {
        if (!isHarming){ return false;}
        if (!catalystInfo.entityInfo.CanAttack(targetInfo.gameObject)) { return false; }
        if (!EnemiesHitContains(targetInfo))
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
        if (!HealedContains(targetInfo))
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
        if (targetInfo == null) return false;
        //if (catalystInfo.Ticks() == 0) return false; //Let's take this piece of code out of the way and see what happens :)

        if(targetInfo == catalystInfo.entityInfo && !canSelfCast)
        {
            return false;
        }

        if (!targetInfo.isAlive && abilityInfo && !abilityInfo.targetsDead)
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
        
        if(catalystInfo)
        {
            if (abilityInfo.canHitSameTarget) return;
            if(!catalystInfo.enemiesHit.Contains(target))
            {
                catalystInfo.enemiesHit.Add(target);
            }
        }
    }
    public void AddAlliesHealed(EntityInfo target)
    {
        if (catalystInfo)
        {
            if (abilityInfo.canHitSameTarget) return;
            if(!catalystInfo.alliesHealed.Contains(target))
            {
                catalystInfo.alliesHealed.Add(target);
            }
        }
    }
    public void AddAlliesAssisted(EntityInfo target)
    {
        if (catalystInfo)
        {
            if (abilityInfo.canAssistSameTarget) return;
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
        if (!appliesBuff) return;
        if (abilityInfo && abilityInfo.buffs.Count > 0)
        {
            foreach (GameObject buff in abilityInfo.buffs)
            {
                if (buff.GetComponent<BuffInfo>() && (buff.GetComponent<BuffInfo>().buffTargetType == buffType || buff.GetComponent<BuffInfo>().buffTargetType == BuffTargetType.Neutral))
                {
                    if(targetInfo.Buffs())
                    {
                        //targetInfo.Buffs().ApplyBuff(buff, catalystInfo.entityInfo, abilityInfo, catalystInfo);
                        targetInfo.Buffs().ApplyBuff(new(buff, catalystInfo));
                    }
                }
            }


        }
    }
}
