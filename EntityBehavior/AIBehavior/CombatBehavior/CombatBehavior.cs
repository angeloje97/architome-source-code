using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System.Threading.Tasks;
using System;

namespace Architome
{

    [Serializable]
    public class SpecialAbility
    {
        public AbilityInfo ability;
        //public int abilityIndex;
        //public bool targetsRandom;
        public SpecialTargeting targeting;
        public List<Role> randomTargetBlackList;
    }
    
    [Serializable]
    public class SpecialHealing : SpecialAbility
    {
        [Range(0, 1)]
        public float minimumHealth;
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
        private EntityInfo focusTarget;
        public EntityInfo target;


        public int[] abilityIndexPriority;
        public List<SpecialAbility> specialAbilities;

        [Serializable]
        public struct HealSettings
        {
            [Range(0, 1)]
            public float targetHealth;
            [Range(0, 1)]
            public float minMana;
            public bool clearFocusOnFullHealthTarget;
            public List<SpecialHealing> specialHealingAbilities;
        }

        public HealSettings healSettings;

        //Private fields

        //Events
        public Action<EntityInfo, EntityInfo, List<bool>> OnCanFocusCheck { get; set; }
        public  Action<EntityInfo, EntityInfo> OnNewTarget { get; set; }
        public Action<EntityInfo, EntityInfo> OnNewFocusTarget { get; set; }
        public event Action<EntityInfo> OnSetFocus;

        //Event Triggers
        EntityInfo previousTarget;
        EntityInfo previousFocusTarget;

        public float tryMoveTimer;

        AbilityManager.Events abilityEvents
        {
            get
            {
                return entityInfo.abilityEvents;
            }
        } 

        bool proactiveTank { get; set; }

        public void GetDependencies()
        {
            entityInfo = GetComponentInParent<EntityInfo>();
            behavior = GetComponentInParent<AIBehavior>();

            if (entityInfo)
            {
                entityObject = entityInfo.gameObject;
                entityInfo.OnLifeChange += OnLifeChange;
                entityInfo.OnCombatChange += OnCombatChange;
                entityInfo.OnHealingDone += OnHealingDone;

                



                movement = entityInfo.Movement();


                if (movement)
                {
                    movement.OnTryMove += OnTryMove;
                    movement.OnChangePath += OnChangePath;
                }

                abilityManager = entityInfo.AbilityManager();

                abilityEvents.OnCastRelease += OnCastRelease;

                //if (abilityManager)
                //{
                //    abilityManager.OnCastRelease += OnCastRelease;
                //}

                var playerController = entityInfo.PlayerController();
                if (playerController)
                {
                    playerController.OnPlayerTargetting += OnPlayerAbilityTargetting;
                }

                entityInfo.partyEvents.OnAddedToParty += delegate (PartyInfo party) {
                    party.events.OnPartyFocus += OnPartyFocus;

                };

                entityInfo.partyEvents.OnRemovedFromParty += delegate (PartyInfo party)
                {
                    party.events.OnPartyFocus -= OnPartyFocus;
                };
            }

            if (behavior)
            {
                behavior.OnCombatBehaviorTypeChange += OnCombatBehaviorTypeChange;
                behavior.OnBehaviorStateChange += OnBehaviorStateChange;

                if (behavior.ThreatManager())
                {
                    threatManager = behavior.ThreatManager();
                    threatManager.OnIncreaseThreat += OnIncreaseThreat;
                    threatManager.OnRemoveThreat += OnRemoveThreat;
                    threatManager.OnEmptyThreats += OnEmptyThreats;

                }
            }

            

            UpdateAggressiveTank();
        }
        void Start()
        {
            GetDependencies();
            if (threatManager) { StartCoroutine(CombatRoutine()); }
            HandleStartCombatStatus();

            
        }
        void Update()
        {
            HandleEvents();
            HandleTimers();
        }

        async void HandleStartCombatStatus()
        {
            var timer = 3f;

            while (timer > 0)
            {
                await Task.Delay(1000);
                timer -= 1f;

                if (entityInfo.isInCombat)
                {
                    OnCombatChange(entityInfo.isInCombat);
                    break;
                }
            }
        }

        public void OnLifeChange(bool isAlive)
        {
            if (isAlive) { return; }
            target = null;
            SetFocus(null);
        }

        void OnPartyFocus(EntityInfo target)
        {
            var attackAbility = abilityManager.attackAbility;

            if (!attackAbility.CanCastAt(target)) return;

            abilityManager.target = target;
            abilityManager.Attack();
            abilityManager.target = null;

            SetFocus(target, "Party Focus");
        }

        public void OnTryCast(AbilityInfo ability)
        {
            if (ability.target && !entityInfo.CanAttack(ability.target))
            {
                if (!abilityManager.attackAbility.isHealing)
                {
                    SetFocus(null, "Try Cast");
                }
            }
        }

        public void OnCastRelease(AbilityInfo ability)
        {
            if (behavior.behaviorType != AIBehaviorType.HalfPlayerControl) { return; }
            if (ability.isAttack) { return; }
            if (ability.abilityType2 == AbilityType2.Passive) return;


            if (entityInfo.CanAttack(ability.target) && abilityManager.attackAbility.isHarming)
            {
                SetFocus(ability.target.GetComponent<EntityInfo>(), $"Casted at {ability.target}");
            }
        }

        public void OnHealingDone(CombatEventData eventData)
        {
            if (entityInfo.role != Role.Healer) return;
            if (!healSettings.clearFocusOnFullHealthTarget) return;
            var target = eventData.target;

            Debugger.InConsole(3845, $"{target} health at {target.health / target.maxHealth}");
            if (target == null) return;

            var percentHealth = target.health / target.maxHealth;

            if (percentHealth >= 1)
            {
                SetFocus(null, "Target at full health");
            }


        }
        public void OnBehaviorStateChange(BehaviorState before, BehaviorState after)
        {
            ClearCombatBehaviors();
            CreateBehavior();
            
            //InitCombatActions();
        }

        public void OnPlayerAbilityTargetting(EntityInfo target, AbilityInfo ability)
        {
            if (target == null) return;
            if (ability == null) return;
            if (ability.isHarming && entityInfo.CanAttack(target))
            {
                SetFocus(target);
            }

            if (ability.isHealing && entityInfo.CanHelp(target))
            {
                SetFocus(target);
            }
        }

        public void OnCombatChange(bool isInCombat)
        {
            ClearCombatBehaviors();
            CreateBehavior();
        }

        public void OnCombatBehaviorTypeChange(CombatBehaviorType before, CombatBehaviorType after)
        {
            UpdateAggressiveTank();
        }

        void UpdateAggressiveTank()
        {
            if (entityInfo.role != Role.Tank)
            {
                proactiveTank = false; 
                return;
            }

            if ((int)behavior.combatType < 2)
            {
                proactiveTank = false; 
                return;
            }

            if (!Entity.IsPlayer(entityInfo))
            {
                proactiveTank = false;
                return;
            }

            proactiveTank = true;
        }

        public void CreateBehavior()
        {
            if (!entityInfo.isInCombat) { return; }
            CreateNoControl();
            CreateHalfControl();

            void CreateNoControl()
            {
                if (behavior.behaviorType != AIBehaviorType.NoControl) return;
                var noControl = new GameObject("Combat: No Control");
                noControl.transform.SetParent(transform);
                noControl.transform.localPosition = new();
                noControl.AddComponent<CombatNoControl>();

            }

            void CreateHalfControl()
            {
                if (behavior.behaviorType != AIBehaviorType.HalfPlayerControl) return;
                var halfControl = new GameObject("Combat: Half Control");
                halfControl.transform.SetParent(transform);
                halfControl.transform.localPosition = new();
                halfControl.AddComponent<CombatHalfControl>();
                
            }
        }

        
        void ClearCombatBehaviors()
        {
            foreach (var combatType in GetComponentsInChildren<CombatType>())
            {

                combatType.DestroySelf();
                //Destroy(combatType.gameObject);
            }
        }
        public void SetFocus(EntityInfo target, string reason = "", bool forceFocus = false)
        {

            var canFocus = CanFocus(target);

            Debugger.Combat(5439, $"Can focus: {canFocus}");

            if (!forceFocus && !canFocus) return;
            
         

            if (target == null)
            {
                Debugger.InConsole(8493, $"Focus = null because {reason}");
            }
            focusTarget = target;
            HandleFocus();
            if (target != null)
            {
                OnSetFocus?.Invoke(target);
            }


            async void HandleFocus()
            {
                if (target == null) return;
                var attackAbility = abilityManager.attackAbility;
                while(focusTarget == target)
                {
                    if(!CanUseAbilityOn(attackAbility, target))
                    {
                        SetFocus(null, "Can no longer use auto ability on target");
                        break;
                    }

                    if (!this) break;
                    await Task.Delay(500); 
                }
            }
        }
        public EntityInfo GetFocus()
        {
            return focusTarget;
        }

        public bool CanUseAbilityOn(AbilityInfo ability, EntityInfo target)
        {
            if (ability.CanHeal(target)) return true;
            if (ability.CanHarm(target)) return true;
            return false;
        }
        public bool CanFocus(EntityInfo target)
        {
            var setFocusChecks = new List<bool>();

            OnCanFocusCheck?.Invoke(entityInfo, target, setFocusChecks);


            foreach (var focusCheck in setFocusChecks)
            {
                if (!focusCheck) return false;
            }

            if (target == null) return true;


            var attackAbility = abilityManager.attackAbility;
            if (attackAbility == null) return false;
            if (entityInfo.CanAttack(target))
            {
                if (attackAbility.isHarming)
                {
                    return true;
                }
            }

            if (entityInfo.CanHelp(target))
            {
                if (attackAbility.isAssisting || attackAbility.isHealing)
                {
                    return true;
                }
            }

            return false;
        }
        public void OnIncreaseThreat(ThreatManager.ThreatInfo threatInfo, float value)
        {
            ArchAction.Yield(() => {
                target = CorrectTarget();

                if (behavior.behaviorType == AIBehaviorType.HalfPlayerControl && behavior.behaviorState != BehaviorState.Idle)
                {
                    return;
                }

                InitCombatActions();
            });
            

        }
        EntityInfo CorrectTarget()
        {
            if (proactiveTank && threatManager.highestNonTargettingThreat)
            {
                return threatManager.highestNonTargettingThreat;
            }

            return threatManager.highestThreat;
        }
        public void OnTryMove(Movement movement)
        {
            tryMoveTimer = 1f;
        }
        public void OnChangePath(Movement movement)
        {
            if (focusTarget && movement.Target() != focusTarget.transform)
            {
                SetFocus(null, "New Path Target");
            }
        }
        public void OnRemoveThreat(ThreatManager.ThreatInfo threatInfo)
        {
            target = CorrectTarget();

            if (focusTarget == threatInfo.threatInfo)
            {
                focusTarget = null;
            }
        }
        public void OnEmptyThreats(ThreatManager threatManager)
        {
        }
        void HandleEvents()
        {
            HandleOnNewTarget();

            void HandleOnNewTarget()
            {
                if (previousTarget != target)
                {
                    if (!focusTarget)
                    {
                        OnNewTarget?.Invoke(previousTarget, target);
                        entityInfo.SetCombatTarget(target);
                    }
                    previousTarget = target;
                }

                if (previousFocusTarget != focusTarget)
                {
                    var nextTarget = focusTarget ? focusTarget : target;

                    OnNewTarget?.Invoke(previousFocusTarget, focusTarget);
                    entityInfo.SetCombatTarget(nextTarget);
                    previousFocusTarget = focusTarget;
                }
            }

        }

        void HandleTimers()
        {
            if (tryMoveTimer > 0)
            {
                tryMoveTimer -= Time.deltaTime;
            }

            if (tryMoveTimer < 0)
            {
                tryMoveTimer = 0;
            }
        }
        IEnumerator CombatRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(.125f);
                if (tryMoveTimer > 0)
                {
                    yield return new WaitForSeconds(tryMoveTimer);
                    continue;
                }
                if (!entityInfo.isAlive) { continue; }
                InitCombatActions();
            }
        }
        public void InitCombatActions()
        {
            if (!entityInfo.isAlive) { return; }
            if (tryMoveTimer > 0) { return; }
        }
        public IEnumerator HandleNoCombatFocus()
        {
            while (!entityInfo.isInCombat && focusTarget != null)
            {
                yield return new WaitForSeconds(5f);
                if (target == null)
                {
                    //focusTarget = null;
                }
            }
        }

        

        // Update is called once per frame


        public CombatInfo CombatInfo()
        {
            if (GetComponent<CombatInfo>())
            {
                return GetComponent<CombatInfo>();
            }

            return null;
        }
    }
}
