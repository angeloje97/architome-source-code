using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;
public class CombatStatusHandler : MonoBehaviour
{
    // Start is called before the first frame update
    public EntityInfo entityInfo;
    public ThreatManager threatManager;
    public float combatTime = 10f;
    public float combatTimer;
    public bool combatTimerComplete;

    AbilityManager.Events abilityEvents
    {
        get
        {
            return entityInfo.abilityEvents;
        }
    }
    void GetDependencies()
    {
        entityInfo = GetComponentInParent<EntityInfo>();
        if(entityInfo)
        {
            entityInfo.ThreatManager().OnIncreaseThreat += OnIncreaseThreat;
            entityInfo.OnDamageDone += OnDamageDone;
            entityInfo.OnHealingDone += OnHealingDone;
            entityInfo.OnLifeChange += OnLifeChange;

            var abilityManager = entityInfo.AbilityManager();


            abilityEvents.OnCastRelease += OnCastRelease;

            if (abilityManager)
            {
                //abilityManager.OnCastRelease += OnCastRelease;
                abilityManager.OnAbilityStart += OnAbilityStart;
                abilityManager.OnWantsToCastChange += OnWantsToCastChange;
            }

            if(entityInfo.ThreatManager())
            {
                threatManager = entityInfo.ThreatManager();
                threatManager.OnRemoveThreat += OnRemoveThreat;
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
        HandleTimers();
        HandleEvents();
    }

    void HandleTimers()
    {
        
        if(!combatTimerComplete)
        {
            combatTimer -= Time.deltaTime;
        }
        
    }

    void HandleEvents()
    {
        if(combatTimerComplete != combatTimer <= 0)
        {
            combatTimerComplete = combatTimer <= 0;

            if(combatTimerComplete)
            {
                combatTimer = 0;
            }

            OnStateChangeTimer(combatTimerComplete);
        }
    }

    public void OnStateChangeTimer(bool isComplete)
    {
        if(isComplete)
        {
            SetCombatStatus(false);
        }
    }

    public void OnIncreaseThreat(ThreatManager.ThreatInfo threatInfo, float value)
    {
        SetCombatStatus(true);
    }

    public void OnRemoveThreat(ThreatManager.ThreatInfo threatInfo)
    {
        if (threatManager.threats.Count == 0)
        {
            SetCombatStatus(false);
        }

    }

    public void OnDamageDone(CombatEventData eventData)
    {
        if (threatManager.threats == null || threatManager.threats.Count == 0) return;
        SetCombatStatus(true);
    }

    public void OnCastRelease(AbilityInfo ability)
    {
        if(ability.targetLocked == null) { return; }
        if (!entityInfo.CanAttack(ability.targetLocked)) { return; }

        SetCombatStatus(true);
    }

    public void OnAbilityStart(AbilityInfo ability)
    {
        if (ability.target == null) return;
        if (!entityInfo.CanAttack(ability.target)) return;
        SetCombatStatus(true);
    }

    public void OnWantsToCastChange(AbilityInfo ability, bool wantsToCast)
    {
        if (!wantsToCast)
        {
            if (threatManager.threats.Count == 0)
            {
                SetCombatStatus(false);
            }
            return;
        }
        if (ability.target == null) return;
        if (!entityInfo.CanAttack(ability.target)) return;

        SetCombatStatus(true);
    }

    public void OnLifeChange(bool isAlive)
    {
        if(!isAlive)
        {
            SetCombatStatus(false);
        }
    }

    public void OnHealingDone(CombatEventData eventData)
    {
        SetCombatStatus(eventData.target.isInCombat);
    }

    public void SetCombatStatus(bool isInCombat)
    {
        if(entityInfo.isInCombat != isInCombat)
        {
            entityInfo.isInCombat = isInCombat;
        }

        combatTimer = isInCombat ? combatTime : 0;
    }

}
