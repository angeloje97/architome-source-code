using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
using System;
using System.Linq;
using System.Threading.Tasks;

public class AIBehavior : EntityProp
{
    // Start is called before the first frame update
    public GameObject entityObject;
    //public AbilityManager abilityManager;
    public Movement movement;

    public AIBehaviorType behaviorType;
    public BehaviorState behaviorState;
    public CombatBehaviorType combatType;

    public bool isPlayer;

    public struct Events
    {
        public Action<EntityInfo> OnDetecedEntity { get; set; }
        public Action<EntityInfo> OnDetectedEnemy { get; set; }
        public Action<EntityInfo> OnSightedEntity { get; set; }

    }

    public Events events;

    public Action<AIBehaviorType, AIBehaviorType> OnBehaviorChange;
    public Action<BehaviorState, BehaviorState> OnBehaviorStateChange { get; set; }
    public Action<CombatBehaviorType, CombatBehaviorType> OnCombatBehaviorTypeChange;

    public BehaviorStateManager stateManager;
    private AIBehaviorType originalBehaviorType;
    private AIBehaviorType behaviorTypeCheck;
    private BehaviorState behaviorStateCheck;
    private CombatBehaviorType combatTypeCheck;
    public override void GetDependencies()
    {
        if (entityInfo)
        {
            entityObject = entityInfo.gameObject;

            movement = entityInfo.Movement();

            if (entityInfo.Movement())
            {
                movement = entityInfo.Movement();
            }

            if (entityInfo.AbilityManager())
            {
                var abilityManager = entityInfo.AbilityManager();
            }

            combatEvents.AddListenerStateEvent(eStateEvent.OnStatesChange, OnStatesChange, this);

            if (entityInfo.npcType == NPCType.Hostile)
            {
                combatType = CombatBehaviorType.Aggressive;
            }

            entityInfo.partyEvents.OnAddedToParty += (PartyInfo party) => {
                behaviorType = AIBehaviorType.HalfPlayerControl;
            };
        }
        stateManager.Activate(this);

        DependenciesLate();
        



        async void DependenciesLate()
        {
            await Task.Delay(500);

            if(GMHelper.GameManager().playableEntities.Contains(entityInfo))
            {
                isPlayer = true;
            }

            if(isPlayer)
            {
                if (GetComponentInParent<PartyInfo>() && 
                    GMHelper.GameManager().playableParties.Contains(GetComponentInParent<PartyInfo>()))
                {
                    behaviorType = AIBehaviorType.HalfPlayerControl;
                }
                else
                {
                    behaviorType = AIBehaviorType.FullControl;
                }
            }
            else
            {
                behaviorType = AIBehaviorType.NoControl;
            }

            originalBehaviorType = behaviorType;
        }
    }


    public void OnStatesChange(StateChangeEvent stateEventData)
    {
        var states = stateEventData.afterEventStates;

        var effectedBy = new List<EntityState>()
        {
            EntityState.Taunted 
        };

        var intersection = states.Intersect(effectedBy).ToList();

        if(intersection.Count > 0)
        {
            behaviorType = AIBehaviorType.NoControl;
        }
        else
        {
            behaviorType = originalBehaviorType;
        }
    }

    public override void EUpdate()
    {
        HandleEventTriggers();
    }
    public void HandleEventTriggers()
    {
        if(behaviorType != behaviorTypeCheck)
        {
            OnBehaviorChange?.Invoke(behaviorTypeCheck, behaviorType);
            behaviorTypeCheck = behaviorType;
        }

        if(behaviorStateCheck != behaviorState)
        {
            OnBehaviorStateChange?.Invoke(behaviorStateCheck, behaviorState);
            behaviorStateCheck = behaviorState;
        }

        if(combatTypeCheck != combatType)
        {
            OnCombatBehaviorTypeChange?.Invoke(combatTypeCheck, combatType);
            combatTypeCheck = combatType;
        }
    }
    public T CreateBehavior<T>(string name = "New Behavior") where T: MonoBehaviour
    {
        var newBehaviorObject = new GameObject(name);

        newBehaviorObject.transform.SetParent(transform);
        newBehaviorObject.transform.localPosition = new Vector3();

        return newBehaviorObject.AddComponent<T>();


    }

    #region Properties
    public LineOfSight LineOfSight()
    {
        return GetComponentInChildren<LineOfSight>();
    }
    public ThreatManager ThreatManager()
    {
        return GetComponentInChildren<ThreatManager>();
    }

    public CombatBehavior CombatBehavior()
    {
        foreach(Transform child in transform)
        {
            if (child.GetComponent<CombatBehavior>())
            {
                return child.GetComponent<CombatBehavior>();
            }
        }
        return null;
    }
    public PatrolBehavior PatrolBehavior()
    {
        foreach(Transform child in transform)
        {
            if (child.GetComponent<PatrolBehavior>())
            {
                return child.GetComponent<PatrolBehavior>();
            }
        }
        return null;
    }
    public NoCombatBehavior NoCombatBehavior()
    {
        foreach(Transform child in transform)
        {
            if(child.GetComponent<NoCombatBehavior>())
            {
                return child.GetComponent<NoCombatBehavior>();
            }
        }
        return null;
    }
    #endregion
}
