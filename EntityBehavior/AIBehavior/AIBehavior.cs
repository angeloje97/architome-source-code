using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
using System;
using System.Linq;
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
    new public void GetDependencies()
    {
        base.GetDependencies();
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

            entityInfo.combatEvents.OnStatesChange += OnStatesChange;

            if (entityInfo.npcType == NPCType.Hostile)
            {
                combatType = CombatBehaviorType.Aggressive;
            }

            entityInfo.partyEvents.OnAddedToParty += (PartyInfo party) => {
                behaviorType = AIBehaviorType.HalfPlayerControl;
            };
        }

        StartCoroutine(DependenciesLate());


        IEnumerator DependenciesLate()
        {
            yield return new WaitForSeconds(.51f);

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


    public void OnStatesChange(List<EntityState> previous, List<EntityState> states)
    {
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




    void Start()
    {
        GetDependencies();
        stateManager.Activate(this);
    }
    void Update()
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
    public LineOfSight LineOfSight()
    {
        return GetComponentInChildren<LineOfSight>();
    }
    public ThreatManager ThreatManager()
    {
        return GetComponentInChildren<ThreatManager>();
    }

    public T CreateBehavior<T>(string name = "New Behavior") where T: MonoBehaviour
    {
        var newBehaviorObject = new GameObject(name);

        newBehaviorObject.transform.SetParent(transform);
        newBehaviorObject.transform.localPosition = new Vector3();

        return newBehaviorObject.AddComponent<T>();


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
}
