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
    //public List<AbilityFunction> abilityFunctions;
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
        public bool enabled;
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


    [Serializable]
    public class StatsContribution
    {
        public float strength, wisdom, dexterity = 1f;
    }

    public void OnValidate()
    {
        coreContribution.strength = strengthContribution;
        coreContribution.wisdom = wisdomContribution;
        coreContribution.dexterity = dexterityContribution;
    }

    public StatsContribution coreContribution;

    [Header("Stats Contribution")]
    public float strengthContribution = 1f;
    public float wisdomContribution = 1f;
    public float dexterityContribution = 1f;

    [Header("Ability Properties")]

    public float knockBackValue = 1.0f;
    public float manaRequiredPercent = .05f;
    public float manaRequired = 25f;
    public float manaProduced = 0f;
    public float manaProducedPercent = 0f;
    public float baseValue = 5.0f;
    public float value;
    public float originalCastTime = 1f;
    public float selfCastMultiplier = 1f;
    public float liveTime = 3;
    public int ticksOfDamage = 1;

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

    [Serializable]
    public struct Resources
    {
        public float requiredAmount;
        public float requiredPercent;
        public float producedAmount;
        public float producedPercent;
    }

    public Resources resources;

    [Serializable]
    public class CoolDownProperties
    {
        public float timePerCharge = 1;
        public float progressTimer;
        public float progress = 1;
        public bool isActive;
        public bool globalCoolDownActive;
        public bool usesGlobal;
        public bool maxChargesOnStart = true;
        public int charges;
        public int maxCharges = 1;
    }



    public CoolDownProperties coolDown;




    [Serializable]
    public struct SplashProperties
    {
        public bool enable;
        public GameObject splashParticleEffects;
        public bool requiresLOS;
        public bool appliesBuffs;
        public float valueContribution;
        public float radius;
        public float delay;
    }


    public SplashProperties splash;

    //[Header("Spalsh Properties")]
    //public bool splashes;
    //public GameObject splashParticleEffects;
    //public bool splashRequiresLOS;
    //public bool splashAppliesBuffs;
    //public int maxSplashTargets;
    //public float valueContributionToSplash;
    //public float splashRadius;
    //public float splashDelay;

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

    [Serializable]
    public struct Restrictions
    {
        public bool activated;
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
    }
    public Restrictions restrictions;


    [Header("AbilityRestrictions")]
    public bool activated;
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
    //public float currentCharges;
    //public float globalCoolDownTimer;
    //public bool globalCoolDownActive;
    public bool canceledCast;
    public bool isCasting;
    public float timerPercent;
    public bool timerPercentActivated;
    public float progress;
    public float progressTimer;

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
                abilityManager.OnGlobalCoolDown += OnGlobalCoolDown;
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

        if(coolDown.maxChargesOnStart)
        {
            coolDown.charges = coolDown.maxCharges;
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
            CancelCast("Moved On Cast");
        }

        if (channel.enabled && channel.active)
        {
            if (channel.cancelChannelOnMove)
            {
                CancelChannel(true);
            }
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
            isAutoAttacking = false;


            if(isCasting)
            {
                CancelCast("a non attack ability is trying to be casted.");
            }
        }
    }
    public void OnTryMove(Movement movement)
    {
        if (isAutoAttacking)
        {
            isAutoAttacking = false;
        }

        if (wantsToCast)
        {
            DeactivateWantsToCast();
        }

        if (isCasting)
        {
            CancelCast("Player tried to move");
        }

        
    }
    public void OnChangeNPCType(NPCType before, NPCType after)
    {
        if(WantsToCast())
        {
            DeactivateWantsToCast();
            ClearTargets();
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
            CancelCast("New Path Target");
        }


        ClearTargets();
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
        SetCoolDownTimer();
    }


    async void OnGlobalCoolDown(AbilityInfo ability)
    {
        if (!coolDown.usesGlobal) return;
        if (coolDown.globalCoolDownActive) return;
        if (coolDown.charges == 0) return;


        coolDown.globalCoolDownActive = true;

        float timer = 1 - Haste() * 1;


        while (timer > 0)
        {
            await Task.Yield();
            timer -= Time.deltaTime;
            coolDown.progress = 1 - timer; 
        }


        coolDown.progress = 1;

        coolDown.globalCoolDownActive = false;
        
    }

    async public void Cast()
    {
        if (isAttack)
        {
            isAutoAttacking = true;
            while (isAutoAttacking)
            {
                var success = await Activate();

                if (!success)
                {
                    break; 
                }
            }
        }
        else
        {
            await Activate();
        }
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

        var recastTimer = recastProperties.recastTimeFrame;

        while (recastTimer > 0)
        {
            await Task.Yield();
            recastTimer -= Time.deltaTime;

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
                abilityManager.currentlyCasting.CancelCast("Canceled auto to allow ability to cast");
                ActivateWantsToCast();
                abilityManager.currentlyCasting = null;
                return true;
            }

            return false;

        }
    }

    bool HandleRequiresTargetLocked()
    {
        locationLocked = location;
        //if (!requiresLockOnTarget)
        //{
        //    return true;
        //}

        if (abilityType != AbilityType.LockOn) return true;

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
            CancelCast("Broke LOS or out of range");

            return;
        }

        abilityManager.OnCastRelease?.Invoke(this);

        HandleResources();
        HandleAbilityType();
        HandleFullHealth();
        HandleCastMovementSpeed();
        SetRecast();


        void HandleResources()
        {
            
            coolDown.charges--;
            timerPercentActivated = false;
            timerPercent = 0f;
            UseMana();
            HandleResourceProduction();

            void HandleResourceProduction()
            {
                entityInfo.GainResource(manaProduced);
                entityInfo.GainResource(manaProducedPercent * entityInfo.maxMana);
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
    public void CancelCast(string reason = "")
    {
        if (!isCasting) return;

        LogCancel(reason);

        if(abilityManager)
        {
            abilityManager.OnCancelCast?.Invoke(this);
        }


        canceledCast = true;
    }
    public void CancelChannel(bool resetTarget = false)
    {
        if (!channel.active) return;
        channel.cancel = true;
        if (abilityManager)
        {
            
            abilityManager.OnCancelChannel?.Invoke(this);
        }


        if(movement && cantMoveWhenChanneling)
        {
            movement.canMove = true;
        }

        timerPercentActivated = false;

        //EndChannel();
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
            var catalystClone = Instantiate(catalyst, entityObject.transform.position, transform.localRotation);
            abilityManager.OnCatalystRelease?.Invoke(this, catalystClone.GetComponent<CatalystInfo>());
        }
        void LockOn()
        {
            if(!catalyst)
            {
                return;
            }

            catalyst.GetComponent<CatalystInfo>().abilityInfo = this;
            var catalystClone = Instantiate(catalyst, entityObject.transform.position, transform.localRotation);


            if (!channel.enabled && !isAttack)
            {
                ClearTargets();
            }

            abilityManager.OnCatalystRelease?.Invoke(this, catalystClone.GetComponent<CatalystInfo>());
        }
        void Use()
        {
            if (!catalyst) { return; }

            catalyst.GetComponent<CatalystInfo>().abilityInfo = this;
            var catalystClone = Instantiate(catalyst, entityObject.transform.position, transform.localRotation);


            abilityManager.OnCatalystRelease?.Invoke(this, catalystClone.GetComponent<CatalystInfo>());
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
                    coolDown.timePerCharge = attackInterval *.75f;
                }
                else
                {
                    castTime = attackInterval *.25f;
                    coolDown.timePerCharge = attackInterval *.75f;
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
        if (isCasting) return false;
        if (channel.active) return false;

        if (coolDown.globalCoolDownActive) return false;

        if(entityInfo.mana < (manaRequired + manaRequiredPercent*entityInfo.maxMana))
        {
            return false;
        }

        if(coolDown.charges <= 0) { return false; }

        return true;
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
                    CancelCast("Target Is Dead");
                    CancelChannel();
                    wantsToCast = false;
                }
            }
        }
    }
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
        if (isCasting || channel.active) return;
        if (GetComponentInParent<AbilityManager>() == null) { return; }
        location = position;
        Cast();
    }
    public void CastAt(GameObject target)
    {
        if (isCasting || channel.active) return;
        if(GetComponentInParent<AbilityManager>() == null) { return; }
        this.target = target;
        Cast();
    }
    public async Task<bool> Activate()
    {
        if (activated) return false;
        if (!CanCast2()) return false;

        activated = true;

        abilityManager.OnAbilityStart?.Invoke(this);
        abilityManager.currentlyCasting = this;

        bool success = await Casting();

        if (success)
        {
            ActivateGlobalCoolDown();
            await Channeling();
            SetRecast();
        }



        if (abilityManager.currentlyCasting == this)
        {
            abilityManager.currentlyCasting = null;
            abilityManager.OnAbilityEnd?.Invoke(this);
        }
        SetCoolDownTimer();
        //SetCoolDownTimer();
        
        activated = false;
        


        if (!isAutoAttacking)
        {
            ClearTargets();
        }

        return success;

        async Task<bool> Casting()
        {
            var success = true;

            

            var timer = castTime - castTime * Haste();
            var startTime = timer;

            progress = 0;


            BeginCast();

            while (timer > 0)
            {
                await Task.Yield();
                timer -= Time.deltaTime;
                progress = 1 - (timer / startTime);
                progressTimer = timer;

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

                abilityManager.OnCastEnd?.Invoke(this);
                isCasting = false;
                timerPercentActivated = false;
                
            }

            void WhileCasting1()
            {
                abilityManager.WhileCasting?.Invoke(this);
                if (isAttack) return;
                if (targetLocked != target)
                {
                    targetLocked = target;
                }
                HandleTimerPercent();

                

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

            var timer = channel.time - channel.time * Haste();
            var startTime = timer;


            Debugger.InConsole(3245, $"Timer is {timer}");

            float progressPerInvoke = (1f / channel.invokeAmount);
            float progressBlock = 1 - progressPerInvoke;

            while (timer > 0)
            {
                await Task.Yield();
                timer -= Time.deltaTime;
                progress = (timer / startTime);

                WhileChanneling();

                if (!CanContinue())
                {
                    break;
                }

                if (progressBlock > progress)
                {
                    HandleAbilityType();
                    abilityManager.OnChannelInterval?.Invoke(this);
                    progressBlock -= progressPerInvoke;
                }

            }

            EndChannel();

            

            void StartChannel()
            {
                progress = 1;
                channel.active = true;
                abilityManager.OnChannelStart?.Invoke(this);
            }

            void WhileChanneling()
            {
                abilityManager.WhileCasting?.Invoke(this);
                if (isAttack) return;


                if (targetLocked != target)
                {
                    targetLocked = target;
                }
            }

            void EndChannel()
            {
                progress = 0;
                channel.active = false;
                abilityManager.OnChannelEnd?.Invoke(this);
            }
        }
    }
    float Haste()
    {
        if (entityInfo)
        {
            return entityInfo.stats.haste;
        }

        return 0;
    }
    public bool CanContinue()
    {
        if (canceledCast && isCasting)
        {
            LogCancel("Canceled Cast");
            canceledCast = false;
            return false;
        }

        if (channel.active && channel.cancel)
        {
            LogCancel("Canceled Channel");
            channel.cancel = false;
            return false;
        }

        if (requiresLockOnTarget)
        {
            if (target == null)
            {
                LogCancel("Null Target");
                return false;
            }
        }

        if (abilityManager.currentlyCasting != this && isAttack)
        {
            LogCancel("Not equal to ability manager currently casting");
            return false;
        }

        if (targetLocked)
        {
            if (!IsCorrectTarget(targetLocked))
            {
                LogCancel("Incorrect Target");
                return false;
            }
        }



        if (!entityInfo.isAlive)
        {
            LogCancel("Entity Died");
            return false;
        }

        
        return true;

        
    }
    void LogCancel(string reason)
    {
        Debugger.InConsole(9536, $"{entityInfo} stopped casting {this} because {reason}");
    }
    async void SetCoolDownTimer()
    {
        if (coolDown.isActive) return;
        if (coolDown.charges >= coolDown.maxCharges) return;

        coolDown.isActive = true;

        var timer = coolDown.timePerCharge - coolDown.timePerCharge * Haste();

        while (coolDown.charges < coolDown.maxCharges)
        {
            await Task.Yield();
            timer -= Time.deltaTime;

            if (!coolDown.globalCoolDownActive)
            {
                coolDown.progress = 1 - (timer / coolDown.timePerCharge);
            }

            coolDown.progressTimer = timer;

            if (timer < 0)
            {
                timer = coolDown.timePerCharge;
                coolDown.charges++;
            }
        }

        coolDown.progress = 1;

        coolDown.isActive = false;
    }
    public void ActivateGlobalCoolDown()
    {
        if (!coolDown.usesGlobal) return;

        abilityManager.OnGlobalCoolDown?.Invoke(this);
    }
    public void ClearTargets()
    {
        target = null;
        targetLocked = null;
    }



    
    

    

    



}
