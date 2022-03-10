using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
using UnityEngine.UI;
using System;

public class AbilityInfo : MonoBehaviour
{
    // Start is called before the first frame update

    [Header("Ability Information")]
    public string abilityName;
    [Multiline]
    public string abilityDescription;


    public GameObject entityObject;
    public GameObject catalyst;
    public GameObject cataling;
    public Sprite abilityIcon;

    public List<GameObject> buffs;
    public BuffProperties buffProperties;

    public AbilityManager abilityManager;
    public EntityInfo entityInfo;
    public Movement movement;
    public CatalystInfo catalystInfo;
    public LineOfSight lineOfSight;
    public AIBehavior behavior;

    public AbilityType abilityType;
    public AbilityType catalingAbilityType;
    public AbilityType2 abilityType2;
    public List<AbilityFunction> abilityFunctions;
    public LayerMask destructiveLayer;
    public LayerMask targetLayer;
    public LayerMask obstructionLayer;

    public GameObject target;
    public GameObject targetLocked;
    public Vector3 location;
    public Vector3 locationLocked;
    public Vector3 directionLocked;

    [Header("Catalyst Info")]
    public float speed = 35;
    public float range = 15;
    public float castTime = 1f;
    public CatalystType catalystType;

    [Header("Stats Contribution")]
    public float strengthContribution = 1f;
    public float wisdomContribution = 1f;
    public float dexterityContribution = 1f;

    [Header("Ability Properties")]

    public float globalCoolDown = 1f;
    public float knockBackValue = 1.0f;
    public float manaRequiredPercent = .05f;
    public float manaRequired = 25f;
    public float manaProduced = 0f;
    public float manaProducedPercent = 0f;
    public float baseValue = 5.0f;
    public float value;
    public float coolDown = 1f;
    public float originalCastTime = 1f;
    public float selfCastMultiplier = 1f;
    public int maxCharge = 1;
    public float liveTime = 3;
    public int ticksOfDamage = 1;
    public bool maxChargesOnStart = true;

    [Serializable]
    public class AbilityVisualEffects
    {
        public bool showCastBar;
        public bool showChannelBar;
    }

    public AbilityVisualEffects vfx;


    [Header("Catalyst Stop Property")]
    public bool catalystStops;
    public float decelleration;
    public bool splashesOnStop;
    public float maxSplashAmount;
    public bool radiateCatalyngsOnStop;
    public float radiateIntervals;
    public float valueContributionToCataling;

    [Header("Spalsh Properties")]
    public bool splashes;
    public GameObject splashParticleEffects;
    public bool splashRequiresLOS;
    public bool splashAppliesBuffs;
    public int maxSplashTargets;
    public float valueContributionToSplash;
    public float splashRadius;
    public float splashDelay;

    [Header("Scanner Properties")]
    public bool scannerRadialAoe;

    [Header("Grow Properties")]
    public bool grows;
    public bool growsForward;
    public bool growsWidth;

    [Header("ChannelProperties")]
    public float channelTime = 3;
    public float channelIntervals = 1f;
    public int channelInvokes = 0;
    public bool cancelChannelOnMove = false;
    public bool cantMoveWhenChanneling;
    public float channelMovementSpeedReduction;

    [Header("Recastable Propertiers")]
    public float canRecast;
    public float recasted;

    [Header("Return Properties")]
    public bool returnAppliesBuffs;
    public bool returnAppliesHeals;
    public bool returns;

    [Header("Bounce Properties")]
    public bool bounces;
    public float bounceRadius;
    public bool bounceRequiresLOS;

    [Header("Hold to Cast Properties")]
    public float holdToCastValue;

    [Header("Movement Casting Properties")]
    public float castingMovementSpeedReduction;
    public bool cancelCastIfMoved;
    public bool cantMoveWhenCasting;

    [Header("AbilityRestrictions")]
    public bool usesGlobalCoolDown;
    public bool playerAiming;
    public bool canCastSelf;
    public bool onlyCastSelf;
    public bool onlyCastOutOfCombat;
    public bool isHealing;
    public bool isAssisting;
    public bool isHarming;
    public bool targetsDead;
    public bool requiresLockOnTarget;
    public bool requiresLineOfSight;
    public bool canHitSameTarget;
    public bool canAssistSameTarget;
    public bool explosive;
    public bool canBeIntercepted;
    public bool nullifyDamage;
    public bool interruptable;
    public bool isAttack;
    public bool active;



    [Serializable]
    public class DestroyConditions
    {
        public bool destroyOnCollisions;
        public bool destroyOnStructure;
        public bool destroyOnNoTickDamage;
        public bool destroyOnReturn;
        public bool destroyOnOutOfRange;
        public bool destroyOnLiveTime;
        public bool destroyOnDeadTarget;
    }

    public DestroyConditions destroyConditions;

    [Serializable]
    public class RecastProperties
    {
        public bool recastable;
        public bool isActive;
        public int maxRecast;
        public int currentRecast;
        public float recastTimeFrame;
        public Action<AbilityInfo> OnRecast;

        public bool CanRecast()
        {
            if(currentRecast > 0 && isActive)
            {
                return true;
            }

            return false;
        }
    }


    public RecastProperties recastProperties;

    [Header("Ability Timers")]
    public float coolDownTimer;
    public float castTimer;
    public float currentCharges;
    public float globalCoolDownTimer;
    public bool globalCoolDownActive;
    public float channelTimer;
    public bool isChanneling;
    public bool isCasting;
    public float timerPercent;
    public bool timerPercentActivated;

    [Header("Lock On Behaviors")]
    private bool wantsToCast;
    public bool isAutoAttacking;
    public void GetDependencies()
    {

        if(abilityManager == null)
        {
            if(GetComponentInParent<AbilityManager>())
            {
                abilityManager = GetComponentInParent<AbilityManager>();
                abilityManager.OnTryCast += OnTryCast;
            }
        }
        if(abilityManager)
        {
            if(entityObject == null && abilityManager.entityObject)
            {
                entityObject = abilityManager.entityObject;
                entityInfo = abilityManager.entityInfo;
                entityInfo.OnChangeNPCType += OnChangeNPCType;
                
            }
        }

        if(entityInfo && entityInfo.Movement())
        {
            movement = entityInfo.Movement();
            movement.OnStartMove += OnStartMove;
            movement.OnEndMove += OnEndMove;
            movement.OnTryMove += OnTryMove;
            movement.OnNewPathTarget += OnNewPathTarget;
        }


        if(catalystInfo == null)
        {
            if(catalyst)
            {
                catalystInfo = catalyst.GetComponent<CatalystInfo>();
                if (catalystInfo.catalystIcon) { abilityIcon = catalystInfo.catalystIcon; }
            }
        }

        if(lineOfSight == null)
        {
            if(entityInfo && entityInfo.LineOfSight())
            {
                lineOfSight = entityInfo.LineOfSight();

                obstructionLayer = lineOfSight.obstructionLayer;
                targetLayer = lineOfSight.targetLayer;
            }
        }


        if(behavior == null && entityInfo.AIBehavior())
        {
            behavior = entityInfo.AIBehavior();
        }

        if(maxChargesOnStart)
        {
            currentCharges = maxCharge;
        }

        UpdateAbility();
    }
    void Start()
    {
        Invoke("GetDependencies", .125f);
    }
    void Update()
    {
        HandleTimers();
        HandleWantsToCast();
        HandleDeadTarget();
        HandleAutoAttack();
    }
    public void OnStartMove(Movement movement)
    {
        if (cancelCastIfMoved && isCasting)
        {
            CancelCast();
        }
    }
    public void OnEndMove(Movement movement)
    {

    }
    public void OnTryCast(AbilityInfo ability)
    {
        if (ability.isAttack) { return; }
        if(isAttack)
        {
            target = null;
            targetLocked = null;
            isAutoAttacking = false;


            if(isCasting)
            {
                CancelCast();
            }
        }
    }
    public void OnTryMove(Movement movement)
    {
        target = null;
        targetLocked = null;
    }
    public void OnChangeNPCType(NPCType before, NPCType after)
    {
        if(WantsToCast())
        {
            DeactivateWantsToCast();
            target = null;
            targetLocked = null;
        }
    }
    public void OnNewPathTarget(Movement movement, Transform before, Transform after)
    {
        if(target != null && after != target.transform)
        {
            if (wantsToCast)
            {
                DeactivateWantsToCast();
                
            }

            if(isAttack)
            {
                target = null;
                targetLocked = null;
                isAutoAttacking = false;
            }
        }
    }
    public void HandleTimers()
    {
        CoolDownTimer();
        CastTimer();
        ChannelTimer();
        GlobalCoolDownTimer();
        HandleCastTimerPercent();

        void CoolDownTimer()
        {
            if(currentCharges < maxCharge && coolDownTimer < coolDown)
            {
                coolDownTimer += Time.deltaTime;
                
            }
            else if(coolDownTimer >= coolDown && currentCharges < maxCharge)
            {
                currentCharges++;
                coolDownTimer = 0;
            }
        }
        void CastTimer()
        {
            if(isChanneling)
            {
                return;
            }

            //Cancel Cast if Move
            if(!isCasting)
            {
                return;
            }

            if(isCasting && castTimer < castTime)
            {
                castTimer += Time.deltaTime;
                timerPercent = castTimer / castTime;
                WhileCasting();
            }
            else if(castTimer >= castTime)
            {
                castTimer = 0;
                EndCast();
            }


        }

        void ChannelTimer()
        {
            if(isChanneling)
            {
                if(castTimer > 0)
                {
                    WhileChanneling();
                    castTimer -= Time.deltaTime;
                }
                if(castTimer <= 0)
                {
                    EndChannel();
                    abilityManager.OnCastChannelInterval?.Invoke(this);
                    HandleAbilityType();
                    StopCoroutine(ChannelCast());
                    entityInfo.stats.movementSpeed += channelMovementSpeedReduction;
                    if(cantMoveWhenChanneling)
                    {
                        CanMove(true);
                    }
                }

                if(cancelChannelOnMove && movement && movement.isMoving)
                {
                    CancelChannel();

                }
            }
        }

        void GlobalCoolDownTimer()
        {
            if(globalCoolDownTimer < globalCoolDown && globalCoolDownActive)
            {
                globalCoolDownTimer += Time.deltaTime;
                if(currentCharges == 0) { globalCoolDownTimer = globalCoolDown; }
            }
            else if(globalCoolDownTimer >= globalCoolDown)
            {
                globalCoolDownTimer = 0;
                globalCoolDownActive = false;
            }
        }
    }
    public void EndChannel()
    {
        castTimer = 0;
        isChanneling = false;
        isCasting = false;
        abilityManager.currentlyCasting = null;
        castTime = originalCastTime;
        channelInvokes = 0;
        abilityManager.OnCastChannelEnd?.Invoke(this);
    }
    public void Cast()
    {
        if (!active) { return; }
        if (isCasting) { return; }
        Recast();
        CancelAutoAttack();
        if (abilityManager.currentlyCasting != null){ return; }
        if (!CanCast()) return;
        if(globalCoolDownActive) { return; }
        if(currentCharges <= 0) { return; }
        if(onlyCastOutOfCombat && entityInfo.isInCombat) { return; }

        HandleCastMovement();
        isCasting = true;
        locationLocked = location;
        abilityManager.currentlyCasting = gameObject.GetComponent<AbilityInfo>();
        if (usesGlobalCoolDown) { globalCoolDownActive = true; }

        HandleNoMoveOnCast(false);
        HandleAutoAttackCast();
        HandleCastMovementSpeed();
        Recast(true);
        abilityManager.OnCastStart?.Invoke(this);


        void HandleCastMovement()
        {
            if(movement)
            {
                if(cancelCastIfMoved)
                {
                    movement.StopMoving();
                }
            }
        }
        void HandleCastMovementSpeed()
        {
            entityInfo.stats.movementSpeed -= castingMovementSpeedReduction;
        }
    }

    public void WhileCasting()
    {
        abilityManager.WhileCasting?.Invoke(this);

        if(!IsCorrectTarget(targetLocked))
        {
            CancelCast();
        }
    }

    public void WhileChanneling()
    {
        abilityManager.WhileChanneling?.Invoke(this);
    }
    public void Recast(bool activate = false, bool deactivate = false)
    {
        if (!recastProperties.recastable) { return; }

        if(activate)
        {
            if(recastProperties.isActive != activate)
            {
                recastProperties.isActive = activate;
                recastProperties.currentRecast = recastProperties.maxRecast;
                ArchAction.Delay(() => { Recast(false, true); }, recastProperties.recastTimeFrame);
            }
            return;
        }

        if(deactivate)
        {
            recastProperties.isActive = false;
            recastProperties.currentRecast = 0;
            return;
        }

        if (!recastProperties.isActive) { return; }
        if (recastProperties.currentRecast == 0 && recastProperties.maxRecast != -1) return;

        recastProperties.OnRecast?.Invoke(this);

    }
    public bool CanCast()
    {
        if (!HandleRequiresTargetLocked()) { return false; }
        if(!HasManaRequired()) { return false; }
        if(entityInfo.isInCombat && onlyCastOutOfCombat) { return false; }

        return true;

        
        bool HandleRequiresTargetLocked()
        {
            if (!requiresLockOnTarget)
            {
                return true;
            }

            if(onlyCastSelf || abilityType == AbilityType.Use)
            {
                target = entityObject;
            }

            if (!target)
            {
                return false;
            }

            if (target)
            {
                targetLocked = target;
                if (!IsCorrectTarget(targetLocked)) { return false; }
                movement.MoveTo(target.transform, range);
                if (targetLocked == null) { return false; }
            }

            if (IsInRange() && HasLineOfSight()) { return true; }
            
            return false;

            bool IsInRange()
            {
                if(range == -1)
                {
                    return true;
                }

                if (V3Helper.Distance(target.transform.position, entityObject.transform.position) > range)
                {
                    movement.MoveTo(target.transform, range - 1);
                    ActivateWantsToCast();
                    return false;
                }
                return true;
            }

            bool HasLineOfSight()
            {
                if (!lineOfSight)
                {
                    return true;
                }

                if (!requiresLineOfSight)
                {
                    return true;
                }

                if (!lineOfSight.HasLineOfSight(target))
                {
                    if (movement)
                    {
                        movement.MoveTo(target.transform);
                        ActivateWantsToCast();
                    }
                    return false;
                }
                return true;
            }
        }
    }
    public bool HasManaRequired()
    {
        if (entityInfo)
        {
            if (entityInfo.mana >= manaRequired + entityInfo.maxMana * manaRequiredPercent)
            {
                return true;
            }
        }

        return false;
    }
    public void EndCast()
    {
        if(!IsInRange() || !HasLineOfSight())
        {
            CancelCast();

            return;
        }

        abilityManager.OnCastRelease?.Invoke(this); 
        HandleResources();
        HandleChannel();
        HandleAbilityType();
        HandleFullHealth();
        HandleNoMoveOnCast(true);
        HandleCanMoveOnChannel();
        HandleCastMovementSpeed();


        void HandleResources()
        {
            abilityManager.currentlyCasting = null;
            
            currentCharges--;
            timerPercentActivated = false;
            timerPercent = 0f;
            HandleCastStatus();
            UseMana();
            HandleResourceProduction();

            void HandleResourceProduction()
            {
                entityInfo.Gain(manaProduced);
                entityInfo.Gain(manaProducedPercent * entityInfo.maxMana);
            }

            void HandleCastStatus()
            {
                if (!abilityFunctions.Contains(AbilityFunction.Channel)) isCasting = false;
            }
            
        }
        void HandleChannel()
        {
            if(abilityFunctions.Contains(AbilityFunction.Channel))
            {
                abilityManager.OnCastChannelStart?.Invoke(this);
                isChanneling = true;
                isCasting = true;
                castTimer = channelTime;
                castTime = channelTime;
                abilityManager.currentlyCasting = GetComponent<AbilityInfo>();
                channelInvokes++;
                entityInfo.stats.movementSpeed -= channelMovementSpeedReduction;
                StartCoroutine(ChannelCast());
            }
        }
        bool IsInRange()
        {
            if(!requiresLockOnTarget)
            {
                return true;
            }

            if(targetLocked == null)
            {
                return false;
            }

            if(V3Helper.Distance(entityObject.transform.position, targetLocked.transform.position) > range)
            {
                return false; 
            }
            return true;
        }
        bool HasLineOfSight()
        {
            if (!requiresLockOnTarget) { return true; }
            if(!requiresLineOfSight || lineOfSight == null)
            {
                return true;
            }

            if(lineOfSight.HasLineOfSight(targetLocked))
            {
                return true;
            }

            return false;
        }
        void HandleCanMoveOnChannel()
        {
            if(abilityFunctions.Contains(AbilityFunction.Channel) && cantMoveWhenChanneling)
            {
                CanMove(false);
            }
            else
            {
                CanMove(true);
            }
        }
        //Handles auto healing if the target has full health
        void HandleFullHealth()
        {
            if (!target) return;
            if (!target.GetComponent<EntityInfo>()) { return; }
            var targetInfo = target.GetComponent<EntityInfo>();
            if (isHealing && entityInfo.CanHelp(target))
            {
                if (targetInfo.health == targetInfo.maxHealth)
                {
                    isAutoAttacking = false;
                }
            }
        }

        void HandleCastMovementSpeed()
        {
            entityInfo.stats.movementSpeed += castingMovementSpeedReduction;
        }

    }
    public void CanMove(bool val)
    {
        if(movement)
        {
            movement.canMove = val;
        }
    }
    void HandleAutoAttackCast()
    {
        if (isAttack)
        {
            if (behavior)
            {
                if (behavior.CombatBehavior().GetFocus() == target)
                {
                    isAutoAttacking = true;
                }
            }

        }
    }
    public IEnumerator ChannelCast()
    {
        while(isChanneling)
        {
            yield return new WaitForSeconds(channelIntervals);

            if (isChanneling)
            {
                if (channelInvokes > 0)
                {
                    //RepeatAnim();
                    abilityManager.OnCastChannelInterval?.Invoke(this);

                }
                HandleAbilityType();
                channelInvokes++;
                
            }
            
        }
        
    }
    public void CancelCast(bool resetTargets = false, bool stopMoving = false)
    {
        if(abilityManager)
        {
            abilityManager.currentlyCasting = null;
            abilityManager.OnCancelCast?.Invoke(this);
        }

        if(movement && cantMoveWhenCasting)
        {
            movement.canMove = true;


            if (stopMoving)
            {
                movement.StopMoving();
            }
        }

        timerPercentActivated = false;

        castTimer = 0;
        isCasting = false;

        

        if(resetTargets)
        {
            target = null;
            targetLocked = null;
        }
    }
    public void CancelChannel()
    {
        if (abilityManager)
        {
            abilityManager.currentlyCasting = null;
            abilityManager.OnCancelChannel?.Invoke(this);
        }



        if(movement && cantMoveWhenChanneling)
        {
            movement.canMove = true;
        }

        timerPercentActivated = false;

        EndChannel();
        StopCoroutine(ChannelCast());
    }
    public void HandleAbilityType()
    {
        if(catalyst) { catalyst.GetComponent<CatalystInfo>().isCataling = false; }
        switch (abilityType)
        {
            case AbilityType.SkillShot: FreeCast(); break;
            case AbilityType.LockOn: LockOn(); break;
            case AbilityType.Use: Use(); break;
            default: FreeCast(); break;
        }

        void FreeCast()
        {
            if(!catalyst)
            {
                return;
            }
            directionLocked = V3Helper.Direction(location, entityObject.transform.position);

            catalyst.GetComponent<CatalystInfo>().abilityInfo = this;
            Instantiate(catalyst, entityObject.transform.position, transform.localRotation);
            abilityManager.OnCatalystRelease?.Invoke(this);
        }
        void LockOn()
        {
            if(!catalyst)
            {
                return;
            }

            catalyst.GetComponent<CatalystInfo>().abilityInfo = this;
            Instantiate(catalyst, entityObject.transform.position, transform.localRotation);


            if (!isChanneling && !isAttack)
            {
                targetLocked = null;
                target = null;
            }

            abilityManager.OnCatalystRelease?.Invoke(this);
        }
        void Use()
        {
            if (!catalyst) { return; }

            catalyst.GetComponent<CatalystInfo>().abilityInfo = this;
            Instantiate(catalyst, entityObject.transform.position, transform.localRotation);

            if(!isChanneling && !isAttack)
            {
                targetLocked = null;
                target = null;

            }

            abilityManager.OnCatalystRelease?.Invoke(this);
        }
    }
    public bool IsCorrectTarget(GameObject target)
    {
        if(target == null) { return true; }
        if(target.GetComponent<EntityInfo>())
        {
            if(!canCastSelf)
            {
                if (target == entityObject)
                {
                    return false;    
                }
            }

            EntityInfo targetInfo = target.GetComponent<EntityInfo>();
            if(!targetInfo.isAlive && !targetsDead)
            {
                abilityManager.OnDeadTarget?.Invoke(this);
                return false; 
            }
            if(targetInfo.isAlive && targetsDead) { return false; }

            if((isHealing || isAssisting) && entityInfo.CanHelp(target))
            {
                return true;
            }

            if(isHarming && entityInfo.CanAttack(target))
            {
                return true;
            }
            
            if(isHealing && isHarming && isAssisting)
            {
                return true;
            }
        }



        return false;
    }
    public void UseMana()
    {
        if(entityInfo)
        {
            entityInfo.Use(manaRequired + entityInfo.maxMana*manaRequiredPercent);
        }
    }
    public void ActivateWantsToCast()
    {
        if(!wantsToCast)
        {
            wantsToCast = true;
            abilityManager.wantsToCastAbility = wantsToCast;
            abilityManager.OnWantsToCastChange?.Invoke(this, wantsToCast);
        }
    }
    public bool WantsToCast()
    {
        return wantsToCast;
    }
    public void DeactivateWantsToCast()
    {
        if (wantsToCast)
        {
            wantsToCast = false;
            abilityManager.wantsToCastAbility = wantsToCast;
            abilityManager.OnWantsToCastChange?.Invoke(this, wantsToCast);
        }
    }
    public void HandleWantsToCast()
    {
        if(!wantsToCast)
        {
            return;
        }    
        if(!entityInfo.isAlive && !targetsDead) {
            abilityManager.OnDeadTarget?.Invoke(this);
            DeactivateWantsToCast();
            return; }
        if(target == null) {
            DeactivateWantsToCast();
            return; }
        if(CanCast())
        {
            Cast();
            DeactivateWantsToCast();
        }
    }
    public void UpdateAbility()
    {
        Debugger.InConsole(56489, $"{entityObject != null}");
        if (abilityManager)
        {
            if (entityObject)
            {
                entityObject = abilityManager.entityObject;
                entityInfo = abilityManager.entityInfo;

                value = baseValue
                    + entityInfo.stats.Wisdom * wisdomContribution
                    + entityInfo.stats.Strength * strengthContribution
                    + entityInfo.stats.Dexterity * dexterityContribution;

                buffProperties.value = value * buffProperties.valueContributionToBuff;
                buffProperties.aoeValue = value * buffProperties.valueContributionToBuffAOE;
            }
        }

        
        if (catalyst)
        {
            
            catalystInfo = catalyst.GetComponent<CatalystInfo>();

            range = catalystInfo.range;
            speed = catalystInfo.speed;
            
            catalystType = catalystInfo.catalystType;

            if(isAttack && entityInfo)
            {
                var attackInterval = 1 / entityInfo.stats.attackSpeed;

                if(catalystInfo.catalystType == CatalystType.Melee)
                {
                    castTime = attackInterval *.25f;
                    coolDown = attackInterval *.75f;
                }
                else
                {
                    castTime = attackInterval *.25f;
                    coolDown = attackInterval *.75f;
                }
                
                
            }
            else
            {
                castTime = catalystInfo.castTime;
            }
            originalCastTime = castTime;

            if (abilityType == AbilityType.SkillShot)
            {
                //bool isMelee = catalystInfo.catalystType == CatalystType.Melee;
                //bool isSweep = catalystInfo.catalystType == CatalystType.Sweep;
                //bool isRadiate = catalystInfo.catalystType == CatalystType.Radiate;
                //if (isMelee || isSweep || isRadiate)
                //{
                //    abilityType = AbilityType.SkillShotScan;
                //}
                requiresLineOfSight = false;
                requiresLockOnTarget = false;
            }

            if (abilityType == AbilityType.LockOn)
            {
                //bool isMelee = catalystInfo.catalystType == CatalystType.Melee;
                //bool isSweep = catalystInfo.catalystType == CatalystType.Sweep;
                //bool isRadiate = catalystInfo.catalystType == CatalystType.Radiate;
                //if (isMelee || isSweep || isRadiate)
                //{
                //    abilityType = AbilityType.LockOnScan;
                    
                //}
                requiresLineOfSight = true;
                requiresLockOnTarget = true;
            }

        }
     }
    public bool IsReady()
    {
        if(!active) { return false; }
        if(entityInfo.mana < (manaRequired + manaRequiredPercent*entityInfo.maxMana))
        {
            return false;
        }
        if(currentCharges <= 0)
        {
            return false;
        }

        return true;
    }
    public void CancelAutoAttack()
    {
        if (abilityManager == null) return;
        if(!abilityManager.currentlyCasting) { return; }
        if (!abilityManager.currentlyCasting.isAttack) return;
        if (isAttack) return;
        //if (!IsReady()) return;
        //if(!CanCast()) { return; }
        abilityManager.currentlyCasting.CancelCast();
    }
    public void HandleAutoAttack()
    {
        if(isAutoAttacking)
        {
            Cast();
        }
    }
    public void HandleDeadTarget()
    {
        if(target)
        {
            if(target.GetComponent<EntityInfo>())
            {
                if(!target.GetComponent<EntityInfo>().isAlive && !targetsDead)
                {
                    CancelCast();
                    CancelChannel();
                    wantsToCast = false;
                }
            }
        }
    }
    public void HandleNoMoveOnCast(bool val)
    {
        if(movement)
        {
            if(cantMoveWhenCasting && isCasting)
            {
                CanMove(val);
            }
        }
    }
    public void HandleCastTimerPercent()
    {
        if (!isCasting) return;
        if (timerPercentActivated) return;
        if (!catalystInfo || catalystInfo.releasePercent <= 0) return;
        if (abilityFunctions.Contains(AbilityFunction.Channel)) return;

        if(timerPercent >= catalystInfo.releasePercent && !isAttack)
        {
            timerPercentActivated = true;
            abilityManager.OnCastReleasePercent?.Invoke(this);
            //SetAnimTrigger(1);
        }
    }

    //Function that allows an ability to cast itself if a script has the ability and the ability only
    //Should only be used when a player tries to cast
    public void Use()
    {
        if (entityInfo && entityInfo.PlayerController())
        {
            if(abilityManager)
            {
                abilityManager.Cast(this);
                abilityManager.OnTryCast?.Invoke(this);
            }
        }
    }

    public void CastAt(Vector3 position)
    {
        if (GetComponentInParent<AbilityManager>() == null) { return; }
        location = position;
        Cast();
    }

    public void CastAt(GameObject target)
    {
        if(GetComponentInParent<AbilityManager>() == null) { return; }
        this.target = target;
        Cast();
    }

}
