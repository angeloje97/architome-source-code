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
    public Sprite abilityIcon;

    public List<GameObject> buffs;
    public BuffProperties buffProperties;

    public AbilityManager abilityManager;
    public EntityInfo entityInfo;
    public Movement movement;
    public CatalystInfo catalystInfo;
    public LineOfSight lineOfSight;

    public AbilityType abilityType;
    public AbilityType2 abilityType2;

    public GameObject target;
    public GameObject targetLocked;
    public Vector3 location;
    public Vector3 locationLocked;
    public Vector3 directionLocked;

    [Serializable]
    public struct SummoningProperty
    {
        public bool enabled;
        public List<GameObject> summonableEntities;

        [Header("Summoning Settings")]
        public float radius;
        public float liveTime;
        public float valueContributionToStats;
        public Stats additiveStats;

        [Header("Death Settings")]
        public bool masterDeath;
        public bool masterCombatFalse;
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

    [Serializable]
    public struct Cataling
    {
        public bool enable;
        public GameObject catalyst;
        public AbilityType catalingType;
        public CatalystEvent releaseCondition;
        public int releasePerInterval;
        public float interval, targetFinderRadius, valueContribution, rotationPerInterval, startDelay;
    }

    public Cataling cataling;

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
    public struct Threat
    {

        public float additiveThreatMultiplier;

        public bool setsThreat;
        public float threatSet;

        public bool clearThreat;
    }

    public Threat threat;

    [Serializable]
    public struct AbilityVisualEffects
    {
        public bool showCastBar;
        public bool showChannelBar;
        public bool showTargetIconOnTarget;
        public bool activateWeaponParticles;
    }

    public AbilityVisualEffects vfx;


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

    public ChannelProperties channel;

    [Header("Recastable Propertiers")]
    public float canRecast;
    public float recasted;

    [Header("Return Properties")]
    public bool returnAppliesBuffs;
    public bool returnAppliesHeals;
    public bool returns;

    [Serializable]
    public struct Bounce
    {
        public bool enable, requireLOS;
        public float radius;
    }

    public Bounce bounce;

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
        public bool destroysSummons;
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
    public bool destroysSummons;
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
    public float castTimer;
    public bool canceledCast;
    public bool isCasting;
    public bool timerPercentActivated;
    public float progress;
    public float progressTimer;


    [Serializable]
    public struct Tracking
    {
        public bool tracksTarget;
        public bool predictsTarget;
        public bool predicting;

        [Range(0, 1)]
        public float trackingInterpolation;
    }

    public Tracking tracking;

    [Header("Lock On Behaviors")]
    [SerializeField] private bool wantsToCast;
    public bool isAutoAttacking;
    public void GetDependencies()
    {
        entityInfo = GetComponentInParent<EntityInfo>();
        if(abilityManager == null)
        {
            if(GetComponentInParent<AbilityManager>())
            {
                abilityManager = GetComponentInParent<AbilityManager>();
                abilityManager.OnTryCast += OnTryCast;
                abilityManager.OnGlobalCoolDown += OnGlobalCoolDown;
            }
        }
        if(entityInfo)
        {
            movement = entityInfo.Movement();
            lineOfSight = entityInfo.LineOfSight();

            entityObject = entityInfo.gameObject;

            entityInfo.OnChangeNPCType += OnChangeNPCType;
        }

        if(movement)
        {
            movement.OnStartMove += OnStartMove;
            movement.OnEndMove += OnEndMove;
            movement.OnTryMove += OnTryMove;
            movement.OnNewPathTarget += OnNewPathTarget;
        }

        catalystInfo = catalyst.GetComponent<CatalystInfo>();

        if (catalystInfo)
        {
            abilityIcon = catalystInfo.catalystIcon ? catalystInfo.catalystIcon : abilityIcon;
        }

        if(coolDown.maxChargesOnStart)
        {
            coolDown.charges = coolDown.maxCharges;
        }

        UpdateAbility();
    }
    void Start()
    {
        GetDependencies();
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
            DeactivateWantsToCast("Player Try Moving", true);
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
            DeactivateWantsToCast("From NPCType change", true);
            ClearTargets();
        }
    }
    public void OnNewPathTarget(Movement movement, Transform before, Transform after)
    {
        if (abilityType != AbilityType.LockOn) return;
        if(target && after == target.transform) { return; }
        if (wantsToCast)
        {
            DeactivateWantsToCast("From New Path Target", true);
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
        if (!SpawnHasLineOfSight()) return false;

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
                ActivateWantsToCast("Canceling Auto Attack");
                abilityManager.currentlyCasting = null;
                return true;
            }

            return false;

        }
    }

    bool SpawnHasLineOfSight()
    {
        if (abilityType != AbilityType.Spawn) return true;

        

        if (locationLocked == new Vector3()) return false;

        var distance = Vector3.Distance(locationLocked, transform.position);

        if (distance > catalystInfo.range)
        {
            locationLocked = Vector3.Lerp(transform.position, locationLocked, catalystInfo.range / distance);
        }

        if (!lineOfSight) return true;
        if (!requiresLineOfSight) return true;
        //if (!V3Helper.IsAboveGround(locationLocked, GMHelper.LayerMasks().walkableLayer, 2f))
        //{
        //}

        


        if (!lineOfSight.HasLineOfSight(locationLocked))
        {

            locationLocked = V3Helper.InterceptionPoint(locationLocked, transform.position, LayerMasksData.active.wallLayer, 2f);
            return true;
            if (movement)
            {
                
                //movement.MoveTo(locationLocked, 0f);
            }

            ActivateWantsToCast("Spawner Does not have line of sight.");

            return false;
        }


        return true;
    }

    bool HandleRequiresTargetLocked()
    {
        if (location.x == 0 && location.z == 0)
        {
            location = Mouse.RelativePosition(entityObject.transform.position);
        }

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

        

    }

    bool IsInRange()
    {
        if (range == -1)
        {
            return true;
        }


        if (abilityType == AbilityType.LockOn && target == null) return false;

        if (V3Helper.Distance(target.transform.position, entityObject.transform.position) > range)
        {
            movement.MoveTo(target.transform, range);
            ActivateWantsToCast("Target is out of range");
            return false;
        }
        return true;
    }

    bool HasLineOfSight()
    {
        if (range == -1) return true;
        if (!lineOfSight)
        {
            return true;
        }

        if (!requiresLineOfSight)
        {
            return true;
        }

        if (abilityType == AbilityType.LockOn && target == null) return false;

        if (!lineOfSight.HasLineOfSight(target))
        {
            if (movement)
            {
                movement.MoveTo(target.transform);
                ActivateWantsToCast("Target is out of line of sight.");
            }
            return false;
        }
        return true;
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
        if (catalystInfo == null) return false;
        var distance = catalystInfo.range;
        if (target == null) return false;

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
    public bool EndCast()
    {
        if (abilityType == AbilityType.LockOn)
        {
            if(!IsInRange() || !HasLineOfSight())
            {
                return false;
            }
        }

        abilityManager.OnCastRelease?.Invoke(this);

        HandleResources();
        HandleAbilityType();
        HandleFullHealth();
        HandleCastMovementSpeed();
        SetRecast();

        return true;

        void HandleResources()
        {
            
            coolDown.charges--;
            timerPercentActivated = false;
            UseMana();
            HandleResourceProduction();

            void HandleResourceProduction()
            {
                entityInfo.GainResource(manaProduced);
                entityInfo.GainResource(manaProducedPercent * entityInfo.maxMana);
            }

            
        }
        //bool IsInRange()
        //{
        //    if (abilityType != AbilityType.LockOn) return true;
        //    if(!requiresLockOnTarget)
        //    {
        //        return true;
        //    }

        //    if(targetLocked == null)
        //    {
        //        return false;
        //    }

        //    if (range == -1) return true;

        //    if(V3Helper.Distance(entityObject.transform.position, targetLocked.transform.position) > range)
        //    {
        //        ActivateWantsToCast();
        //        return false;
        //    }
        //    return true;
        //}
        //bool HasLineOfSight()
        //{
        //    if (abilityType != AbilityType.LockOn) return true;
        //    if (!requiresLockOnTarget) { return true; }
        //    if(range == -1) { return true; }
        //    if(!requiresLineOfSight || lineOfSight == null)
        //    {
        //        return true;
        //    }

        //    if(lineOfSight.HasLineOfSight(targetLocked))
        //    {
        //        return true;
        //    }

        //    ActivateWantsToCast();
        //    return false;
        //}
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

        timerPercentActivated = false;

        //EndChannel();
    }
    public void HandleAbilityType()
    {
        if(catalyst) { catalyst.GetComponent<CatalystInfo>().isCataling = false; }
        switch (abilityType)
        {
            case AbilityType.SkillShot: FreeCast(); break;
            case AbilityType.Spawn: Spawn(); break;
            case AbilityType.LockOn: LockOn(); break;
            case AbilityType.Use: Use(); break;
            default: FreeCast(); break;
        }

        void Spawn()
        {
            if (!catalyst)
            {
                return;
            }
            directionLocked = V3Helper.Direction(location, entityObject.transform.position);

            var heightFromGround = V3Helper.HeightFromGround(entityObject.transform.position, GMHelper.LayerMasks().walkableLayer);
            var groundPos = V3Helper.GroundPosition(locationLocked, GMHelper.LayerMasks().walkableLayer);
            locationLocked.y = groundPos.y + heightFromGround;

            catalyst.GetComponent<CatalystInfo>().abilityInfo = this;
            var catalystClone = Instantiate(catalyst, locationLocked, transform.localRotation);



            catalystClone.transform.rotation = V3Helper.LerpLookAt(catalystClone.transform, locationLocked, 1f);

            abilityManager.OnCatalystRelease?.Invoke(this, catalystClone.GetComponent<CatalystInfo>());
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


            var heightFromGround = V3Helper.HeightFromGround(entityObject.transform.position, GMHelper.LayerMasks().walkableLayer);
            var groundPos = V3Helper.GroundPosition(locationLocked, GMHelper.LayerMasks().walkableLayer);
            locationLocked.y = groundPos.y + heightFromGround;




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
    public void ActivateWantsToCast(string reason)
    {
        if (wantsToCast) return;
        if (coolDown.charges <= 0) return;
        Debugger.InConsole(896537, $"{reason}");
        wantsToCast = true;
        abilityManager.wantsToCastAbility = wantsToCast;
        abilityManager.OnWantsToCastChange?.Invoke(this, wantsToCast);
    }
    public bool WantsToCast()
    {
        return wantsToCast;
    }
    public void DeactivateWantsToCast(string reason, bool clearTargets = false)
    {
        if (wantsToCast)
        {

            Debugger.InConsole(4389645, $"{reason}");
            wantsToCast = false;
            abilityManager.wantsToCastAbility = wantsToCast;
            abilityManager.OnWantsToCastChange?.Invoke(this, wantsToCast);

            if (clearTargets)
            {
                ClearTargets();
            }
        }
    }
    public void HandleWantsToCast()
    {
        if(!wantsToCast)
        {
            return;
        }
        
        if (coolDown.charges <= 0)
        {
            DeactivateWantsToCast("No Charges");
            return;
        }

        if (target == null && abilityType == AbilityType.LockOn)
        {
            DeactivateWantsToCast("Target = null", true);
            return;
        }


        if (!target.GetComponent<EntityInfo>().isAlive && !targetsDead) {
            abilityManager.OnDeadTarget?.Invoke(this);
            DeactivateWantsToCast("Target Died", true);
            return; }
        

        if(IsInRange() && HasLineOfSight())
        {
            Cast();
            
        }
    }
    public void UpdateAbility()
    {
        if (abilityManager)
        {
            if (entityInfo)
            {
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
        if (abilityType != AbilityType.LockOn) return;
        if (targetsDead) return;

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
        DeactivateWantsToCast("From Start Cast");
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
                    success = EndCast();
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
                HandleTracking();

                

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
                progressTimer = timer;

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
                HandleTracking();
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

        if (abilityManager.currentlyCasting != this && isAttack)
        {
            LogCancel("Not equal to ability manager currently casting");
            return false;
        }

        if (targetLocked && abilityType == AbilityType.LockOn)
        {
            if (!IsCorrectTarget(targetLocked) || target == null)
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
        if (wantsToCast) return;
        target = null;
        targetLocked = null;
    }
    public void HandleTracking()
    {
        if (targetLocked == null) return;

        if (tracking.tracksTarget)
        {
            locationLocked = Vector3.Lerp(locationLocked, targetLocked.transform.position, tracking.trackingInterpolation);
        }

        if (tracking.predictsTarget)
        {
            PredictTarget(targetLocked);
        }

        async void PredictTarget(GameObject target)
        {
            if (tracking.predicting) return;

            tracking.predicting = true;
            var movement = target.GetComponentInChildren<Movement>();
            while (isCasting)
            {
                var distance = V3Helper.Distance(target.transform.position, entityObject.transform.position);

                var travelTime = distance / catalystInfo.speed;

                locationLocked = target.transform.position + (travelTime * movement.velocity);

                await Task.Yield();
            }

            tracking.predicting = false;


        }
    }
    public float Radius(RadiusType type)
    {
        return type switch
        {
            RadiusType.Catalyst => catalystInfo.range,
            RadiusType.Bounce => bounce.radius,
            RadiusType.Buff => buffProperties.radius,
            RadiusType.Splash => splash.radius,
            RadiusType.Cataling => cataling.targetFinderRadius,
            RadiusType.Detection => lineOfSight != null ? lineOfSight.radius: 0f,
            _ => 0f,
        };
    }











}
