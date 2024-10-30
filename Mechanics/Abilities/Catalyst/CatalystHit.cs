using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
using System.Threading;

public class CatalystHit : MonoBehaviour
{
    // Start is called before the first frame update

    public AbilityInfo abilityInfo
    {
        get
        {
            return catalystInfo.abilityInfo;
        }
    }
    public CatalystInfo catalystInfo;


    public bool isHealing { get; set; }
    public bool isHarming { get; set; }
    public bool isAssisting { get; set; }

    public bool canSelfCast;
    public bool appliesBuff = true;
    public bool splashing { get; set; }

    public bool forceHit { get; set; }
    public bool forceHeal { get; set; }
    public bool forceAssist { get; set; }
    public bool forceHarm { get; set; }

    public float value { get { return catalystInfo.value; } }

    public float tankThreatMultiplier = 4;
    public void GetDependencies()
    {
        catalystInfo = GetComponent<CatalystInfo>();

        if (catalystInfo)
        {
            catalystInfo.OnCloseToTarget += OnCloseToTarget;
            catalystInfo.OnEntityTrigger += OnEntityTrigger;
        }

        if (abilityInfo)
        {


            isHealing = abilityInfo.isHealing;
            isHarming = abilityInfo.isHarming;
            isAssisting = abilityInfo.isAssisting;
            canSelfCast = abilityInfo.restrictionHandler.restrictions.canCastSelf;
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

    void OnEntityTrigger(CatalystInfo catalyst, EntityInfo info)
    {
        if (info == null) return;

        if (!CorrectLockOn(info)) { return; }


        try
        {
            HandleTargetHit(info);
        }
        catch
        {
            //Debugger.InConsole(1094, $"{targetHit}, {catalystInfo.entityInfo}, {abilityInfo}");
        }
    }


    public void OnCloseToTarget(CatalystInfo catalyst, GameObject target)
    {
        HandleTargetHit(target.GetComponent<EntityInfo>());
    }

    public void HandleTargetHit(EntityInfo targetHit, bool forceHit = false)
    {
        var hit = false;
        if (!forceHit)
        {
            if (catalystInfo.Ticks() == 0) return;
            if (catalystInfo.isDestroyed) return;
        }

        if (!CanHit(targetHit, true)) return;

        HandleMainTarget(targetHit);
        HandleEvent();
        HandleHeal();
        HandleDamage();
        HandleAssist();

        if (hit)
        {
            catalystInfo.ReduceTicks();
        }
        

        void HandleHeal()
        {
            if (!CanHeal(targetHit)) { return; }

            var healthEvent = new HealthEvent(catalystInfo, catalystInfo.entityInfo, value);
            targetHit.Heal(healthEvent);
            catalystInfo.OnHeal?.Invoke(catalystInfo ,targetHit);
            AddAlliesHealed(targetHit);

            hit = true;

        }

        

        void HandleDamage()
        {
            if(!CanHarm(targetHit)) { return; }

            var combatData = new HealthEvent(catalystInfo, catalystInfo.entityInfo, value);
            targetHit.Damage(combatData);
            catalystInfo.OnDamage?.Invoke(catalystInfo, targetHit);
            ApplyBuff(targetHit, BuffTargetType.Harm);
            AddEnemyHit(targetHit);
            hit = true;
        }
        void HandleAssist()
        {
            if (!CanAssist(targetHit))
            {
                return;
            }

                
            ApplyBuff(targetHit, BuffTargetType.Assist);
            catalystInfo.OnAssist?.Invoke(catalystInfo, targetHit);
            AddAlliesAssisted(targetHit);


            hit = true;
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

    public void HandleMainTarget(EntityInfo target)
    {
        if (catalystInfo.target != target) { return; }
        if (abilityInfo.abilityType != AbilityType.LockOn) return;
        if (abilityInfo.isHarming && abilityInfo.isHealing && abilityInfo.isAssisting) { return; }

        if (abilityInfo.isHarming && !abilityInfo.isAssisting && !abilityInfo.isHealing)
        {
            if (!catalystInfo.entityInfo.CanAttack(target))
            {
                catalystInfo.OnWrongTargetHit?.Invoke(catalystInfo, target.gameObject);
            }
        }

        if ((abilityInfo.isAssisting || abilityInfo.isHealing) && !abilityInfo.isHarming)
        {
            if (!catalystInfo.entityInfo.CanHelp(target))
            {
                catalystInfo.OnWrongTargetHit?.Invoke(catalystInfo, target.gameObject);
            }
        }
        
    }
    public bool CorrectLockOn(EntityInfo entity)
    {
        var checks = new List<bool>();

        catalystInfo.OnCorrectLockOnCheck?.Invoke(this, entity, checks);

        foreach(var check in checks)
        {
            if (check) return true;
        }


        if (catalystInfo && !catalystInfo.requiresLockOnTarget)
        {
            return true;
        }

        if (catalystInfo && catalystInfo.target == entity.gameObject)
        {
            return true;
        }

        

        if (abilityInfo && abilityInfo.canBeIntercepted)
        {

            if (abilityInfo.entityInfo != entity)
            {
                catalystInfo.OnIntercept?.Invoke(entity.gameObject);
            }
            return true;
        }

        return false;
    }
    public bool CanHarm(EntityInfo targetInfo)
    {

        var checks = new List<bool>();

        catalystInfo.OnCanHarmCheck?.Invoke(this, targetInfo, checks);


        if (!forceHarm)
        {
            foreach (var check in checks)
            {
                if (!check) return false;
            }

        }


        return true;
    }
    public bool CanHeal(EntityInfo targetInfo)
    {

        var checks = new List<bool>();

        catalystInfo.OnCanHealCheck?.Invoke(this, targetInfo, checks);

        if (forceHeal) return true;

        
        foreach(var check in checks)
        {
            if (check) return true;
        }

        return false;
    }
    public bool CanAssist(EntityInfo targetInfo)
    {

        var checks = new List<bool>();

        catalystInfo.OnCanAssistCheck?.Invoke(this, targetInfo, checks);

        if (forceAssist) return true;
        
        foreach (var check in checks)
        {
            if (check) return true;
        }

        return false;
    }

    
    public bool CanHit(EntityInfo targetInfo, bool hitOnly = false)
    {
        if (targetInfo == null) return false;

        var checks = new List<bool>();

        catalystInfo.OnCanHitCheck?.Invoke(this, targetInfo, checks);


        if (!forceHit)
        {
            foreach(var check in checks)
            {
                if (!check) return false;
            }

        }

        if (hitOnly) return true;

        if (CanHeal(targetInfo)) return true;
        if (CanHarm(targetInfo)) return true;
        if (CanAssist(targetInfo)) return true;

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
                var buffInfo = buff.GetComponent<BuffInfo>();
                if (buffInfo && (buffInfo.buffTargetType == buffType || buffInfo.buffTargetType == BuffTargetType.Neutral))
                {
                    var buffManager = targetInfo.Buffs();

                    if (buffManager)
                    {
                        buffManager.ApplyBuff(new(buff, catalystInfo));
                    }
                }
            }


        }
    }
}
