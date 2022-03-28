using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;

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

    [Serializable]
    public struct SummoningProperty
    {
        public bool summons;
        public List<GameObject> SummonableEntities;

        [Header("Summoning Settings")]
        public float radius;
        public float valueContributionToStats;
    }

    public SummoningProperty summoning;

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
    public struct AbilityVisualEffects
    {
        public bool showCastBar;
        public bool showChannelBar;
        public bool showTargetIconOnTarget;
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

    [Serializable]
    public struct ChannelProperties
    {
        public bool enabled;
        public bool active;
        public float time;
        public int invokeAmount;
        public bool cancel;

        [Header("Restrictions")]
        public bool canMove;
        public bool cancelChannelOnMove;
        public float deltaMovementSpeed;

    }

    [Header("ChannelProperties")]
    public ChannelProperties channel;


    public float channelTime = 3;
    public float channelIntervals = 1f;
    public int channelInvokes = 0;
    public bool cancelChannelOnMove = false;
    public bool cantMoveWhenChanneling;
    public float channelMovementSpeedReduction;

    public void OnValidate()
    {
        channel.enabled = abilityFunctions.Contains(AbilityFunction.Channel);
        channel.time = channelTime;
        channel.invokeAmount =(int)  (channelTime / channelIntervals);

        channel.cancelChannelOnMove = cancelChannelOnMove;
        channel.deltaMovementSpeed = channelMovementSpeedReduction;
        
    }

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
    public bool activated;
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
    public struct DestroyConditions
    {
        public bool destroyOnCollisions;
        public bool destroyOnStructure;
        public bool destroyOnNoTickDamage;
        public bool destroyOnReturn;
        public bool destroyOnOutOfRange;
        public bool destroyOnLiveTime;
        public bool destroyOnDeadTarget;
        public bool destroyOnCantFindTarget;
    }

    public DestroyConditions destroyConditions;

    [Serializable]
    public struct RecastProperties
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
    public bool canceledCast;
    public float channelTimer;
    public bool isChanneling;
    public bool isCasting;
    public float timerPercent;
    public bool timerPercentActivated;
    public float progress;

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
        if (!ability.IsReady()) return;
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
        if (wantsToCast)
        {
            DeactivateWantsToCast();
        }

        if (isCasting)
        {
            CancelCast();
        }

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
        if (abilityType != AbilityType.LockOn) return;
        if(target && after == target.transform) { return; }
        if (wantsToCast)
        {
            DeactivateWantsToCast();
        }

        if (isAutoAttacking)
        {
            CancelCast();
        }

        target = null;
        targetLocked = null;
        isAutoAttacking = false;
        

        //if(target != null && after != target.transform)
        //{
        //    if (wantsToCast)
        //    {
        //        DeactivateWantsToCast();
                
        //    }

        //    if(isAttack)
        //    {
        //        target = null;
        //        targetLocked = null;
        //        isAutoAttacking = false;
        //    }
        //}
    }
    public void HandleTimers()
    {
        CoolDownTimer();
        //CastTimer();
        //ChannelTimer();
        GlobalCoolDownTimer();
        //HandleCastTimerPercent();

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
                    //abilityManager.OnCastChannelInterval?.Invoke(this);
                    HandleAbilityType();
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

    async Task<bool> CastTimer()
    {
        if (isCasting) return false;

        abilityManager.OnCastStart?.Invoke(this);
        isCasting = true;

        var success = true;

        while (castTimer < castTime)
        {
            await Task.Yield();
            WhileCasting();
            castTimer += Time.deltaTime;
            timerPercent = castTimer / castTime;
            abilityManager.WhileCasting?.Invoke(this);
            HandleCastTimerPercent();
            progress = castTimer / castTime;

            if (!CanContinue())
            {
                success = false;
                break;
                
            }

            if (canceledCast)
            {
                canceledCast = false;
                success = false;
                abilityManager.OnCancelCast?.Invoke(this);
                break;
            }


        }


        castTimer = 0;


        abilityManager.OnCastEnd?.Invoke(this);
        return success;


        bool CanContinue()
        {
            if (!IsCorrectTarget(targetLocked))
            {
                return false;
            }

            if (canceledCast)
            {
                canceledCast = false;
                return false;
            }

            if (targetLocked != target)
            {
                targetLocked = target;
            }

            return true;
        }
    }
    public void EndChannel()
    {
        castTimer = 0;
        isChanneling = false;
        isCasting = false;
        //abilityManager.currentlyCasting = null;
        castTime = originalCastTime;
        channelInvokes = 0;
        abilityManager.OnCastEnd?.Invoke(this);
        //abilityManager.OnCastChannelEnd?.Invoke(this);
    }

    public async Task Progress()
    {
        while (isCasting || channel.active)
        {
            await Task.Yield();
        }
    }
    async public void Cast()
    {
        Activate();
        return;
        if (!CanCast2()) return;

        HandleCastMovement();
        locationLocked = location;
        
        if (usesGlobalCoolDown) { globalCoolDownActive = true; }

        HandleNoMoveOnCast(false);
        HandleAutoAttackCast();
        HandleCastMovementSpeed();

        abilityManager.currentlyCasting = this;

        abilityManager.OnAbilityStart?.Invoke(this);
        abilityManager.OnCastStart?.Invoke(this);

        var success = await CastTimer();

        abilityManager.OnCastEnd?.Invoke(this);

        HandleNoMoveOnCast(true);

        if (!success)
        {
            abilityManager.OnAbilityEnd?.Invoke(this);
            abilityManager.currentlyCasting = null;
            return;
        }

        EndCast();

        await ExtraAbilityFunctions();

        abilityManager.OnAbilityEnd?.Invoke(this);

        abilityManager.currentlyCasting = null;

        
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

    public async Task ExtraAbilityFunctions()
    {
        await ChannelTimer();

    }

    public bool IsBusy()
    {
        if (isCasting) return true;

        if (channel.active) return true;

        return false;
    }

    public async Task ChannelTimer()
    {
        if (!channel.enabled) return;
        if (channel.active) return;

        channel.active = true;

        abilityManager.OnChannelStart?.Invoke(this);

        float timer = channel.time;
        int invokes = channel.invokeAmount;
        float percentPerInvoke = (channel.time / channel.invokeAmount);

        Debugger.InConsole(4104, $"Total invokes is {invokes}");

        while (timer > 0)
        {
            await Task.Yield();
            timer -= Time.deltaTime;
            var percent = timer / channel.time;

            castTimer = castTime * percent;
            


            if (invokes > (timer / channelIntervals) + 1 && invokes > 0)
            {
                invokes--;
                abilityManager.OnChannelInterval?.Invoke(this);
                Debugger.InConsole(4104, $"{invokes} invokes left");
                
            }

            if (channel.cancel)
            {
                abilityManager.OnCancelChannel?.Invoke(this);
                channel.cancel = false;
                break;
            }
        }

        channel.active = false;

        abilityManager.OnChannelEnd?.Invoke(this);
    }

    public void WhileCasting()
    {
        abilityManager.WhileCasting?.Invoke(this);

        if(!IsCorrectTarget(targetLocked))
        {
            CancelCast();
        }
        if (targetLocked != target)
        {
            targetLocked = target;
        }
    }

    public void WhileChanneling()
    {
        abilityManager.WhileChanneling?.Invoke(this);
    }
    public void Recast()
    {
        if (!recastProperties.isActive) return;
        if (recastProperties.maxRecast == -1)
        {
            recastProperties.currentRecast += 1;
            recastProperties.OnRecast?.Invoke(this);
            return;
        }

        if (recastProperties.currentRecast < recastProperties.maxRecast) return;

        recastProperties.currentRecast += 1;

        recastProperties.OnRecast?.Invoke(this);
    }

    async void SetRecast()
    {
        if (!recastProperties.recastable) return;
        if (recastProperties.isActive) return;

        recastProperties.isActive = true;

        var recastTimer = 0f;

        while (recastTimer < recastProperties.recastTimeFrame)
        {
            await Task.Yield();
            recastTimer += Time.deltaTime;

        }

        recastProperties.isActive = false;

    }
    public bool CanCast()
    {
        if (!HandleRequiresTargetLocked()) { return false; }
        if(!HasManaRequired()) { return false; }
        if(entityInfo.isInCombat && onlyCastOutOfCombat) { return false; }

        return true;

        
        
    }

    public bool CanCast2()
    {
        if (!IsReady()) return false;
        if (IsBusy()) return false;
        if (!AbilityManagerCanCast()) return false;
        if (!HasManaRequired()) return false;
        if (!HandleRequiresTargetLocked()) return false;
        if (!CorrectCombat()) return false;

        return true;

        bool CorrectCombat()
        {
            if (!onlyCastOutOfCombat) return true;

            if (onlyCastOutOfCombat && !entityInfo.isInCombat)
            {
                return true;
            }

            return false;
        }

        bool AbilityManagerCanCast()
        {
            if (abilityManager.currentlyCasting == null) return true;
            if (abilityManager.currentlyCasting && abilityManager.currentlyCasting.isAttack)
            {
                abilityManager.currentlyCasting.CancelCast();
                return true;
            }

            return false;

        }
    }

    bool HandleRequiresTargetLocked()
    {
        if (!requiresLockOnTarget)
        {
            locationLocked = location;
            return true;
        }


        if (onlyCastSelf || abilityType == AbilityType.Use)
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
            if (range == -1)
            {
                return true;
            }

            if (V3Helper.Distance(target.transform.position, entityObject.transform.position) > range)
            {
                movement.MoveTo(target.transform, range);
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


    public bool CanCastAt(GameObject target)
    {
        if (entityInfo.CanAttack(target) && isHarming)
        {
            return true;
        }

        if (entityInfo.CanHelp(target) && (isHealing || isAssisting))
        {
            return true;
        }

        return false;
    }

    public bool AbilityIsInRange(GameObject target)
    {
        var distance = catalystInfo.range;

        if (V3Helper.Distance(target.transform.position, entityObject.transform.position) <= distance)
        {
            return true;
        }


        return false;
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
        SetRecast();


        void HandleResources()
        {
            
            currentCharges--;
            timerPercentActivated = false;
            timerPercent = 0f;
            //HandleCastStatus();
            UseMana();
            HandleResourceProduction();

            void HandleResourceProduction()
            {
                entityInfo.GainResource(manaProduced);
                entityInfo.GainResource(manaProducedPercent * entityInfo.maxMana);
            }

            
        }
        void HandleChannel()
        {
            //if (abilityFunctions.Contains(AbilityFunction.Channel))
            //{
            //    abilityManager.OnCastChannelStart?.Invoke(this);
            //    isChanneling = true;
            //    isCasting = true;
            //    castTimer = channelTime;
            //    castTime = channelTime;
            //    abilityManager.currentlyCasting = GetComponent<AbilityInfo>();
            //    channelInvokes++;
            //    entityInfo.stats.movementSpeed -= channelMovementSpeedReduction;
            //    StartCoroutine(ChannelCast());
            //}
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

            if (range == -1) return true;

            if(V3Helper.Distance(entityObject.transform.position, targetLocked.transform.position) > range)
            {
                return false; 
            }
            return true;
        }
        bool HasLineOfSight()
        {
            if (!requiresLockOnTarget) { return true; }
            if(range == -1) { return true; }
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
                    //isAutoAttacking = false;
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
    public void CancelCast(bool resetTargets = false, bool stopMoving = false)
    {
        if (!isCasting) return;

        if(abilityManager)
        {
            //abilityManager.currentlyCasting = null;
            abilityManager.OnCastEnd?.Invoke(this);
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



        canceledCast = true;

        

        if(resetTargets)
        {
            target = null;
            targetLocked = null;
        }
    }
    public void CancelChannel()
    {
        if (!isChanneling) return;
        channel.cancel = true;
        if (abilityManager)
        {
            //abilityManager.currentlyCasting = null;
            
            abilityManager.OnCancelChannel?.Invoke(this);
        }



        if(movement && cantMoveWhenChanneling)
        {
            movement.canMove = true;
        }

        timerPercentActivated = false;

        EndChannel();
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


            if (!channel.enabled && !isAttack)
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
        if(CanCast2())
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

            abilityIcon = catalystInfo.catalystIcon;
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

        if (globalCoolDownActive) return false;

        if(entityInfo.mana < (manaRequired + manaRequiredPercent*entityInfo.maxMana))
        {
            return false;
        }

        if(currentCharges <= 0) { return false; }

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


    //New Generation of Casting;

    public async void Activate()
    {
        if (activated) return;
        if (!CanCast2()) return;

        activated = true;

        abilityManager.OnAbilityStart?.Invoke(this);
        abilityManager.currentlyCasting = this;

        bool success = await Casting();

        if (success)
        {
            await Channeling();
        }

        abilityManager.OnAbilityEnd?.Invoke(this);
        abilityManager.currentlyCasting = null;
        
        

        activated = false;
        HandleAttack();
        
        if (!isAutoAttacking)
        {
            ClearTargets();
        }

        async Task<bool> Casting()
        {
            var success = true;
            var timer = 0f;
            progress = 0;

            BeginCast();

            while (timer < castTime && castTime > 0)
            {
                await Task.Yield();
                timer += Time.deltaTime;
                progress = timer / castTime;

                WhileCasting1();

                if (!CanContinue())
                {
                    success = false;
                    break;
                }
            }


            EndCast1();

            return success;

            void BeginCast()
            {
                isCasting = true;
                timerPercentActivated = false;
                abilityManager.OnCastStart?.Invoke(this);
            }

            void EndCast1()
            {
                if (success)
                {
                    EndCast();
                }
                isCasting = false;
                timerPercentActivated = false;
                
            }

            void WhileCasting1()
            {
                HandleTimerPercent();

                abilityManager.WhileCasting?.Invoke(this);

                void HandleTimerPercent()
                {
                    if (timerPercentActivated) return;
                    if (progress < catalystInfo.releasePercent) return;

                    timerPercentActivated = true;


                    abilityManager.OnCastReleasePercent?.Invoke(this);
                }
            }
        }

        async Task Channeling()
        {
            if (!channel.enabled) return;
            if (channel.active) return;


            StartChannel();

            var timer = 0f;
            

            float progressPerInvoke = (1f / channel.invokeAmount);
            float progressBlock = progressPerInvoke;

            while (timer < channel.time)
            {
                await Task.Yield();
                timer += Time.deltaTime;
                progress = 1 - (timer / channel.time);

                abilityManager.WhileCasting?.Invoke(this);

                if (!CanContinue())
                {
                    break;
                }

                if (progressBlock < (timer / channel.time))
                {
                    HandleAbilityType();
                    abilityManager.OnChannelInterval?.Invoke(this);
                    progressBlock += progressPerInvoke;
                }

            }

            EndChannel();

            

            void StartChannel()
            {
                progress = 1;
                channel.active = true;
                abilityManager.OnChannelStart?.Invoke(this);
            }

            void EndChannel()
            {
                progress = 0;
                channel.active = false;
                abilityManager.OnChannelEnd?.Invoke(this);
            }
        }

        async void HandleAttack()
        {
            if (!success) return;
            if (!isAttack) return;

            isAutoAttacking = true;
            var target = targetLocked;

            while (currentCharges <= 0)
            {
                await Task.Yield();
            }

            if (!isAutoAttacking) return;
            if (target != targetLocked) return;
            Cast();
        }
    }

    public bool CanContinue()
    {
        if (targetLocked)
        {
            if (!IsCorrectTarget(targetLocked))
            {
                return false;
            }
        }

        if (canceledCast && isCasting)
        {
            canceledCast = false;
            return false;
        }

        if (channel.active && channel.cancel)
        {
            channel.cancel = false;
            return false;
        }

        if (!entityInfo.isAlive)
        {
            return false;
        }

        
        return true;
    }
    

    public void ClearTargets()
    {
        target = null;
        targetLocked = null;
    }



    
    

    

    



}
