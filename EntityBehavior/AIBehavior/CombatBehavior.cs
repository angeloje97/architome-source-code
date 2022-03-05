using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
using System;

[Serializable]
public class SpecialAbility
{
    public int abilityIndex;
    public bool targetsRandom;
    public List<Role> randomTargetWhiteList;
    public List<Role> randomTargetBlackList;
}

[RequireComponent(typeof(CombatInfo))]

public class CombatBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject entityObject;
    public EntityInfo entityInfo;
    public AIBehavior behavior;

    public ThreatManager threatManager;
    public AbilityManager abilityManager;
    public Movement movement;


    [SerializeField]
    private GameObject focusTarget;
    public GameObject target;


    public int[] abilityIndexPriority;
    public List<SpecialAbility> specialAbilities;

    

    //Private fields

    //Events
    public event Action<GameObject, GameObject> OnNewTarget;

    //Event Triggers
    public GameObject previousTarget;

    public void GetDependencies()
    {
        if(GetComponentInParent<EntityInfo>())
        {
            entityInfo = GetComponentInParent<EntityInfo>();
            entityObject = entityInfo.gameObject;
            entityInfo.OnLifeChange += OnLifeChange; 

            if(entityInfo.Movement())
            {
                movement = entityInfo.Movement();
            }

            if(entityInfo.AbilityManager())
            {
                abilityManager = entityInfo.AbilityManager();
            }

        }

        if(GetComponentInParent<AIBehavior>())
        {
            behavior = GetComponentInParent<AIBehavior>();

            if(behavior.ThreatManager())
            {
                threatManager = behavior.ThreatManager();
                threatManager.OnIncreaseThreat += OnIncreaseThreat;
                threatManager.OnRemoveThreat += OnRemoveThreat;
                threatManager.OnEmptyThreats += OnEmptyThreats;
            }
        }
    }
    void Start()
    {
        GetDependencies();
        if (threatManager) { StartCoroutine(CombatRoutine()); }
    }


    public void OnLifeChange(bool isAlive)
    {
        if(isAlive) { return; }
        target = null;
        focusTarget = null;
    }

    public void OnTryCast(AbilityInfo ability)
    {
        if(ability.target && !entityInfo.CanAttack(ability.target.gameObject))
        {
            if(!abilityManager.attackAbility.isHealing)
            {
                SetFocus(null);
            }
        }
    }
    public void SetFocus(GameObject target)
    {
        focusTarget = target;
    }

    public GameObject GetFocus()
    {
        return focusTarget;
    }

    public void OnIncreaseThreat(ThreatManager.ThreatInfo threatInfo)
    {
        target = threatManager.highestThreat;
    }
    public void OnRemoveThreat(ThreatManager.ThreatInfo threatInfo)
    {
        if(target == threatInfo.threatObject || focusTarget == threatInfo.threatObject)
        {
            target = threatManager.highestThreat;
        }
    }
    public void OnEmptyThreats(ThreatManager threatManager)
    {
    }
    void Update()
    {
        HandleEvents();
    }
    void HandleEvents()
    {
        HandleOnNewTarget();

        void HandleOnNewTarget()
        {
            if (focusTarget != null)
            {
                if(previousTarget != focusTarget)
                {
                    OnNewTarget?.Invoke(previousTarget, focusTarget);
                    previousTarget = focusTarget;
                }


                return;
            }

            if (previousTarget != target)
            {
                OnNewTarget?.Invoke(previousTarget, target);
                previousTarget = target;
            }
        }
        
    }
    IEnumerator CombatRoutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(.125f);
            if(!entityInfo.isAlive){continue;}
            HandleThreat();
            HandleNoControl();
            HandleHalfControl();
            HandleDeadTarget();
        }
    }
    public void HandleThreat()
    {
        if (behavior.behaviorType == AIBehaviorType.HalfPlayerControl || behavior.behaviorType == AIBehaviorType.NoControl)
        {
            if(threatManager.threats.Count == 0) { return; }
            if (behavior.ThreatManager().highestThreat)
            {
                target = behavior.ThreatManager().highestThreat;
            }


            if (focusTarget != null)
            {
                //abilityManager.target = focusTarget;
                StartCoroutine(HandleNoCombatFocus());
            }
            //else
            //{
            //    abilityManager.target = target;
            //}


            //if (target && !target.GetComponent<EntityInfo>().isAlive)
            //{
            //    target = null;
            //    abilityManager.target = null;
            //}
        }
        
        

    }
    public void HandleNoControl()
    {
        if (behavior.behaviorType != AIBehaviorType.NoControl) { return; }
        if (!entityInfo.isInCombat) { return; }
        HandleNoControlDamage();

        void HandleNoControlDamage()
        {
            
            abilityManager.target = focusTarget ? focusTarget : target;
            for (int i = 0; i < specialAbilities.Count; i++)
            {
                var specialAbility = specialAbilities[i];

                if (specialAbility.randomTargetWhiteList.Count > 0)
                {

                }
                else if (specialAbility.randomTargetBlackList.Count > 0)
                {
                    if (abilityManager.Ability(specialAbility.abilityIndex) && abilityManager.Ability(specialAbility.abilityIndex).IsReady())
                    {
                        var target = threatManager.RandomTargetBlackList(specialAbility.randomTargetBlackList);
                        if(target)
                        {
                            abilityManager.target = target;
                            abilityManager.Cast(specialAbility.abilityIndex);
                            return;
                        }
                    }
                }
            }

            for (int i = 0; i < abilityIndexPriority.Length; i++)
            {
                try
                {
                    if (abilityManager.Ability(abilityIndexPriority[i]).IsReady() && abilityManager.target)
                    {
                        abilityManager.Cast(abilityIndexPriority[i]);
                        return;
                    }
                }
                catch (Exception) { throw; }
            }

            if (abilityManager)
            {
                if (abilityManager.target)
                {
                    abilityManager.Attack();
                }
            }
        }
    }
    public void HandleHalfControl()
    {
        if (behavior.behaviorType != AIBehaviorType.HalfPlayerControl)
        {
            return;
        }

        if(behavior.combatType == CombatBehaviorType.Passive && focusTarget == null) { return; }
        if(behavior.behaviorState != BehaviorState.Idle) { return; }


        if(HandleAbilities()) { return; }
        if(HandleAutoHeal()) { return; }
        HandleAutoAttack();
        
        bool HandleAbilities()
        {
            if(focusTarget == null) { return false; }
            var focusInfo = focusTarget.GetComponent<EntityInfo>();

            if(HandleHarmAbility()) { return true; }

            return false;

            bool HandleHarmAbility()
            {

                if(abilityIndexPriority.Length == 0) { return false; }
                foreach (int index in abilityIndexPriority)
                {
                    if (abilityManager.Ability(index).isHarming &&
                        abilityManager.canAttack.Contains(focusInfo.npcType) &&
                        abilityManager.Ability(index).IsReady())
                    {
                        abilityManager.target = focusTarget;
                        abilityManager.Cast(index);
                        abilityManager.target = null;
                        return true;
                    }
                }
                return false;
            }
        }
        void HandleAutoAttack()
        {
            if(!abilityManager.attackAbility.isHarming) { return; }


            if(focusTarget && focusTarget.GetComponent<EntityInfo>() && focusTarget.GetComponent<EntityInfo>().npcType != entityInfo.npcType)
            {
                abilityManager.target = focusTarget;
                abilityManager.Attack();
                abilityManager.target = null;
            }

            else if(target)
            {
                Debugger.InConsole(7953, $"{target != null}");
                if(behavior.combatType == CombatBehaviorType.Reactive)
                {
                    if (V3Helper.Distance(target.transform.position, entityObject.transform.position) < abilityManager.attackAbility.range)
                    {
                        if(behavior.LineOfSight().HasLineOfSight(target))
                        {
                            abilityManager.target = target;
                            abilityManager.Attack();
                            abilityManager.target = null;

                        }
                    }
                    else
                    {
                        if (threatManager.NearestHighestThreat(abilityManager.attackAbility.range))
                        {
                            abilityManager.target = threatManager.NearestHighestThreat(abilityManager.attackAbility.range);
                            abilityManager.Attack();
                            abilityManager.target = null;
                        }
                    }
                }

                if(behavior.combatType == CombatBehaviorType.Proactive || behavior.combatType == CombatBehaviorType.Agressive)
                {
                    abilityManager.target = target;
                    abilityManager.Attack();
                    abilityManager.target = null;
                }
            }
        }

        bool HandleAutoHeal()
        {
            if(abilityManager.attackAbility.isHealing)
            {
                
                if(focusTarget && focusTarget.GetComponent<EntityInfo>() && focusTarget.GetComponent<EntityInfo>().npcType == entityInfo.npcType)
                {
                    var focusInfo = focusTarget.GetComponent<EntityInfo>();

                    abilityManager.target = focusTarget;
                    abilityManager.Attack();
                    return true;
                }
            }
            return false;
        }
    }
    public void HandleDeadTarget()
    {
        if (focusTarget != null && focusTarget && focusTarget.GetComponent<EntityInfo>())
        {
            if (!focusTarget.GetComponent<EntityInfo>().isAlive)
            {
                focusTarget = null;
                if (abilityManager && abilityManager.attackAbility && abilityManager.attackAbility.target)
                {
                    if (!abilityManager.attackAbility.target.GetComponent<EntityInfo>().isAlive)
                    {
                        abilityManager.attackAbility.target = null;
                        abilityManager.attackAbility.isAutoAttacking = false;
                        abilityManager.attackAbility.DeactivateWantsToCast();
                    }
                }
            }
        }

        if(target != null && target.GetComponent<EntityInfo>() && !target.GetComponent<EntityInfo>().isAlive)
        {

            if (abilityManager && abilityManager.attackAbility && abilityManager.attackAbility.target)
            {
                if (!abilityManager.attackAbility.target.GetComponent<EntityInfo>().isAlive)
                {
                    if(threatManager)
                    {
                        threatManager.HandleMaxThreat();

                        if (threatManager && threatManager.highestThreat)
                        {

                            target = threatManager.highestThreat;
                        }
                        else
                        {
                            target = null;
                        }
                    }
                    
                }
            }
        }

        
    }
    public IEnumerator HandleNoCombatFocus()
    {
        while (!entityInfo.isInCombat && focusTarget != null)
        {
            yield return new WaitForSeconds(5f);
            if (target == null)
            {
                focusTarget = null;
            }
        }
    }

    // Update is called once per frame


    public CombatInfo CombatInfo()
    {
        if(GetComponent<CombatInfo>())
        {
            return GetComponent<CombatInfo>();
        }

        return null;
    }
}
