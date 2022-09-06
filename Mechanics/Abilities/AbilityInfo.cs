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
    [SerializeField] int id;
    public int _id
    {
        get
        {
            return idSet ? id : 99999;
        }
    }
    bool idSet;
    public void SetId(int id, bool forceId = false)
    {
        if (idSet && !forceId) return;
        idSet = true;
        this.id = id;
    }

    public string abilityName;
    [Multiline]
    public string abilityDescription;
    
    public GameObject entityObject;
    public GameObject catalyst;
    public Sprite abilityIcon;
    
    public List<GameObject> buffs;
    public List<ItemData> augmentsData;
    public Dictionary<AugmentItem, Augment> augments;
    //public BuffProperties buffProperties;

    public AbilityManager abilityManager;
    public EntityInfo entityInfo;
    public Movement movement;
    public CatalystInfo catalystInfo;
    public LineOfSight lineOfSight;

    public AbilityType abilityType;
    public AbilityType2 abilityType2;
    public Role appropriateRole;
    public ClassType appropriateClassType;

    public EntityInfo target;
    public EntityInfo targetLocked;
    public Vector3 location;
    public Vector3 locationLocked;
    public Vector3 directionLocked;

    [Header("Catalyst Info")]
    public float speed = 35;
    public float range = 15;
    public float castTime = 1f;


    public AugmentProp.Restrictions restrictions;
    public AugmentProp.DestroyConditions destroyConditions;
    public AugmentProp.RecastProperties recastProperties;
    public AugmentProp.Threat threat;

    [Serializable]
    public class StatsContribution
    {
        public float strength, wisdom, dexterity = 1f;
    }
    


    public StatsContribution coreContribution;

    [Header("Ability Properties")]

    public float knockBackValue = 1.0f;
    public float baseValue = 5.0f;
    public float value;
    public float originalCastTime = 1f;
    public float liveTime = 3;
    public int ticksOfDamage = 1;

    [Serializable]
    public struct AbilityVisualEffects
    {
        public bool showCastBar;
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


        public float ManaRequired(float maxMana)
        {
            return requiredAmount + (maxMana * requiredPercent);
        }

        public float ManaProduced(float maxMana)
        {
            return producedAmount + (producedPercent * maxMana);
        }
        public void Initiate(AbilityInfo ability)
        {
            ability.OnReadyCheck += OnReadyCheck;
            ability.OnSuccessfulCast += OnSuccessfulCast;
        }

        void OnReadyCheck(AbilityInfo ability, List<(string, bool)> readyChecks)
        {
            var entity = ability.entityInfo;
            if (entity == null) return;


            readyChecks.Add(("Sufficient Resources", entity.mana >= ManaRequired(entity.maxMana)));
        }

        void OnSuccessfulCast(AbilityInfo ability)
        {
            var entity = ability.entityInfo;
            if (entity == null) return;
            entity.Use(ManaRequired(entity.maxMana));
            entity.GainResource(ManaProduced(entity.maxMana));
        }
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
        public bool interruptConsumesCharge;
        public int charges;
        public int maxCharges = 1;

        public void Initiate(AbilityInfo ability)
        {
            if (interruptConsumesCharge)
            {
                ability.OnInterrupt += OnInterrupt;
            }

            ability.OnReadyCheck += OnReadyCheck;
            ability.OnSuccessfulCast += OnSuccesfulCast;
        }

        void OnInterrupt(AbilityInfo ability)
        {
            if (charges > 0)
            {
                charges -= 1;
            }
        }
        void OnReadyCheck(AbilityInfo ability, List<(string, bool)> andChecks)
        {
            andChecks.Add(("Sufficient charges", charges > 0));
        }

        void OnSuccesfulCast(AbilityInfo ability)
        {
            charges--;
        }
    }
    public CoolDownProperties coolDown;


    [Header("Scanner Properties")]
    public bool scannerRadialAoe;

    [Header("Grow Properties")]
    public bool grows;
    public bool growsForward;
    public bool growsWidth;

    [Header("Return Properties")]
    public bool returnAppliesBuffs;
    public bool returnAppliesHeals;
    public bool returns;


    [Header("Hold to Cast Properties")]
    public float holdToCastValue;

    [Header("Movement Casting Properties")]
    public float castingMovementSpeedReduction;
    public bool cancelCastIfMoved;
    public bool cantMoveWhenCasting;

    //[Header("AbilityRestrictions")]
    public bool activated; //If the ability is casting or channeling.

    [Header("Legacy Restrictions(Still in Use)")]
    [SerializeField] bool validate;
    public void OnValidate()
    {

        //restrictions.UpdateSelf(this);
        //UnityEditor.EditorUtility.SetDirty(this);   
        //var restrictionFields = restrictions.GetType().GetFields();
        
        //foreach (var field in this.GetType().GetFields())
        //{
        //    foreach (var restrictionField in restrictionFields)
        //    {
        //        if (field.Name != restrictionField.Name) continue;

        //        var resValue = restrictionField.GetValue(restrictions);
        //        var abilityValue = field.GetValue(this);

        //        if (resValue.GetType() != typeof(bool)) continue;
        //        if (abilityValue.GetType() != typeof(bool)) continue;

        //        restrictionField.SetValue(restrictions, abilityValue);
        //    }
        //}
    }
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
    public bool active; //State if the ability can be casted.
    public bool usesWeaponAttackDamage { get; set; }

    [Header("Ability Timers")]
    public float castTimer;
    public bool canceledCast;
    public bool isCasting;
    public bool timerPercentActivated;
    public float progress;
    public float progressTimer;


    [Header("Lock On Behaviors")]
    [SerializeField] private bool wantsToCast;
    public bool isAutoAttacking;

    public Action<CatalystInfo> OnCatalystRelease;
    public Action<AbilityInfo> OnSuccessfulCast;
    public Action<AbilityInfo, bool> OnAbilityStartEnd;
    public Action<AbilityInfo> OnUpdateRestrictions;
    public Action<AbilityInfo> WhileCasting;
    public Action<AbilityInfo, List<(string, bool)>> OnReadyCheck { get; set; }
    public Action<AbilityInfo, List<bool>> OnBusyCheck;
    public Action<AbilityInfo, EntityInfo, List<bool>, List<bool>> OnCorrectTargetCheck;
    public Action<AbilityInfo> OnInterrupt;
    public List<AugmentType> augmentAbilities;



    //Augments
    public AugmentChannel currentChannel { get; set; }
    public void GetDependencies()
    {
        entityInfo = GetComponentInParent<EntityInfo>();


        abilityManager = GetComponentInParent<AbilityManager>();
        if (abilityManager)
        {
            abilityManager = GetComponentInParent<AbilityManager>();
            abilityManager.OnTryCast += OnTryCast;
            abilityManager.OnGlobalCoolDown += OnGlobalCoolDown;
            
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

        if (catalyst)
        {
            catalystInfo = catalyst.GetComponent<CatalystInfo>();

            if (catalystInfo)
            {
                abilityIcon = catalystInfo.catalystIcon ? catalystInfo.catalystIcon : abilityIcon;
            }

            if(coolDown.maxChargesOnStart)
            {
                coolDown.charges = coolDown.maxCharges;
            }

        }

        UpdateAbility();
    }
    void Start()
    {
        GetDependencies();
        HandleInitiates();
        
        active = true;

        void HandleInitiates()
        {
            coolDown.Initiate(this);
            resources.Initiate(this);
        }
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


    // Augments
    public async void AddAugment(AugmentItem augmentItem)
    {
        if (augments == null) augments = new();
        if (augments.ContainsKey(augmentItem))
        {

            await Task.Yield();

            if (augments.ContainsKey(augmentItem))
            {
                return;
            }
        }

        var newAugment = Instantiate(augmentItem.augment, transform);

        newAugment.spawnedByItem = true;        

        newAugment.gameObject.name = augmentItem.itemName;
        
        augments.Add(augmentItem, newAugment);
    }

    public void RemoveAugment(AugmentItem augmentItem)
    {
        if (!augments.ContainsKey(augmentItem)) return;
        augments[augmentItem].RemoveAugment();

        augments.Remove(augmentItem);

    }
    public string Name()
    {
        if (abilityName.Length == 0)
        {
            if (name.Length == 0)
            {
                if (catalyst)
                {
                    return catalyst.name;
                }
            }

            return name;

        }

        return abilityName;
    }

    public Sprite Icon()
    {
        //if (abilityType2 == AbilityType2.AutoAttack && abilityManager.settings.useWeaponAbility)
        //{
        //    if (abilityManager.currentWeapon && abilityManager.currentWeapon.itemIcon)
        //    {
        //        return abilityManager.currentWeapon.itemIcon;
        //    }
        //}

        if (abilityIcon != null)
        {
            return abilityIcon;
        }


        if (catalyst)
        {
            var info = catalyst.GetComponent<CatalystInfo>();

            return info.Icon();
        }

        return abilityIcon;
    }
    public string Description()
    {
        var description = "";

        if (abilityDescription != null && abilityDescription.Length > 0)
        {
            description += $"{abilityDescription}\n";
        }

        return description;
    }
    public string PropertiesDescription()
    {
        string properties = "";

        if (!nullifyDamage && (isHarming || isHealing))
        {
            
            if (isHealing)
            {
                properties = "Heals health ";
                if (isHarming)
                {
                    properties += "or ";
                }
            }

            if (isHarming)
            {
                properties += "Deals damage ";
            }

            properties += "equal to ";

            if (entityInfo)
            {
                properties += $"{value} value. ";
            }

            var listString = new List<string>();

            if (coreContribution.dexterity > 0)
            {
                listString.Add($"{ArchString.FloatToSimple(coreContribution.dexterity*100)}% Dexterity");
            }

            if (coreContribution.strength > 0)
            {
                listString.Add($"{ArchString.FloatToSimple(coreContribution.strength*100)}% Strength");
            }

            if (coreContribution.wisdom > 0)
            {
                listString.Add($"{ArchString.FloatToSimple(coreContribution.wisdom*100)}% Wisdom");
            }

            

            properties += $"({ArchString.StringList(listString)}).\n";
        }

        if (ticksOfDamage > 1)
        {
            properties += $"Base Ticks: {ticksOfDamage}";
        }

        return properties;
    }
    public string ResourceDescription()
    {
        string resourceDescription = "";
        var info = catalyst.GetComponent<CatalystInfo>();

        if (vfx.showCastBar && info)
        {
            var castTime = info.castTime;

            var alteredCastTime = castTime - (castTime * Haste());
            resourceDescription += $"{ArchString.FloatToSimple(alteredCastTime)}s Cast ";
        }

        if (resources.requiredAmount > 0 || resources.requiredPercent > 0)
        {
            var requiredAmount = $"";
            if (resources.requiredAmount > 0)
            {
                requiredAmount += $"{resources.requiredAmount}";

                if (resources.requiredPercent > 0)
                {
                    requiredAmount += " + ";
                }
            }

            if (resources.requiredPercent > 0)
            {
                requiredAmount += $"({resources.requiredPercent * 100}%)";
            }

            resourceDescription += $"{requiredAmount} mana required ";
        }

        if (resources.producedAmount > 0 || resources.producedPercent > 0)
        {
            var producedAmount = "";

            if (resources.producedAmount > 0)
            {
                producedAmount += $"{resources.producedAmount}";

                if (resources.producedPercent > 0)
                {
                    producedAmount += " + ";
                }
            }

            if (resources.producedPercent > 0)
            {
                producedAmount += $"({resources.producedPercent*100}%)";
            }

            resourceDescription += $"{producedAmount} mana produced ";
        }

        if (resourceDescription.Length > 0)
        {
            resourceDescription += "\n";
        }

        return resourceDescription;
    }
    public string RestrictionDescription()
    {
        var properties = "";

        if (destroysSummons)
        {
            properties += $"Can destroy any unit that is summoned by this target with this ability\n";
        }

        if (onlyCastOutOfCombat)
        {
            properties += "Can only cast out of combat. \n";
        }

        if (onlyCastSelf)
        {
            properties += $"Can only target self.\n";
        }

        UpdateFromCatalyst();

        return properties;

        void UpdateFromCatalyst()
        {
            if (catalyst == null) return;
            var info = catalyst.GetComponent<CatalystInfo>();
            if (info == null) return;
            if (!info.requiresWeapon) return;

            properties += $"Requires weapon : {ArchString.CamelToTitle(info.weaponType.ToString())}.\n";

        }
    }
    public string BuffList()
    {
        var buffDescription = "";

        if (buffs.Count > 0)
        {
            var assistBuffs = new List<BuffInfo>();
            var harmBuffs = new List<BuffInfo>();
            var selfBuffs = new List<BuffInfo>();

            foreach (var buff in buffs)
            {
                var info = buff.GetComponent<BuffInfo>();
                if (info == null) continue;

                var list = assistBuffs;


                if (info.buffTargetType == BuffTargetType.Harm)
                {
                    list = harmBuffs;
                }

                list.Add(info);
            }

            if (assistBuffs.Count > 0)
            {
                buffDescription += $"Applies buff(s): ";

                var list = new List<string>();

                foreach (var buff in assistBuffs)
                {
                    list.Add(buff.name);
                }


                buffDescription += $"{ArchString.StringList(list)}\n";
            }

            if (harmBuffs.Count > 0)
            {
                buffDescription += $"Applies debuff(s) to enemies: ";

                var list = new List<string>();

                foreach (var buff in harmBuffs)
                {
                    list.Add(buff.name);
                }

                buffDescription += $"{ArchString.StringList(list)}\n";
            }

            if (selfBuffs.Count > 0)
            {
                buffDescription += $"Applies buff(s) to self: ";

                var list = new List<string>();

                foreach (var buff in selfBuffs)
                {
                    list.Add(buff.name);
                }


                buffDescription += $"{ArchString.StringList(list)}\n";
            }
        }

        return buffDescription;
    }
    public string BuffDescriptions()
    {
        var buffDescription = "";
        var entityExists = entityInfo != null;
        foreach (var buff in buffs)
        {
            var info = buff.GetComponent<BuffInfo>();

            if (info == null) continue;

            var time = info.properties.time > 0 ? $"({info.properties.time}s)" : "";

            var name = info.name + time;

            var generalDescription = "";
            

            if (!entityExists)
            {
                generalDescription += info.TypesDescriptionGeneral();
            }
            else
            {
                generalDescription += info.TypeDescriptionFace(value);
            }

            generalDescription += $"{info.PropertiesDescription()}";

            if (generalDescription.Length == 0) continue;

            buffDescription += $"{name}: {generalDescription}";
        }

        return buffDescription;
    }

    public string AugmentDescriptions()
    {
        var result = "";

        foreach (Transform child in transform)
        {
            var augment = child.GetComponent<Augment>();
            if (augment == null) continue;
            if (augment.spawnedByItem) continue;


            var description = $"{augment.name} : {augment.TypeDescription()}";

            if (result.Length > 0 && description.Length > 0)
            {
                result += "\n";
            }

            result += description;
        }

        return result;
    }
    public ToolTipData ToolTipData(bool showResources = true)
    {
        var attributesList = new List<string>()
        {
            PropertiesDescription(),
            BuffDescriptions(),
            AugmentDescriptions(),
        };

        var attributes = "";

        foreach (var attribute in attributesList)
        {
            if (attributes.Length > 0 && attribute.Length > 0)
            {
                attributes += "\n";
            }

            attributes += $"{attribute}";
        }

        var value = showResources ? ResourceDescription() : "";

        return new() {
            icon = Icon(),
            subeHeadline = ArchString.CamelToTitle(abilityType2.ToString()),
            name = Name(),
            description = Description(),
            attributes = attributes,
            requirements = RestrictionDescription(),
            value = value
        };

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

        var busyList = new List<bool>();

        OnBusyCheck?.Invoke(this, busyList);


        foreach (var busy in busyList)
        {
            if (busy) return true;
        }

        return false;
    }

    public bool IsChanneling()
    {
        if (currentChannel == null) return false;

        return currentChannel.active;
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
        if (!recastProperties.enabled) return;
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
        if (!IsReady())
        {
            CantCastReason("Not Ready");
            return false;
        }
        if (IsBusy())
        {
            CantCastReason("Is Busy");
            return false;
        }
        if (!AbilityManagerCanCast())
        {
            CantCastReason("Ability Manager Can't Cast");
            return false;
        }
        if (!HasManaRequired())
        {
            CantCastReason("Not Enough Mana");
            return false;
        }
        if (!HandleRequiresTargetLocked())
        {
            CantCastReason("Incorrect Target");
            return false;
        }
        if (!CorrectCombat())
        {
            CantCastReason("Not in correct combat state.");
            return false;
        }
        if (!SpawnHasLineOfSight())
        {
            CantCastReason("Don't have line of sight");
            return false;
        }

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

        void CantCastReason(string reason)
        {
            Debugger.InConsole(5329, $"{this} can't cast because {reason}");
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

            locationLocked = V3Helper.InterceptionPoint(locationLocked, transform.position, LayerMasksData.active.structureLayerMask, 2f);
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

        if (abilityType != AbilityType.LockOn) return true;

        if (onlyCastSelf || abilityType == AbilityType.Use)
        {
            target = entityInfo;
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
    bool IsInRange(float offset = 0f)
    {
        if (range == -1)
        {
            return true;
        }


        if (abilityType == AbilityType.LockOn && target == null) return false;

        if (V3Helper.Distance(target.transform.position, entityObject.transform.position) > range + offset)
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

        if (!lineOfSight.HasLineOfSight(target.gameObject))
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
            if (entityInfo.mana >= resources.requiredAmount + entityInfo.maxMana * resources.requiredPercent)
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
            var offset = range * .25f;
            if(!IsInRange(offset) || !HasLineOfSight())
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
            
            //coolDown.charges--;
            timerPercentActivated = false;
            //UseMana();
            //HandleResourceProduction();

            //void HandleResourceProduction()
            //{
            //    entityInfo.GainResource(resources.producedAmount);
            //    entityInfo.GainResource(resources.producedPercent * entityInfo.maxMana);
            //}

            
        }

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
    public void HandleAbilityType()                                 //This is where the magic happens.. IE where the catalyst gets released.
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


            catalystInfo.abilityInfo = this;
            var catalystClone = Instantiate(catalyst, locationLocked, transform.localRotation);



            catalystClone.transform.rotation = V3Helper.LerpLookAt(catalystClone.transform, locationLocked, 1f);

            var newCatalyst = catalystClone.GetComponent<CatalystInfo>();
            //newCatalyst.abilityInfo = this;

            abilityManager.OnCatalystRelease?.Invoke(this, newCatalyst);
            OnCatalystRelease?.Invoke(newCatalyst);
        }

        void FreeCast()
        {
            if(!catalyst)
            {
                return;
            }
            directionLocked = V3Helper.Direction(location, entityObject.transform.position);
            catalystInfo.abilityInfo = this;
            var catalystClone = Instantiate(catalyst, entityObject.transform.position, transform.localRotation);


            var heightFromGround = V3Helper.HeightFromGround(entityObject.transform.position, GMHelper.LayerMasks().walkableLayer);
            var groundPos = V3Helper.GroundPosition(locationLocked, GMHelper.LayerMasks().walkableLayer);
            locationLocked.y = groundPos.y + heightFromGround;

            var newCatalyst = catalystClone.GetComponent<CatalystInfo>();
            //newCatalyst.abilityInfo = this;

            abilityManager.OnCatalystRelease?.Invoke(this, newCatalyst);
            OnCatalystRelease?.Invoke(newCatalyst);


        }
        void LockOn()
        {
            if(!catalyst)
            {
                return;
            }

            catalystInfo.abilityInfo = this;
            var catalystClone = Instantiate(catalyst, entityObject.transform.position, transform.localRotation);


            var newCatalyst = catalystClone.GetComponent<CatalystInfo>();
            //newCatalyst.abilityInfo = this;
            abilityManager.OnCatalystRelease?.Invoke(this, newCatalyst);
            OnCatalystRelease?.Invoke(newCatalyst);
        }
        void Use()
        {
            if (!catalyst) { return; }

            //catalyst.GetComponent<CatalystInfo>().abilityInfo = this;
            catalystInfo.abilityInfo = this;
            var catalystClone = Instantiate(catalyst, entityObject.transform.position, transform.localRotation);

            var newCatalyst = catalystClone.GetComponent<CatalystInfo>();

            abilityManager.OnCatalystRelease?.Invoke(this, newCatalyst);
            OnCatalystRelease?.Invoke(newCatalyst);
        }
    }
    public bool IsCorrectTarget(EntityInfo target)
    {
        var info = target;
        if(target == null) { return true; }


        if(!canCastSelf)
        {
            if (info == entityInfo)
            {
                return false;    
            }
        }

        var orChecks = new List<bool>();
        var andChecks = new List<bool>();


        OnCorrectTargetCheck?.Invoke(this, target, orChecks, andChecks);
        foreach(var check in orChecks) { if (check) return true; }
        foreach(var check in andChecks) { if (!check) return false; }

        if(!info.isAlive && !targetsDead)
        {
            abilityManager.OnDeadTarget?.Invoke(this);
            return false; 
        }
        if(info.isAlive && targetsDead) { return false; }

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

        return false;
    }
    //public void UseMana()
    //{
    //    if(entityInfo)
    //    {
    //        entityInfo.Use(resources.requiredAmount + entityInfo.maxMana*resources.requiredPercent);
    //    }
    //}
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
                if (usesWeaponAttackDamage && abilityManager.currentWeapon)
                {
                    value = abilityManager.currentWeapon.attackValue;
                }
                else
                {
                    value = AbilityValue();
                }
                

            }
        }

        
        if (catalyst)
        {
            catalystInfo = catalyst.GetComponent<CatalystInfo>();

            abilityIcon = catalystInfo.catalystIcon;
            range = catalystInfo.range;
            speed = catalystInfo.speed;

            var isAttack = abilityType2 == AbilityType2.AutoAttack;

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
                requiresLineOfSight = false;
                requiresLockOnTarget = false;
            }

            if (abilityType == AbilityType.LockOn)
            {
                requiresLineOfSight = true;
                requiresLockOnTarget = true;
            }

        }
    }

    float AbilityValue()
    {
        var value = baseValue
                        + entityInfo.stats.Wisdom * coreContribution.wisdom
                        + entityInfo.stats.Strength * coreContribution.strength
                        + entityInfo.stats.Dexterity * coreContribution.dexterity;

        return value;
    }
    public bool IsReady()
    {
        if (!active)
        {
            NotReadyReason("not active");
            return false;
        }



        //if (isCasting)
        //{
        //    NotReadyReason("Already casting");
        //    return false;
        //}
        //if (channel.active)
        //{
        //    NotReadyReason("Already channeling");
        //    return false;
        //}

        if (IsBusy())
        {
            NotReadyReason("Already casting or already channeling");
            return false;
        }
        if (coolDown.globalCoolDownActive)
        {
            NotReadyReason("Global Cooldown is still active");
            return false;
        }
        if (!HasCorrectWeapon())
        {
            return false;
        }

        if(entityInfo.mana < (resources.requiredAmount + resources.requiredPercent*entityInfo.maxMana))
        {
            NotReadyReason("Not enough resources");
            return false;
        }

        //if (coolDown.charges <= 0)
        //{
        //    NotReadyReason("still on cooldown");
        //    return false;
        //}

        var readyList = new List<(string, bool)>();

        OnReadyCheck?.Invoke(this, readyList);

        foreach (var (reason, success) in readyList)
        {
            if (!success)
            {
                NotReadyReason(reason);
                return false;

            }
        }

        return true;

        bool HasCorrectWeapon()
        {
            if (!catalystInfo.requiresWeapon) return true;
            
            var hasWeapon = abilityManager.HasWeaponType(catalystInfo.weaponType);

            if (hasWeapon == false)
            {
                NotReadyReason($"Ability requires {catalystInfo.weaponType}");
            }

            return hasWeapon;
        }

        void NotReadyReason(string reason)
        {
            Debugger.InConsole(5330, $"{this} can't cast because {reason}");
        }
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
                    //CancelChannel();
                    DeactivateWantsToCast("Target is Dead", true);
                }
            }
        }
    }
    public void Use()
    {
        if(abilityManager)
        {
            abilityManager.Cast(this, true);
            abilityManager.OnTryCast?.Invoke(this);
        }
    }
    public void CastAt(Vector3 position)
    {
        if (IsBusy()) return;
        if (GetComponentInParent<AbilityManager>() == null) { return; }
        location = position;
        Cast();
    }
    public void CastAt(EntityInfo target)
    {
        if (IsBusy()) return;
        if(abilityManager == null) { return; }
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
        OnAbilityStartEnd?.Invoke(this, true);

        bool success = await Casting();

        if (success)
        {
            OnSuccessfulCast?.Invoke(this);

            ActivateGlobalCoolDown();
            await AugmentAbilities();
            SetRecast();
        }



        if (abilityManager.currentlyCasting == this)
        {
            abilityManager.currentlyCasting = null;
            abilityManager.OnAbilityEnd?.Invoke(this);
        }

        OnAbilityStartEnd?.Invoke(this, false);

        SetCoolDownTimer();
        //SetCoolDownTimer();
        

        if (!isAutoAttacking)
        {
            ClearTargets();
        }
        

        activated = false;

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
                WhileCasting?.Invoke(this);
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

        async Task AugmentAbilities()
        {
            if (augmentAbilities == null) return;
            foreach (var augment in augmentAbilities)
            {
                var success = await augment.Ability();
                if (!success) break;
            }
        }

        //async Task Channeling()
        //{
        //    if (!channel.enabled) return;
        //    if (channel.active) return;


        //    StartChannel();

        //    var timer = channel.time - channel.time * Haste();
        //    var startTime = timer;


        //    Debugger.InConsole(3245, $"Timer is {timer}");

        //    float progressPerInvoke = (1f / channel.invokeAmount);
        //    float progressBlock = 1 - progressPerInvoke;

        //    while (timer > 0)
        //    {
        //        await Task.Yield();
        //        timer -= Time.deltaTime;
        //        progress = (timer / startTime);
        //        progressTimer = timer;

        //        WhileChanneling();

        //        if (!CanContinue())
        //        {
        //            break;
        //        }

        //        if (progressBlock > progress)
        //        {
        //            HandleAbilityType();
        //            abilityManager.OnChannelInterval?.Invoke(this);
        //            progressBlock -= progressPerInvoke;
        //        }

        //    }

        //    EndChannel();

            

        //    void StartChannel()
        //    {
        //        progress = 1;
        //        channel.active = true;
        //        abilityManager.OnChannelStart?.Invoke(this);
        //    }

        //    void WhileChanneling()
        //    {
        //        abilityManager.WhileCasting?.Invoke(this);
        //        HandleTracking();
        //        if (isAttack) return;


        //        if (targetLocked != target)
        //        {
        //            targetLocked = target;
        //        }
        //    }

        //    void EndChannel()
        //    {
        //        progress = 0;
        //        channel.active = false;
        //        abilityManager.OnChannelEnd?.Invoke(this);
        //    }
        //}
    }

    public async Task<AbilityInfo> EndActivation()
    {
        while (activated)
        {
            await Task.Yield();
        }

        return this;
    }
    public bool HasChannel()
    {
        return GetComponentInChildren<AugmentChannel>() != null;
    }
    public float Haste()
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

        //if (channel.active && channel.cancel)
        //{
        //    LogCancel("Canceled Channel");
        //    channel.cancel = false;
        //    return false;
        //}

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
        Debugger.Combat(9536, $"{entityInfo} stopped casting {this} because {reason}");
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
        //if (targetLocked == null) return;

        //if (tracking.tracksTarget)
        //{
        //    locationLocked = Vector3.Lerp(locationLocked, targetLocked.transform.position, tracking.trackingInterpolation);
        //}

        //if (tracking.predictsTarget)
        //{
        //    PredictTarget(targetLocked);
        //}

        //async void PredictTarget(GameObject target)
        //{
        //    if (tracking.predicting) return;

        //    tracking.predicting = true;
        //    var movement = target.GetComponentInChildren<Movement>();
        //    while (isCasting)
        //    {
        //        var distance = V3Helper.Distance(target.transform.position, entityObject.transform.position);

        //        var travelTime = distance / catalystInfo.speed;

        //        locationLocked = target.transform.position + (travelTime * movement.velocity);

        //        await Task.Yield();
        //    }

        //    tracking.predicting = false;


        //}
    }
    public float Radius(RadiusType type)
    {
        return type switch
        {
            RadiusType.Catalyst => catalystInfo.range,
            RadiusType.Buff => BuffRadius(),
            RadiusType.Detection => lineOfSight != null ? lineOfSight.radius: 0f,
            _ => 0f,
        };

        float BuffRadius()
        {
            float largest = 0;
            foreach (var buff in buffs)
            {
                var info = buff.GetComponent<BuffInfo>();

                if (info.properties.radius > largest)
                {
                    largest = info.properties.radius;
                }
            }

            return largest;
        }
    }
}
